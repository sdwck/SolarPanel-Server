const mqtt = require('mqtt');
const { exec } = require('child_process');

const execPromise = (cmd) => {
    return new Promise((resolve, reject) => {
        exec(cmd, (error, stdout) => {
            if (error) return reject(error);
            resolve(stdout);
        });
    });
};

const task = async () => {
    const password = Buffer.from('UGFzc3dvcmQx', 'base64').toString('utf-8');

    const client = mqtt.connect({
        username: 'solar-panel-1',
        password: password,
        protocol: 'mqtts',
        host: '9044eae185c0455d85f985eb24270bc9.s1.eu.hivemq.cloud',
        port: 8883,
        clean: true,
        connectTimeout: 4000,
        rejectUnauthorized: true
    });

    const topic = 'data';

    client.on('connect', () => {
        console.log(`[${new Date().toISOString()}] Connected to MQTT broker`);

        setInterval(async () => {
            let response = {};

            try {
                const qpigsOut = await execPromise('mpp-solar -p /dev/ttyACM0 --porttype serial -c QPIGS -o json');
                const qpigsData = JSON.parse(qpigsOut);
                response = { ...response, ...qpigsData };
            } catch (err) {
                console.error(`[Error QPIGS] ${err.message}`);
            }

            const queries = [
                { cmd: 'QMOD', key: 'mode' },
                { cmd: 'QPIRI', key: 'rating' },
                { cmd: 'QPIWS', key: 'warnings' },
                { cmd: 'QID', key: 'serial' },
                { cmd: 'QVFW', key: 'firmware' },
            ];

            for (const { cmd, key } of queries) {
                try {
                    const output = await execPromise(`mpp-solar -p /dev/ttyACM0 --porttype serial -c ${cmd} -o json`);
                    response[key] = JSON.parse(output.trim());
                } catch (err) {
                    console.error(`[Error ${cmd}] ${err.message}`);
                }
            }

            const payload = JSON.stringify({
                timestamp: new Date().toISOString(),
                ...response
            });

            client.publish(topic, payload, (err) => {
                if (err) {
                    console.error(`[MQTT Publish Error] ${err.message}`);
                } else {
                    console.log(`[Publish] ${payload}`);
                }
            });

        }, 60000);
    });

    client.on('error', (err) => {
        console.error(`[MQTT Error] ${err.message}`);
    });
};

task();
