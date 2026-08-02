const instances = new Map();
let googleMapsLoadPromise;

const dayMapStyle = [];
const nightMapStyle = [
    { elementType: "geometry", stylers: [{ color: "#202536" }] },
    { elementType: "labels.icon", stylers: [{ visibility: "simplified" }, { saturation: -35 }, { lightness: -18 }] },
    { elementType: "labels.text.fill", stylers: [{ color: "#d8d8e8" }] },
    { elementType: "labels.text.stroke", stylers: [{ color: "#202536" }] },
    { featureType: "administrative.country", elementType: "geometry.stroke", stylers: [{ color: "#77709d" }] },
    { featureType: "administrative.province", elementType: "geometry.stroke", stylers: [{ color: "#4f5368" }] },
    { featureType: "landscape.natural", elementType: "geometry", stylers: [{ color: "#252b3b" }] },
    { featureType: "poi", elementType: "geometry", stylers: [{ color: "#282f40" }] },
    { featureType: "poi", elementType: "labels.text.fill", stylers: [{ color: "#aaa9c4" }] },
    { featureType: "road", elementType: "geometry", stylers: [{ color: "#353b4c" }] },
    { featureType: "road", elementType: "labels.text.fill", stylers: [{ color: "#aaa9ba" }] },
    { featureType: "transit", elementType: "geometry", stylers: [{ color: "#30364a" }] },
    { featureType: "water", elementType: "geometry", stylers: [{ color: "#17172f" }] }
];

export async function initialize(
    elementId,
    markers,
    datasetCode,
    selectedCode,
    selectedMarkerId,
    dotNetReference,
    suppliedRuntimeConfig) {
    const runtimeConfig = suppliedRuntimeConfig ?? globalThis.ssalddelRuntimeConfig;
    if (!isRuntimeOriginAllowed(runtimeConfig)) {
        return "blocked-origin";
    }

    const googleMapsAlreadyLoaded = Boolean(globalThis.google?.maps?.importLibrary);
    const apiKey = googleMapsAlreadyLoaded ? "" : consumeRuntimeValue(runtimeConfig, "browserApiKey", "googleMapsBrowserApiKey");
    if (!googleMapsAlreadyLoaded && !apiKey) {
        return "unconfigured";
    }

    const element = document.getElementById(elementId);
    if (!element) {
        return "failed";
    }

    try {
        await loadGoogleMaps(apiKey);
        const { Map: GoogleMap } = await google.maps.importLibrary("maps");
        const map = new GoogleMap(element, {
            center: { lat: 20, lng: 15 },
            zoom: 2,
            minZoom: 2,
            maxZoom: 14,
            mapTypeId: "roadmap",
            mapTypeControl: true,
            mapTypeControlOptions: {
                mapTypeIds: ["roadmap", "terrain", "satellite"],
                position: google.maps.ControlPosition.TOP_RIGHT,
                style: google.maps.MapTypeControlStyle.DROPDOWN_MENU
            },
            zoomControl: true,
            zoomControlOptions: {
                position: google.maps.ControlPosition.RIGHT_CENTER
            },
            scaleControl: true,
            streetViewControl: true,
            streetViewControlOptions: {
                position: google.maps.ControlPosition.RIGHT_CENTER
            },
            fullscreenControl: true,
            clickableIcons: false,
            keyboardShortcuts: true,
            gestureHandling: "greedy",
            controlSize: 32
        });

        const instance = {
            map,
            dotNetReference,
            selectedCode,
            selectedMarkerId,
            datasetCode,
            clickListener: map.data.addListener("click", event => {
                const countryCode = event.feature.getProperty("code");
                const markerId = event.feature.getProperty("markerId");
                if (countryCode && markerId && instance.dotNetReference) {
                    instance.dotNetReference.invokeMethodAsync("SelectMapFeatureFromGoogleMap", countryCode, markerId);
                }
            })
        };

        instances.set(elementId, instance);
        updateDataset(elementId, markers, datasetCode, selectedCode, selectedMarkerId, false);
        return "ready";
    } catch {
        return "failed";
    }
}

export function updateDataset(elementId, markers, datasetCode, selectedCode, selectedMarkerId, preserveViewport = false) {
    const instance = instances.get(elementId);
    if (!instance) {
        return;
    }

    instance.datasetCode = datasetCode;
    instance.selectedCode = selectedCode;
    instance.selectedMarkerId = selectedMarkerId;
    const oldFeatures = [];
    instance.map.data.forEach(feature => oldFeatures.push(feature));
    oldFeatures.forEach(feature => instance.map.data.remove(feature));

    const bounds = new google.maps.LatLngBounds();
    for (const marker of markers ?? []) {
        const position = { lat: marker.latitude, lng: marker.longitude };
        instance.map.data.add({
            id: marker.id ?? marker.code,
            geometry: new google.maps.Data.Point(position),
            properties: {
                code: marker.code,
                markerId: marker.id ?? marker.code,
                name: marker.name,
                dataLabel: marker.dataLabel,
                layerCode: marker.layerCode
            }
        });
        bounds.extend(position);
    }

    instance.map.setOptions({
        styles: datasetCode === "night-learning" ? nightMapStyle : dayMapStyle,
        backgroundColor: datasetCode === "night-learning" ? "#17172f" : "#eef6f3"
    });
    applyDataStyle(instance);

    if (!preserveViewport && !bounds.isEmpty()) {
        instance.map.fitBounds(bounds, mapViewportPadding(instance));
    }
}

export function updateSelection(elementId, selectedCode, selectedMarkerId) {
    const instance = instances.get(elementId);
    if (!instance) {
        return;
    }

    instance.selectedCode = selectedCode;
    instance.selectedMarkerId = selectedMarkerId;
    applyDataStyle(instance);
    focusSelection(instance);
}

export function focusElement(elementId) {
    globalThis.requestAnimationFrame(() => {
        document.getElementById(elementId)?.focus({ preventScroll: true });
    });
}

export function dispose(elementId) {
    const instance = instances.get(elementId);
    if (!instance) {
        return;
    }

    if (instance.clickListener) {
        instance.clickListener.remove();
    }
    google.maps.event.clearInstanceListeners(instance.map);
    instances.delete(elementId);
}

function applyDataStyle(instance) {
    const night = instance.datasetCode === "night-learning";
    instance.map.data.setStyle(feature => {
        const selected = instance.selectedMarkerId
            ? feature.getProperty("markerId") === instance.selectedMarkerId
            : feature.getProperty("code") === instance.selectedCode;
        const layerCode = feature.getProperty("layerCode");
        const layerStyle = markerStyleFor(layerCode, night);
        return {
            title: `${feature.getProperty("name")} · ${feature.getProperty("dataLabel")}`,
            icon: {
                path: layerStyle.path,
                scale: selected ? layerStyle.selectedScale : layerStyle.scale,
                fillColor: selected ? (night ? "#fff0a8" : "#176b4d") : layerStyle.color,
                fillOpacity: 1,
                strokeColor: "#ffffff",
                strokeOpacity: 1,
                strokeWeight: selected ? 4 : 3
            },
            zIndex: selected ? 2 : 1
        };
    });
}

function markerStyleFor(layerCode, night) {
    switch (layerCode) {
        case "regional-culture":
            return { color: "#176b4d", path: google.maps.SymbolPath.CIRCLE, scale: 8, selectedScale: 11 };
        case "public-price":
            return { color: "#ef8f3c", path: "M 0,-10 10,0 0,10 -10,0 z", scale: .82, selectedScale: 1.08 };
        case "wholesale-market":
            return { color: "#2f6fab", path: "M -8,-5 -5,-9 5,-9 8,-5 8,8 -8,8 z", scale: .72, selectedScale: .94 };
        case "traditional-market-hub":
            return { color: "#8a4b24", path: "M -9,-2 -6,-8 6,-8 9,-2 7,0 7,9 -7,9 -7,0 z", scale: .72, selectedScale: .96 };
        case "learning-channel":
            return { color: "#6750a4", path: google.maps.SymbolPath.CIRCLE, scale: 8, selectedScale: 11 };
        case "scripture-classics":
            return { color: "#b7791f", path: "M 0,-10 10,0 0,10 -10,0 z", scale: .82, selectedScale: 1.08 };
        default:
            return { color: night ? "#6750a4" : "#ef8f3c", path: google.maps.SymbolPath.CIRCLE, scale: 8, selectedScale: 11 };
    }
}

function focusSelection(instance) {
    const bounds = new google.maps.LatLngBounds();
    instance.map.data.forEach(feature => {
        if (instance.selectedMarkerId && feature.getProperty("markerId") !== instance.selectedMarkerId) {
            return;
        }

        if (!instance.selectedMarkerId
            && instance.selectedCode
            && feature.getProperty("code") !== instance.selectedCode) {
            return;
        }

        const point = feature.getGeometry();
        if (point instanceof google.maps.Data.Point) {
            bounds.extend(point.get());
        }
    });

    if (!bounds.isEmpty()) {
        instance.map.fitBounds(bounds, mapViewportPadding(instance));
    }
}

function mapViewportPadding(instance) {
    const compact = globalThis.matchMedia?.("(max-width: 560px)")?.matches;
    if (compact) {
        return { top: 170, right: 28, bottom: 150, left: 28 };
    }

    return {
        top: 72,
        right: instance.selectedCode ? 430 : 72,
        bottom: 112,
        left: 330
    };
}

function consumeRuntimeValue(runtimeConfig, primaryName, legacyName) {
    const configName = typeof runtimeConfig?.[primaryName] === "string" ? primaryName : legacyName;
    const runtimeValue = runtimeConfig?.[configName];
    if (typeof runtimeValue === "string" && runtimeValue.trim()) {
        const value = runtimeValue.trim();
        try {
            delete runtimeConfig[configName];
        } catch {
            try {
                runtimeConfig[configName] = "";
            } catch {
                // A frozen deployment config is still readable; do not fail map loading.
            }
        }
        return value;
    }

    return "";
}

function isRuntimeOriginAllowed(runtimeConfig) {
    const currentOrigin = globalThis.location?.origin;
    const hostname = globalThis.location?.hostname?.toLowerCase();
    const isLocalDevelopment = (hostname === "localhost" || hostname === "127.0.0.1" || hostname === "::1")
        && (globalThis.location?.protocol === "http:" || globalThis.location?.protocol === "https:");
    if (isLocalDevelopment) {
        return true;
    }

    if (globalThis.location?.protocol !== "https:") {
        return false;
    }

    const allowedOrigins = runtimeConfig?.allowedOrigins ?? runtimeConfig?.googleMapsAllowedOrigins;
    return typeof currentOrigin === "string"
        && Array.isArray(allowedOrigins)
        && allowedOrigins.some(origin => origin === currentOrigin);
}

function loadGoogleMaps(apiKey) {
    if (globalThis.google?.maps?.importLibrary) {
        return Promise.resolve();
    }
    if (googleMapsLoadPromise) {
        return googleMapsLoadPromise;
    }

    googleMapsLoadPromise = new Promise((resolve, reject) => {
        const callbackName = "__ssalddelCommunityGoogleMapsReady";
        globalThis[callbackName] = () => {
            delete globalThis[callbackName];
            script.remove();
            resolve();
        };

        const parameters = new URLSearchParams({
            key: apiKey,
            loading: "async",
            callback: callbackName,
            v: "weekly",
            language: "ko",
            region: "KR",
            auth_referrer_policy: "origin"
        });
        const script = document.createElement("script");
        script.async = true;
        script.referrerPolicy = "strict-origin-when-cross-origin";
        script.src = `https://maps.googleapis.com/maps/api/js?${parameters}`;
        script.onerror = () => {
            delete globalThis[callbackName];
            script.remove();
            googleMapsLoadPromise = undefined;
            reject(new Error("Google Maps JavaScript API를 불러오지 못했습니다."));
        };
        document.head.append(script);
    });

    return googleMapsLoadPromise;
}
