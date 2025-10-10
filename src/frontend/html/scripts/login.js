import { DEFAULT_HEADERS, saveCredentials, API_SERVER_ERROR, changeButtonToLoading, changeButtonToSuccess, changeButtonToNormal, sleep } from "./shared.js";
import { JWT_URL } from "./api_consts.js";
import { isLoggedIn } from "./login_required.js";

// If the user is already logged in, redirect to main page
if (isLoggedIn()) {
    window.location.href = "/";
}

const SUBMIT_BUTTON_ID = "login-form-submit-button";
const SUBMIT_BUTTON_ENABLED_TEXT_ID = "login-form-submit-button-enabled-text";
const SUBMIT_BUTTON_LOADER_ID = "login-form-submit-button-loader";
const SUBMIT_BUTTON_SUCCESS_ID = "login-form-submit-button-success";

async function onSubmit(event) {
    event.preventDefault();

    changeButtonToLoading(SUBMIT_BUTTON_ID, SUBMIT_BUTTON_ENABLED_TEXT_ID, SUBMIT_BUTTON_LOADER_ID, SUBMIT_BUTTON_SUCCESS_ID);
    // await sleep(3000); // TODO: REMOVE WHEN DONE

    const username = document.getElementById("input-username").value;
    const password = document.getElementById("input-password").value;
    const data = JSON.stringify({
        Username: username,
        Password: password
    });


    await fetch(JWT_URL, {
        method: "POST",
        cors: "no-cors",
        headers: DEFAULT_HEADERS,
        body: data
    }).then(async response => {
        if (response.ok) {
            saveCredentials(await response.json());
            changeButtonToSuccess(SUBMIT_BUTTON_ID, SUBMIT_BUTTON_ENABLED_TEXT_ID, SUBMIT_BUTTON_LOADER_ID, SUBMIT_BUTTON_SUCCESS_ID);
            await sleep(300);
            window.location.href = "/";
        } else if (response.status == 401) {
            alert("Username and Password combination was not recognized!\nPlease try again.")
            changeButtonToNormal(SUBMIT_BUTTON_ID, SUBMIT_BUTTON_ENABLED_TEXT_ID, SUBMIT_BUTTON_LOADER_ID, SUBMIT_BUTTON_SUCCESS_ID);
        } else {
            changeButtonToNormal(SUBMIT_BUTTON_ID, SUBMIT_BUTTON_ENABLED_TEXT_ID, SUBMIT_BUTTON_LOADER_ID, SUBMIT_BUTTON_SUCCESS_ID);
            alert(API_SERVER_ERROR);
        }
    }).catch(error => {
        console.log(error);
        changeButtonToNormal(SUBMIT_BUTTON_ID, SUBMIT_BUTTON_ENABLED_TEXT_ID, SUBMIT_BUTTON_LOADER_ID, SUBMIT_BUTTON_SUCCESS_ID);
        alert(API_SERVER_ERROR);
    });
}

document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("login-form").addEventListener("submit", onSubmit);
})