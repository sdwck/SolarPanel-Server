const fs = require('fs')
const fsp = require('fs').promises
const path = require('path')
const crypto = require('crypto')
const { spawn } = require('child_process')
const axios = require('axios')

const DEVICE_ID = process.env.DEVICE_ID || '1'

const CONFIG = {
    DEVICE_ID,
    REMOTE_URL: process.env.REMOTE_URL || 'http://localhost:3000/remote/1/script',
    POLL_INTERVAL_MS: parseInt(process.env.POLL_INTERVAL_MS || '60000', 10),
    SCRIPT_DIR: process.env.SCRIPT_DIR || path.resolve(__dirname, 'scripts'),
    SCRIPT_FILE: process.env.SCRIPT_FILE || 'script.js',
    META_FILE: process.env.META_FILE || path.resolve(__dirname, 'state', 'meta.json'),
    SHARED_SECRET: process.env.SHARED_SECRET || '3e79d2e745f1bd52583a7cc45a7729e33680538cd9978d753b767dfad64c6ca3',
    FETCH_TIMEOUT_MS: parseInt(process.env.FETCH_TIMEOUT_MS || '15000', 10),
    HEALTH_WAIT_MS: parseInt(process.env.HEALTH_WAIT_MS || '5000', 10),
    MAX_RESTART_BACKOFF_MS: parseInt(process.env.MAX_RESTART_BACKOFF_MS || '300000', 10),
    LOG_FILE: process.env.LOG_FILE || path.resolve(__dirname, 'updater.log'),
    MAX_FETCH_RETRIES: parseInt(process.env.MAX_FETCH_RETRIES || '5', 10),
    BACKUP_KEEP: parseInt(process.env.BACKUP_KEEP || '3', 10)
}

function log(...parts) {
    const ts = new Date().toISOString()
    const line = [ts, ...parts].map(p => (typeof p === 'string' ? p : JSON.stringify(p))).join(' ')
    console.log(line)
    try { fs.appendFileSync(CONFIG.LOG_FILE, line + '\n') } catch (e) {}
}

async function ensureDirs() {
    await fsp.mkdir(CONFIG.SCRIPT_DIR, { recursive: true })
    await fsp.mkdir(path.dirname(CONFIG.META_FILE), { recursive: true })
}

async function loadMeta() {
    try {
        const raw = await fsp.readFile(CONFIG.META_FILE, 'utf8')
        return JSON.parse(raw)
    } catch (e) {
        return {}
    }
}

async function saveMeta(meta) {
    const tmp = CONFIG.META_FILE + '.tmp.' + Date.now()
    await fsp.writeFile(tmp, JSON.stringify(meta, null, 2), 'utf8')
    await fsp.rename(tmp, CONFIG.META_FILE)
}

function computeHash(buf) {
    return crypto.createHash('sha256').update(buf).digest('hex')
}

function timingSafeEqualStr(a, b) {
    try {
        const A = Buffer.from(a, 'utf8')
        const B = Buffer.from(b, 'utf8')
        if (A.length !== B.length) return false
        return crypto.timingSafeEqual(A, B)
    } catch (e) {
        return false
    }
}

async function atomicWrite(filePath, buffer) {
    const tmp = filePath + '.tmp.' + Date.now()
    await fsp.writeFile(tmp, buffer, { mode: 0o644 })
    await fsp.rename(tmp, filePath)
}

async function rotateBackups(scriptPath, keep) {
    try {
        const dir = path.dirname(scriptPath)
        const base = path.basename(scriptPath)
        const files = await fsp.readdir(dir)
        const bakFiles = files.filter(f => f.startsWith(base + '.bak.')).sort()
        while (bakFiles.length > keep) {
            const toDelete = bakFiles.shift()
            try { await fsp.unlink(path.join(dir, toDelete)) } catch (e) {}
        }
    } catch (e) {}
}

async function fetchRemote(etag, lastModified) {
    const headers = {}
    if (etag) headers['If-None-Match'] = etag
    if (lastModified) headers['If-Modified-Since'] = lastModified
    const resp = await axios.get(CONFIG.REMOTE_URL, {
        headers,
        timeout: CONFIG.FETCH_TIMEOUT_MS,
        responseType: 'arraybuffer',
        validateStatus: s => (s >= 200 && s < 300) || s === 304
    })
    if (resp.status === 304) return { notModified: true }
    const body = Buffer.from(resp.data)
    if (CONFIG.SHARED_SECRET) {
        const signatureHeader = resp.headers['x-signature'] || resp.headers['x-signature-256'] || ''
        const expected = crypto.createHmac('sha256', CONFIG.SHARED_SECRET).update(body).digest('hex')
        if (!signatureHeader || !timingSafeEqualStr(signatureHeader, expected)) {
            throw new Error('invalid signature')
        }
    }
    return {
        content: body,
        etag: resp.headers.etag || null,
        lastModified: resp.headers['last-modified'] || null
    }
}

let currentChild = null
let restartAttempts = 0
let shuttingDown = false

function startScriptInstance(scriptPath) {
    const child = spawn(process.execPath, [scriptPath], { stdio: ['ignore', 'pipe', 'pipe'] })
    child.stdout.on('data', d => log('child:out', d.toString().trim()))
    child.stderr.on('data', d => log('child:err', d.toString().trim()))
    child.on('exit', (code, signal) => {
        log('child exited', { code, signal })
        if (shuttingDown) return
        restartAttempts++
        const backoff = Math.min(1000 * Math.pow(2, restartAttempts), CONFIG.MAX_RESTART_BACKOFF_MS)
        log('scheduling child restart', backoff)
        setTimeout(() => {
            if (shuttingDown) return
            if (!currentChild || currentChild === child) {
                currentChild = startScriptInstance(scriptPath)
            }
        }, backoff)
    })
    child.on('error', err => log('child error', err && err.message))
    return child
}

async function ensureScriptRunning() {
    const scriptPath = path.join(CONFIG.SCRIPT_DIR, CONFIG.SCRIPT_FILE)
    try {
        if (currentChild && currentChild.pid) return
        if (!fs.existsSync(scriptPath)) {
            log('ensureScriptRunning', 'no script file found')
            return
        }
        const child = startScriptInstance(scriptPath)
        await new Promise(r => setTimeout(r, CONFIG.HEALTH_WAIT_MS))
        if (child.exitCode !== null) {
            log('ensureScriptRunning', 'existing script died immediately')
            return
        }
        currentChild = child
        restartAttempts = 0
        log('ensureScriptRunning', 'started existing script', scriptPath)
    } catch (e) {
        log('ensureScriptRunning error', e && e.message)
    }
}

async function applyUpdate(buffer, metaHeaders) {
    const scriptPath = path.join(CONFIG.SCRIPT_DIR, CONFIG.SCRIPT_FILE)
    const newHash = computeHash(buffer)
    const meta = await loadMeta()
    if (meta.hash === newHash) {
        log('no-change', 'hash matches, skipping')
        return
    }
    let backupPath = null
    try {
        if (fs.existsSync(scriptPath)) {
            backupPath = scriptPath + '.bak.' + Date.now()
            await fsp.copyFile(scriptPath, backupPath)
            await rotateBackups(scriptPath, CONFIG.BACKUP_KEEP)
        }
        await atomicWrite(scriptPath, buffer)
        const newChild = startScriptInstance(scriptPath)
        await new Promise(r => setTimeout(r, CONFIG.HEALTH_WAIT_MS))
        if (newChild.exitCode !== null) {
            log('new child died immediately, restoring backup')
            if (backupPath) {
                try {
                    const backupBuf = await fsp.readFile(backupPath)
                    await atomicWrite(scriptPath, backupBuf)
                } catch (er) {
                    log('restore failed', er && er.message)
                }
            } else {
                try { await fsp.unlink(scriptPath) } catch (e) {}
            }
            return
        }
        if (currentChild && currentChild.pid && currentChild !== newChild) {
            try { currentChild.kill('SIGTERM') } catch (e) {}
            setTimeout(() => { try { currentChild.kill('SIGKILL') } catch (e) {} }, 5000)
        }
        currentChild = newChild
        restartAttempts = 0
        const newMeta = Object.assign({}, metaHeaders || {}, { hash: newHash, updatedAt: new Date().toISOString(), source: CONFIG.REMOTE_URL })
        await saveMeta(newMeta)
        log('update applied', newMeta)
    } catch (e) {
        log('applyUpdate error', e && e.message)
        if (backupPath) {
            try {
                const backupBuf = await fsp.readFile(backupPath)
                await atomicWrite(path.join(CONFIG.SCRIPT_DIR, CONFIG.SCRIPT_FILE), backupBuf)
            } catch (er) {
                log('restore failed', er && er.message)
            }
        }
    }
}

async function performFetchOnce() {
    const meta = await loadMeta()
    let tries = 0
    while (tries < CONFIG.MAX_FETCH_RETRIES) {
        try {
            const res = await fetchRemote(meta.etag, meta.lastModified)
            if (res.notModified) {
                log('fetch', 'not modified')
                await ensureScriptRunning()
                return
            }
            await applyUpdate(res.content, { etag: res.etag, lastModified: res.lastModified })
            return
        } catch (e) {
            tries++
            log('fetch error', e && e.message, 'try', tries)
            const wait = Math.min(1000 * Math.pow(2, tries), CONFIG.MAX_RESTART_BACKOFF_MS)
            await new Promise(r => setTimeout(r, wait))
        }
    }
    log('fetch failed after retries')
}

function startPolling() {
    let aborted = false
    async function loop() {
        if (aborted) return
        try {
            await performFetchOnce()
        } catch (e) {
            log('poll loop error', e && e.message)
        }
        if (aborted) return
        setTimeout(loop, CONFIG.POLL_INTERVAL_MS)
    }
    loop()
    return () => { aborted = true }
}

(async function main() {
    if (!CONFIG.REMOTE_URL) process.exit(1)
    await ensureDirs()
    try {
        await performFetchOnce()
    } catch (e) {
        log('startup fetch failed', e && e.message)
    }
    await ensureScriptRunning()
    startPolling()
})()
