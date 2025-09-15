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
            exec('mpp-solar -p /dev/ttyACM0 --porttype serial -c QPIGS -o json', (error, stdout, stderr) => {
                client.publish(topic, stdout);
                console.log(`[${new Date().toISOString()}, Publish] ${stdout}`);
                if (error) {
                    console.error(`[${new Date().toISOString()}, Error] ${error.message}`);
                    return;
                }
                if (stderr) {
                    console.error(`[${new Date().toISOString()}, Stderr] ${stderr}`);
                }
            });
        }, 60000)
    });
};

const executing = task();