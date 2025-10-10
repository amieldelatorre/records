import { getCredentials, deleteCredentials } from "./shared.js";

export function isLoggedIn() {
    const credentials = getCredentials();
    return credentials !== undefined && credentials != null && credentials.trim() !== "";
}

export function logout() {
    deleteCredentials();
    window.location.href = "/login";
}


document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("logout-button")?.addEventListener("click", logout);
})