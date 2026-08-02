const instances = new Map();
let googleMapsLoadPromise;

const dayMapStyle = [];
const nightMapStyle = [
    { elementType: "geometry", stylers: [{ color: "#24213f" }] },
    { elementType: "labels.text.fill", stylers: [{ color: "#d8d3f4" }] },
    { elementType: "labels.text.stroke", stylers: [{ color: "#24213f" }] },
    { featureType: "administrative.country", elementType: "geometry.stroke", stylers: [{ color: "#77709d" }] },
    { featureType: "poi", stylers: [{ visibility: "off" }] },
    { featureType: "road", stylers: [{ visibility: "off" }] },
    { featureType: "transit", stylers: [{ visibility: "off" }] },
    { featureType: "water", elementType: "geometry", stylers: [{ color: "#17172f" }] }
];

export async function initialize(elementId, markers, datasetCode, selectedCode, dotNetReference) {
    const apiKey = readRuntimeValue("googleMapsBrowserApiKey", "ssalddel-google-maps-browser-key");
    if (!apiKey) {
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
            minZoom: 1,
            maxZoom: 8,
            mapTypeControl: false,
            streetViewControl: false,
            fullscreenControl: true,
            clickableIcons: false,
            gestureHandling: "cooperative"
        });

        const instance = {
            map,
            dotNetReference,
            selectedCode,
            datasetCode,
            clickListener: map.data.addListener("click", event => {
                const countryCode = event.feature.getProperty("code");
                if (countryCode && instance.dotNetReference) {
                    instance.dotNetReference.invokeMethodAsync("SelectCountryFromGoogleMap", countryCode);
                }
            })
        };

        instances.set(elementId, instance);
        updateDataset(elementId, markers, datasetCode, selectedCode);
        return "ready";
    } catch {
        return "failed";
    }
}

export function updateDataset(elementId, markers, datasetCode, selectedCode) {
    const instance = instances.get(elementId);
    if (!instance) {
        return;
    }

    instance.datasetCode = datasetCode;
    instance.selectedCode = selectedCode;
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

    if (!bounds.isEmpty()) {
        instance.map.fitBounds(bounds, 64);
    }
}

export function updateSelection(elementId, selectedCode) {
    const instance = instances.get(elementId);
    if (!instance) {
        return;
    }

    instance.selectedCode = selectedCode;
    applyDataStyle(instance);
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
        const selected = feature.getProperty("code") === instance.selectedCode;
        const layerCode = feature.getProperty("layerCode");
        const layerStyle = markerStyleFor(layerCode, night);
        return {
            title: `${feature.getProperty("name")} · ${feature.getProperty("dataLabel")}`,
            icon: {
                path: layerStyle.path,
                scale: selected ? 11 : 8,
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
            return { color: "#176b4d", path: google.maps.SymbolPath.CIRCLE };
        case "public-price":
            return { color: "#ef8f3c", path: "M 0,-10 10,0 0,10 -10,0 z" };
        case "learning-channel":
            return { color: "#6750a4", path: google.maps.SymbolPath.CIRCLE };
        case "scripture-classics":
            return { color: "#b7791f", path: "M 0,-10 10,0 0,10 -10,0 z" };
        default:
            return { color: night ? "#6750a4" : "#ef8f3c", path: google.maps.SymbolPath.CIRCLE };
    }
}

function readRuntimeValue(configName, metaName) {
    const runtimeValue = globalThis.ssalddelRuntimeConfig?.[configName];
    if (typeof runtimeValue === "string" && runtimeValue.trim()) {
        return runtimeValue.trim();
    }

    const metaValue = document.querySelector(`meta[name="${metaName}"]`)?.content;
    return typeof metaValue === "string" ? metaValue.trim() : "";
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
        script.src = `https://maps.googleapis.com/maps/api/js?${parameters}`;
        script.onerror = () => {
            delete globalThis[callbackName];
            googleMapsLoadPromise = undefined;
            reject(new Error("Google Maps JavaScript API를 불러오지 못했습니다."));
        };
        document.head.append(script);
    });

    return googleMapsLoadPromise;
}
