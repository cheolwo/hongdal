(() => {
    const showStartupError = (message) => {
        const detail = document.getElementById("blazor-error-detail");
        const errorUi = document.getElementById("blazor-error-ui");
        if (detail && !detail.textContent) {
            detail.textContent = String(message || "역할 앱을 시작하지 못했습니다.").slice(0, 800);
        }
        if (errorUi) {
            errorUi.style.display = "block";
        }
    };

    const originalConsoleError = console.error.bind(console);
    console.error = (...values) => {
        originalConsoleError(...values);
        showStartupError(values.map(value => {
            if (value instanceof Error) {
                return value.message;
            }

            return typeof value === "string" ? value : String(value);
        }).join(" "));
    };

    window.addEventListener("error", event => {
        showStartupError(event.error?.message || event.message);
    });
    window.addEventListener("unhandledrejection", event => {
        showStartupError(event.reason?.message || event.reason);
    });

    window.ssalddelRoleApp = { showStartupError };
})();
