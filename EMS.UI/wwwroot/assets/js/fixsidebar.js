document.addEventListener("DOMContentLoaded", function () {
    const dropdowns = document.querySelectorAll(".menu-item.menu-accordion");

    // Function to set cookie
    function setCookie(name, value, days) {
        let expires = "";
        if (days) {
            let date = new Date();
            date.setTime(date.getTime() + days * 24 * 60 * 60 * 1000);
            expires = "; expires=" + date.toUTCString();
        }
        document.cookie = name + "=" + value + "; path=/" + expires;
    }

    // Function to get cookie
    function getCookie(name) {
        let nameEQ = name + "=";
        let ca = document.cookie.split(";");
        for (let i = 0; i < ca.length; i++) {
            let c = ca[i];
            while (c.charAt(0) == " ") c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    }

    // Load saved dropdown from cookies
    const openDropdownIndex = getCookie("openDropdown");

    dropdowns.forEach((dropdown, index) => {
        const submenu = dropdown.querySelector(".menu-sub");

        // Set Metronic trigger to manual
        dropdown.setAttribute("data-kt-menu-trigger", "manual");

        // If previously opened, keep it open
        if (`dropdown_${index}` === openDropdownIndex) {
            dropdown.classList.add("hover");
            submenu.style.display = "block";
        }

        dropdown.querySelector(".menu-link").addEventListener("click", function (event) {
            event.preventDefault(); // Prevent Metronic default toggle

            const isCurrentlyOpen = dropdown.classList.contains("hover");

            // Close all dropdowns except current
            dropdowns.forEach((d, i) => {
                d.classList.remove("hover");
                d.querySelector(".menu-sub").style.display = "none";
                if (i !== index) {
                    setCookie("openDropdown", "", -1); // Remove cookie
                }
            });

            // Toggle only the clicked dropdown
            if (!isCurrentlyOpen) {
                dropdown.classList.add("hover");
                submenu.style.display = "block";
                setCookie("openDropdown", `dropdown_${index}`, 7); // Save for 7 days
            } else {
                setCookie("openDropdown", "", -1); // Remove cookie
            }
        });
    });
});
