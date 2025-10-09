export const LOCAL_STORAGE_CREDENTIALS_KEY = "credentials";

export const DEFAULT_HEADERS = {
    "Content-Type": "application/json"
}

export function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms))
}

export function saveCredentials(data) {
    localStorage.setItem(LOCAL_STORAGE_CREDENTIALS_KEY, JSON.stringify(data))
}

export function getCredentials() {
    return JSON.parse(localStorage.getItem(LOCAL_STORAGE_CREDENTIALS_KEY)).credentials.accessToken
}

export function deleteCredentials() {
    localStorage.removeItem(LOCAL_STORAGE_CREDENTIALS_KEY);
}