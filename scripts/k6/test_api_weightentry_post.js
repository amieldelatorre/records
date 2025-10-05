import http from 'k6/http';
import { check } from 'k6';

// const HOST = __ENV.HOST || 'http://localhost:8080';
const HOST = __ENV.HOST || 'http://host.docker.internal:8080';
const USER_URL_PATH = '/api/v1/user';
const JWT_URL_PATH = "/api/v1/auth/jwt"
const URL_PATH = __ENV.URL_PATH || '/api/v1/weightentry';
const RPS_TARGET = __ENV.RPS_TARGET || 1000;


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
    var result = '';
    var characters = 'abcdefghijklmnopqrstuvwxyz0123456789';
    var charactersLength = characters.length;
    for (var i = 0; i < length; i++) {
        result += characters.charAt(Math.floor(Math.random() * charactersLength));
    }
    return result;
}

function generateRandomDate() {
    const start = new Date('0001-01-01T00:00:00Z').getTime();
    const end = Date.now();

    const randomTimestemp = Math.floor(Math.random() * (end - start)) + start;
    const randomDate = new Date(randomTimestemp);
    return randomDate.toISOString().split("T", 2)[0]
}


export function setup() {
    const username = generateRandomString(8);
    const password = generateRandomString(8);
    const email = `${username}@example.invalid`;

    const userCreateData = JSON.stringify({
        username: username,
        password: password,
        email: email,
    });
    const headers = {
        headers: {
            'Content-Type': 'application/json',
        },
    };
    const userCreateRes = http.post(`${HOST}${USER_URL_PATH}`, userCreateData, headers);
    const userCreateSuccess = check(userCreateRes, {
        'created user successfully': (r) => r.status === 201,
    });
    if (!userCreateSuccess) {
        console.log(`❌ Could not create user: ${userCreateRes.body}`)
    }

    const jwtCreateData = JSON.stringify({
        username: username,
        password: password,
    })
    const jwtCreateRes = http.post(`${HOST}${JWT_URL_PATH}`, jwtCreateData, headers);
    const jwtCreateSuccess = check(jwtCreateRes, {
        'logged in successfully': (r) => r.status === 201,
    });
    if (!jwtCreateSuccess) {
        console.log(`❌ Could not create jwt: ${jwtCreateRes.body}`)
    }
    const access_token = jwtCreateRes.json('credentials.accessToken');
    return { access_token };
}


export default function (creds) {
    const data = JSON.stringify({
        entryDate: generateRandomDate(),
        value: 80
    });

    const res = http.post(`${HOST}${URL_PATH}`, data, {
        headers: {
            'Authorization': `Bearer ${creds.access_token}`,
            'Content-Type': 'application/json'
        },
    });

    // ignore duplicate entries
    const success = check(res, {
        'status is 201': (r) => r.status === 201 || (r.status == 400 && res.body.includes("duplicate entry")),
    });
    if (!success && !res.body.includes("duplicate entry")) {
        console.log(`❌ Could not create weightEntry: ${res.body}`)
    }
}