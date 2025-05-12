document.addEventListener("DOMContentLoaded", function () {
    document.addEventListener("keydown", function (e) {
        const focusable = Array.from(document.querySelectorAll('[tabindex]'))
            .filter(el => !el.disabled);

        const currentIndex = focusable.indexOf(document.activeElement);

        if (e.key === "ArrowDown" || e.key === "ArrowRight") {
            e.preventDefault();
            const next = focusable[(currentIndex + 1) % focusable.length];
            next.focus();
        } else if (e.key === "ArrowUp" || e.key === "ArrowLeft") {
            e.preventDefault();
            const prev = focusable[(currentIndex - 1 + focusable.length) % focusable.length];
            prev.focus();
        } else if (e.key === "Enter") {
            if (document.activeElement.tagName === "BUTTON" || document.activeElement.tagName === "A") {
                document.activeElement.click();
            }
        }
    });
});
