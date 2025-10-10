export const LOCAL_STORAGE_CREDENTIALS_KEY = "credentials";
export const API_SERVER_ERROR = "Unable to reach server 😞.\nPlease try again later and if problem persists, please contact the administrator."

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
    const credentials = JSON.parse(localStorage.getItem(LOCAL_STORAGE_CREDENTIALS_KEY))?.credentials?.accessToken;
    return credentials;
}

export function deleteCredentials() {
    localStorage.removeItem(LOCAL_STORAGE_CREDENTIALS_KEY);
}

const BUTTON_STYLING_NORMAL_NAME = "normal-button";
const BUTTON_STYLING_SUCCESS_NAME = "success-button";
const BUTTON_STYLING_LOADING_NAME = "loading-button";

export function changeButtonToNormal(buttonId, buttonNormalTextId, loaderId, successTickId) {
    const button = document.getElementById(buttonId);
    const buttonEnabledText = document.getElementById(buttonNormalTextId);
    const buttonLoader = document.getElementById(loaderId);
    const buttonSuccess = document.getElementById(successTickId);

    buttonLoader.hidden = true;
    buttonSuccess.hidden = true;
    button.classList.remove("loading-button");
    button.classList.add("normal-button")
    buttonEnabledText.hidden = false;
    button.disabled = false;
}

export function changeButtonToSuccess(buttonId, buttonNormalTextId, loaderId, successTickId) {
    const button = document.getElementById(buttonId);
    const buttonEnabledText = document.getElementById(buttonNormalTextId);
    const buttonLoader = document.getElementById(loaderId);
    const buttonSuccess = document.getElementById(successTickId);

    button.disabled = true;
    buttonLoader.hidden = true;
    buttonEnabledText.hidden = true;
    button.classList.remove(BUTTON_STYLING_LOADING_NAME);
    button.classList.add(BUTTON_STYLING_SUCCESS_NAME);
    buttonSuccess.hidden = false;
}

export function changeButtonToLoading(buttonId, buttonNormalTextId, loaderId, successTickId) {
    const button = document.getElementById(buttonId);
    const buttonEnabledText = document.getElementById(buttonNormalTextId);
    const buttonLoader = document.getElementById(loaderId);
    const buttonSuccess = document.getElementById(successTickId);

    button.disabled = true;
    buttonEnabledText.hidden = true;
    buttonSuccess.hidden = true;
    button.classList.remove(BUTTON_STYLING_NORMAL_NAME)
    button.classList.add(BUTTON_STYLING_LOADING_NAME);
    buttonLoader.hidden = false;
}