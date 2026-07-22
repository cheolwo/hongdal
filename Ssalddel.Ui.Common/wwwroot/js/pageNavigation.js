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
