const mqtt = require('mqtt');
const { exec } = require('child_process');

// Configuration
const config = {
    mqtt: {
        username: 'solar-panel-1',
        password: Buffer.from('UGFzc3dvcmQx', 'base64').toString(),
        protocol: 'mqtts',
        host: '9044eae185c0455d85f985eb24270bc9.s1.eu.hivemq.cloud',
        port: 8883,
        clean: true,
        connectTimeout: 4000,
        rejectUnauthorized: true
    },
    topics: {
        data: 'data',
        chargeSwitch: 'ChargeSwitch'
    },
    device: {
        port: '/dev/ttyACM0',
        portType: 'serial'
    },
    pollingInterval: 60000 // 1 minute
};

// State management
let state = {
    currentBatteryMode: '',
    currentLoadMode: '',
    isConnected: false
};

// MQTT client management
function setupMqttClient() {
    const client = mqtt.connect({
        username: config.mqtt.username,
        password: config.mqtt.password,
        protocol: config.mqtt.protocol,
        host: config.mqtt.host,
        port: config.mqtt.port,
        clean: config.mqtt.clean,
        connectTimeout: config.mqtt.connectTimeout,
        rejectUnauthorized: config.mqtt.rejectUnauthorized
    });

    // Set up event handlers
    client.on('connect', () => handleConnect(client));
    client.on('message', (topic, message) => handleMessage(client, topic, message));
    client.on('error', handleError);
    client.on('disconnect', handleDisconnect);
    client.on('reconnect', handleReconnect);

    return client;
}

// Event handlers
function handleConnect(client) {
    state.isConnected = true;
    console.log(`[${new Date().toISOString()}] Connected to MQTT broker`);
    
    subscribeToTopics(client);
    startDataPolling(client);
}

function handleDisconnect() {
    state.isConnected = false;
    console.log(`[${new Date().toISOString()}] Disconnected from MQTT broker`);
}

function handleReconnect() {
    console.log(`[${new Date().toISOString()}] Attempting to reconnect to MQTT broker`);
}

function handleError(error) {
    console.error(`[${new Date().toISOString()}] MQTT error:`, error.message);
}

function handleMessage(client, topic, message) {
    if (topic === config.topics.chargeSwitch) {
        try {
            const data = JSON.parse(message.toString());
            processSwitchCommands(data);
        } catch (err) {
            console.error('[MQTT JSON Error]', err.message);
        }
    }
}

// Subscription management
function subscribeToTopics(client) {
    client.subscribe(config.topics.chargeSwitch, (err) => {
        if (err) {
            console.error('[Subscribe Error]', err.message);
        } else {
            console.log(`[Subscribe] Subscribed to topic: ${config.topics.chargeSwitch}`);
        }
    });
}

// Command execution
function executeCommand(command, type) {
    return new Promise((resolve, reject) => {
        // Validate command to prevent command injection
        if (!validateCommand(command)) {
            reject(new Error(`Invalid command: ${command}`));
            return;
        }

        const cmdString = `mpp-solar -p ${config.device.port} --porttype ${config.device.portType} -c ${command}`;
        exec(cmdString, (err, stdout, stderr) => {
            if (err) {
                console.error(`[${type} Error]`, err.message);
                reject(err);
            } else {
                console.log(`[${type}] Executed command: ${command}`);
                resolve(stdout);
            }
        });
    });
}

function validateCommand(command) {
    // Basic validation - only allow alphanumeric characters and specific symbols
    const validCommandPattern = /^[a-zA-Z0-9_]+$/;
    return validCommandPattern.test(command);
}

function processSwitchCommands(data) {
    const newBatteryMode = data.CommandCharge || '';
    const newLoadMode = data.CommandLoad || '';

    if (newBatteryMode && newBatteryMode !== state.currentBatteryMode) {
        executeCommand(newBatteryMode, 'PCP')
            .then(() => {
                state.currentBatteryMode = newBatteryMode;
                console.log(`[PCP] Applied new battery mode: ${newBatteryMode}`);
            })
            .catch(err => console.error('[PCP Error]', err.message));
    }

    if (newLoadMode && newLoadMode !== state.currentLoadMode) {
        executeCommand(newLoadMode, 'POP')
            .then(() => {
                state.currentLoadMode = newLoadMode;
                console.log(`[POP] Applied new load mode: ${newLoadMode}`);
            })
            .catch(err => console.error('[POP Error]', err.message));
    }
}

// Data collection and publishing
function startDataPolling(client) {
    // Initial poll
    pollAndPublishData(client);
    
    // Set up interval for regular polling
    setInterval(() => pollAndPublishData(client), config.pollingInterval);
}

function pollAndPublishData(client) {
    if (!state.isConnected) {
        console.log('[Polling] Skipping data poll - not connected to MQTT broker');
        return;
    }

    collectInverterData()
        .then(data => {
            const payload = JSON.stringify({
                timestamp: new Date().toISOString(),
                ...data
            });

            client.publish(config.topics.data, payload);
            console.log(`[Publish] Data published to ${config.topics.data}`);
        })
        .catch(err => console.error('[Polling Error]', err.message));
}

async function collectInverterData() {
    try {
        const currentData = await executeQpigs();
        const inverterSettings = await executeQpiri();

        return {
            pv_to_load: currentData.pv_input_power || 0,
            load_power: currentData.ac_output_active_power || 0,
            grid_power: currentData.ac_input_voltage > 0 ? currentData.ac_output_active_power : 0,
            battery_soc: currentData.battery_capacity || 0,
            inverter_settings: inverterSettings
        };
    } catch (error) {
        console.error('[Data Collection Error]', error.message);
        throw error;
    }
}

function executeQpigs() {
    return new Promise((resolve, reject) => {
        exec(`mpp-solar -p ${config.device.port} --porttype ${config.device.portType} -c QPIGS -o json`, (err, stdout) => {
            if (err) {
                reject(new Error(`QPIGS Error: ${err.message}`));
                return;
            }

            try {
                const data = JSON.parse(stdout);
                resolve(data);
            } catch (parseErr) {
                reject(new Error(`QPIGS JSON parse error: ${parseErr.message}`));
            }
        });
    });
}

function executeQpiri() {
    return new Promise((resolve, reject) => {
        exec(`mpp-solar -p ${config.device.port} --porttype ${config.device.portType} -c QPIRI -o json`, (err, stdout) => {
            if (err) {
                console.error('[QPIRI Error]', err.message);
                resolve({}); // Return empty object on error, don't fail the whole data collection
                return;
            }

            try {
                const data = JSON.parse(stdout);
                resolve(data);
            } catch (parseErr) {
                console.error('[QPIRI JSON Error]', parseErr.message);
                resolve({});
            }
        });
    });
}

// Main execution
function main() {
    try {
        const client = setupMqttClient();
        
        // Handle application termination
        process.on('SIGINT', () => {
            console.log('Terminating application...');
            if (client && client.connected) {
                client.end(true, () => {
                    console.log('MQTT client disconnected');
                    process.exit(0);
                });
            } else {
                process.exit(0);
            }
        });
    } catch (error) {
        console.error('Failed to start application:', error.message);
        process.exit(1);
    }
}

// Start the application
main();
