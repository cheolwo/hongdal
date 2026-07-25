export function observe(element, dotNetReference) {
    const observer = new IntersectionObserver(
        entries => {
            if (entries.some(entry => entry.isIntersecting)) {
                dotNetReference.invokeMethodAsync("NotifyReachedAsync");
            }
        },
        {
            root: null,
            rootMargin: "360px 0px",
            threshold: 0
        });

    observer.observe(element);

    return {
        dispose: () => observer.disconnect()
    };
}
