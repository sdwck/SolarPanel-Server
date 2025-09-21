const mqtt = require('mqtt');
const { exec } = require('child_process');

const task = () => {
    const password = Buffer.from('UGFzc3dvcmQx', 'base64').toString();
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

    const topicData = 'data';         
    const topicChargeSwitch = 'ChargeSwitch';

    
    let currentBatteryMode = '';
    let currentLoadMode = '';

    
    function applyCommand(command, type) {
        if (type === 'battery' && command && command !== currentBatteryMode) {
            exec(`mpp-solar -p /dev/ttyACM0 --porttype serial -c ${command}`, err => {
                if (err) console.error('[PCP Error]', err.message);
                else {
                    console.log(`[PCP] Applied new battery mode: ${command}`);
                    currentBatteryMode = command;
                }
            });
        }

        if (type === 'load' && command && command !== currentLoadMode) {
            exec(`mpp-solar -p /dev/ttyACM0 --porttype serial -c ${command}`, err => {
                if (err) console.error('[POP Error]', err.message);
                else {
                    console.log(`[POP] Applied new load mode: ${command}`);
                    currentLoadMode = command;
                }
            });
        }
    }

    client.on('connect', () => {
        console.log(`[${new Date().toISOString()}] Connected to MQTT broker`);

        
        client.subscribe(topicChargeSwitch, (err) => {
            if (err) console.error('[Subscribe Error]', err.message);
            else console.log(`[Subscribe] Subscribed to topic: ${topicChargeSwitch}`);
        });

        
 

        
        setInterval(() => {
            exec('mpp-solar -p /dev/ttyACM0 --porttype serial -c QPIGS -o json', (err, qpgsOut) => {
                if (err) return console.error('[QPIGS Error]', err.message);

                let currentData;
                try { currentData = JSON.parse(qpgsOut); }
                catch (parseErr) { return console.error('[QPIGS JSON Error]', parseErr.message); }

                exec('mpp-solar -p /dev/ttyACM0 --porttype serial -c QPIRI -o json', (err, qpiriOut) => {
                    let inverterSettings = {};
                    if (!err) {
                        try { inverterSettings = JSON.parse(qpiriOut); }
                        catch (parseErr) { console.error('[QPIRI JSON Error]', parseErr.message); }
                    } else console.error('[QPIRI Error]', err.message);

                    const payload = JSON.stringify({
                        timestamp: new Date().toISOString(),
                        pv_to_load: currentData.pv_input_power || 0,
                        load_power: currentData.ac_output_active_power || 0,
                        grid_power: currentData.ac_input_voltage > 0 ? currentData.ac_output_active_power : 0,
                        battery_soc: currentData.battery_capacity || 0,
                        inverter_settings: inverterSettings
                    });

                    client.publish(topicData, payload);
                    console.log(`[Publish] ${payload}`);
                });
            });
        }, 60000);
    });

    
    client.on('message', (receivedTopic, message) => {
        if (receivedTopic === topicChargeSwitch) {
            try {
                const data = JSON.parse(message.toString());
                const newBatteryMode = data.CommandCharge || '';
                const newLoadMode = data.CommandLoad || '';

                
                applyCommand(newBatteryMode, 'battery');
                applyCommand(newLoadMode, 'load');

            } catch (err) {
                console.error('[MQTT JSON Error]', err.message);
            }
        }
    });
};

task();
