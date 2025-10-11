import { handleNavHamburgerButton } from "./shared.js";

document.addEventListener("DOMContentLoaded", () => {
    const hamburger = document.getElementById("nav-hamburger");
    hamburger.addEventListener("click", () => {
        hamburger.classList.toggle("nav-hamburger-open");
        handleNavHamburgerButton();
    });
    hamburger.addEventListener("keydown", (event) => {
        if (event.key === "Enter") {
            event.preventDefault();
            hamburger.click();  
        }
    });
});

