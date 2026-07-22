export function scrollToFocusTarget(targetId) {
    if (!targetId) {
        return false;
    }

    const element = document.getElementById(targetId);
    if (!element) {
        return false;
    }

    element.scrollIntoView({ block: "center", inline: "nearest", behavior: "auto" });
    const focusable = element.querySelector("button, a, input, select, textarea, [tabindex]");
    if (focusable instanceof HTMLElement) {
        focusable.focus({ preventScroll: true });
    }
    return true;
}

export function scrollToPageTop() {
    const mainContent = document.querySelector(".mud-main-content");
    if (mainContent instanceof HTMLElement) {
        mainContent.scrollTo({ top: 0, left: 0, behavior: "auto" });
    }

    if (document.scrollingElement) {
        document.scrollingElement.scrollTop = 0;
        document.scrollingElement.scrollLeft = 0;
    }

    window.scrollTo({ top: 0, left: 0, behavior: "auto" });
}
