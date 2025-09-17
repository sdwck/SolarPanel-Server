const mqtt = require('mqtt');
const { exec } = require('child_process');

const task = async () => {
    const password = atob('UGFzc3dvcmQx');
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
        console.log(`[${new Date().toISOString()}, Connected]`);

        setInterval(() => {
            exec('mpp-solar -p /dev/ttyACM0 --porttype serial -c QPIGS -o json', (error, stdout) => {
                if (error) {
                    console.error(`[Error QPIGS] ${error.message}`);
                    return;
                }

                let data;
                try {
                    data = JSON.parse(stdout);
                } catch (parseError) {
                    console.error(`[JSON Error] ${parseError.message}`);
                    return;
                }



               
                exec('mpp-solar -p /dev/ttyACM0 --porttype serial -c POP02', (err) => {
                    if (err) console.error(`[Error POP02] ${err.message}`);
                });

                
                exec('mpp-solar -p /dev/ttyACM0 --porttype serial -c PCP01', (err) => {
                    if (err) console.error(`[Error PCP01] ${err.message}`);
                });

                
                exec('mpp-solar -p /dev/ttyACM0 --porttype serial -c QMOD -o json', (err, modOut) => {
                    let currentMode = 'UNKNOWN';
                    if (!err) {
                        try {
                            const modData = JSON.parse(modOut);
                            currentMode = modData.mode || 'UNKNOWN';
                        } catch (parseErr) {
                            console.error(`[JSON Error QMOD] ${parseErr.message}`);
                        }
                    } else {
                        console.error(`[Error QMOD] ${err.message}`);
                    }

                   
                    const payload = JSON.stringify({
                        timestamp: new Date().toISOString(),
                        ...data,
                        inverter_action: 'SBU_MODE',
                        current_mode: currentMode
                    });

                    client.publish(topic, payload);
                    console.log(`[Publish] ${payload}`);
                });

            });
        }, 60000); 
    });
};

task();
