import { handleNavHamburgerButton } from "./shared.js";
import { logout, isLoggedIn } from "./login_required.js";

const NAV_HAMBURGER_ID = "nav-hamburger";
const PAGE_CONTENT_ID = "page-content";
let previousWindowInnerWidth = window.innerWidth;

// If the user is already logged in, redirect to main page
if (!isLoggedIn()) {
    window.location.href = "/login";
}

document.addEventListener("DOMContentLoaded", () => {
    const hamburger = document.getElementById(NAV_HAMBURGER_ID);
    const pageContent = document.getElementById(PAGE_CONTENT_ID);
    hamburger.addEventListener("click", () => {
        hamburger.classList.toggle("nav-hamburger-open");
        pageContent.hidden = !pageContent.hidden;
        handleNavHamburgerButton();
    });
    hamburger.addEventListener("keydown", (event) => {
        if (event.key === "Enter") {
            event.preventDefault();
            hamburger.click();  
        }
    });
});


window.addEventListener("resize", () => {
    const hamburger = document.getElementById(NAV_HAMBURGER_ID);
    const pageContent = document.getElementById(PAGE_CONTENT_ID);
    const navItemsId = "nav-items";
    const navItems = document.getElementById(navItemsId);
    const currentWindowInnerWidth = window.innerWidth;

    // If the screen resize is not mobile
    //      close hamburger
    //      show page content
    //      show bar items
    if (currentWindowInnerWidth > 768 && previousWindowInnerWidth <= 768) {
        hamburger.classList.remove("nav-hamburger-open");
        pageContent.hidden = false;
        navItems.style.display = "block";
    }
    // if screen resize is mobile
    //      close hamburger
    //      show page content
    //      hide nav items
    else if (window.innerWidth <= 768 && previousWindowInnerWidth > 768) {
        hamburger.classList.remove("nav-hamburger-open");
        pageContent.hidden = false;
        navItems.style.display = "none";
    }

    previousWindowInnerWidth = currentWindowInnerWidth;
});


document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("logout-button")?.addEventListener("click", logout);
});