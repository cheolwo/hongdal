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

export function observeMedia(container) {
    const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
    const connection = navigator.connection
        ?? navigator.mozConnection
        ?? navigator.webkitConnection;
    const visible = new Map();
    const manuallyPaused = new WeakSet();
    let frame = 0;
    let activeVideo = null;
    let disposed = false;

    const autoplayDisabled = () =>
        reducedMotion.matches
        || connection?.saveData === true
        || document.visibilityState !== "visible";

    const pause = video => {
        if (!video.paused) {
            video.pause();
        }
    };

    const selectActiveVideo = () => {
        frame = 0;
        if (disposed) {
            return;
        }

        const videos = [...container.querySelectorAll("[data-community-feed-video]")];
        if (autoplayDisabled()) {
            videos.forEach(pause);
            activeVideo = null;
            return;
        }

        const viewportCenter = window.innerHeight / 2;
        const candidate = videos
            .map(video => {
                const entry = visible.get(video);
                if (!entry?.isIntersecting || entry.intersectionRatio < 0.55) {
                    return null;
                }

                const bounds = entry.boundingClientRect;
                const center = bounds.top + bounds.height / 2;
                return {
                    video,
                    ratio: entry.intersectionRatio,
                    centerDistance: Math.abs(center - viewportCenter)
                };
            })
            .filter(Boolean)
            .sort((left, right) =>
                left.centerDistance - right.centerDistance
                || right.ratio - left.ratio)[0]?.video ?? null;

        activeVideo = candidate;
        for (const video of videos) {
            if (video !== candidate) {
                pause(video);
            }
        }

        if (candidate && !manuallyPaused.has(candidate)) {
            candidate.muted = true;
            candidate.play().catch(() => {
                // 플랫폼 autoplay 정책이 더 엄격하면 controls를 통한 직접 재생을 유지합니다.
            });
        }
    };

    const scheduleSelection = () => {
        if (!frame) {
            frame = window.requestAnimationFrame(selectActiveVideo);
        }
    };

    const observer = new IntersectionObserver(
        entries => {
            for (const entry of entries) {
                visible.set(entry.target, entry);
                if (!entry.isIntersecting || entry.intersectionRatio < 0.25) {
                    manuallyPaused.delete(entry.target);
                }
            }
            scheduleSelection();
        },
        {
            root: null,
            rootMargin: "-12% 0px -12% 0px",
            threshold: [0, 0.25, 0.55, 0.7, 0.9, 1]
        });

    const onVisibilityChanged = () => scheduleSelection();
    const onPreferenceChanged = () => scheduleSelection();
    document.addEventListener("visibilitychange", onVisibilityChanged);
    reducedMotion.addEventListener?.("change", onPreferenceChanged);

    const observed = new Set();
    const register = video => {
        if (observed.has(video)) {
            return;
        }

        observed.add(video);
        observer.observe(video);
        video.addEventListener("pause", event => {
            if (event.isTrusted && video === activeVideo) {
                manuallyPaused.add(video);
            }
        });
        video.addEventListener("play", event => {
            if (event.isTrusted) {
                manuallyPaused.delete(video);
            }
        });
    };

    const refresh = () => {
        for (const video of [...observed]) {
            if (container.contains(video)) {
                continue;
            }

            observer.unobserve(video);
            observed.delete(video);
            visible.delete(video);
            manuallyPaused.delete(video);
        }

        container
            .querySelectorAll("[data-community-feed-video]")
            .forEach(register);
        scheduleSelection();
    };

    refresh();

    return {
        refresh,
        dispose: () => {
            disposed = true;
            if (frame) {
                window.cancelAnimationFrame(frame);
            }
            observer.disconnect();
            document.removeEventListener("visibilitychange", onVisibilityChanged);
            reducedMotion.removeEventListener?.("change", onPreferenceChanged);
            observed.forEach(pause);
            observed.clear();
            visible.clear();
            activeVideo = null;
        }
    };
}
