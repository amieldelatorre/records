import http from 'k6/http';
import { check } from 'k6';

// const HOST = __ENV.HOST || 'http://localhost:8080';
const HOST = __ENV.HOST || 'http://host.docker.internal:8080';
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

export function setup() {
    const data = JSON.stringify({
        username: 'stephenhawking',
        password: 'password',
    });

    const headers = {
        headers: {
            'Content-Type': 'application/json',
        },
    };

    const jwt_path = "/api/v1/auth/jwt"
    const res = http.post(`${HOST}${jwt_path}`, data, headers);

    check(res, {
        'logged in successfully': (r) => r.status === 201,
    });

    const access_token = res.json('credentials.accessToken');
    return { access_token };
}

export default function (data) {
    const res = http.get(`${HOST}${URL_PATH}/0199a94e-996b-7cf1-ad15-e2b3776d6f13`, {
        headers: { 'Authorization': `Bearer ${data.access_token}` },
    });

    check(res, {
        'status is 200': (r) => r.status === 200,
    });
}