window.ssalddelLocale = {
    readSignals: function () {
        const cookieName = "ssalddel.display-language=";
        const languageCookie = document.cookie
            .split(";")
            .map(value => value.trim())
            .find(value => value.startsWith(cookieName));

        return {
            cookieLanguageCode: languageCookie
                ? decodeURIComponent(languageCookie.substring(cookieName.length))
                : null,
            browserLanguageCodes: Array.isArray(navigator.languages) && navigator.languages.length > 0
                ? navigator.languages
                : navigator.language
                    ? [navigator.language]
                    : []
        };
    },
    writePreference: function (languageCode) {
        const secure = window.location.protocol === "https:" ? "; Secure" : "";
        document.cookie = "ssalddel.display-language="
            + encodeURIComponent(languageCode)
            + "; Path=/; Max-Age=31536000; SameSite=Lax"
            + secure;
        this.applyDocumentLanguage(languageCode);
    },
    applyDocumentLanguage: function (languageCode) {
        const normalized = typeof languageCode === "string"
            ? languageCode.trim()
            : "";
        document.documentElement.lang = normalized || "ko-KR";
    }
};
