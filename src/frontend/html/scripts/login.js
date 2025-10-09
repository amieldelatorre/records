import { DEFAULT_HEADERS, saveCredentials, getCredentials, deleteCredentials, sleep } from "./shared.js";
import { JWT_URL } from "./api_consts.js";

// TODO: A LOT

async function onSubmit(event) {
    event.preventDefault();

    const submitButton = document.getElementById("login-form-submit-button");
    const loginFormSubmitButtonEnabledText = document.getElementById("login-form-submit-button-enabled-text");
    const loginFormSubmitButtonLoader = document.getElementById("login-form-submit-button-loader");
    const loginFormSubmitButtonSuccess = document.getElementById("login-form-submit-button-success");


    // while waiting for response
    submitButton.classList.toggle("normal-button")
    submitButton.disabled = true;
    submitButton.classList.toggle("waiting-button");
    loginFormSubmitButtonEnabledText.hidden = true;
    loginFormSubmitButtonSuccess.hidden = true;
    loginFormSubmitButtonLoader.hidden = false;
    // await sleep(3000); // TODO: REMOVE WHEN DONE



    const username = document.getElementById("input-username").value;
    const password = document.getElementById("input-password").value;
    const data = JSON.stringify({
        Username: username,
        Password: password
    });


    loginFormSubmitButtonEnabledText.hidden = false;
    await fetch(JWT_URL, {
        method: "POST",
        cors: "no-cors",
        headers: DEFAULT_HEADERS,
        body: data
    }).then(async response => {
        if (response.ok) {
            console.log("OK")
            saveCredentials(await response.json());

            loginFormSubmitButtonLoader.hidden = true;
            submitButton.classList.toggle("waiting-button");
            submitButton.classList.toggle("success-button");
            loginFormSubmitButtonSuccess.hidden = false;
            window.location.href = "/"
        }
    }).catch(error => {
        alert("nok")
    });

    // submitButton.disabled = false;
}

document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("login-form").addEventListener("submit", onSubmit);
})