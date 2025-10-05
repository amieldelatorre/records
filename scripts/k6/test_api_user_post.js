import http from 'k6/http';
import { check } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';
// const BASE_URL = __ENV.BASE_URL || 'http://host.docker.internal:8080';
const URL_PATH = __ENV.URL_PATH || '/api/v1/user';
const RPS_TARGET = __ENV.RPS_TARGET || 10;


export let options = {
    scenarios: {
        ramped_rate: {
            executor: 'ramping-arrival-rate',
            startRate: 10,             // 10 requests/sec (600 req/min)
            timeUnit: '1s',
            preAllocatedVUs: 500,      // Pre-allocate 500 virtual users
            maxVUs: 8000,              // Allow up to 8,0000 VUs if needed
            stages: [
                { target: RPS_TARGET, duration: '10s' },    // Ramp to target
                { target: RPS_TARGET, duration: '1m' },     // Hold target
            ],
        },
    },
};

function generateRandomString(length) {
    var result           = '';
    var characters       = 'abcdefghijklmnopqrstuvwxyz0123456789';
    var charactersLength = characters.length;
    for ( var i = 0; i < length; i++ ) {
        result += characters.charAt(Math.floor(Math.random() * charactersLength));
    }
    return result;
}

export default function () {
    let username = generateRandomString(8);
    let password = generateRandomString(8);
    let email = `${username}@example.invalid`;
    let data = {
        "Username": username,
        "Email": email,
        "Password": password
    };

    const res = http.post(`${BASE_URL}${URL_PATH}`, JSON.stringify(data), {
        headers: { 'Content-Type': 'application/json' },
    });

    check(res, {
        'status is 201': (r) => r.status === 201,
    });
}