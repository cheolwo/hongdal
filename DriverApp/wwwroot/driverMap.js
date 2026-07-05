window.driverMap = (function () {
    let map;
    let markers = [];
    let selectedMarker;
    let selectedItem;
    let infoWindow;
    let naverMapsLoadingPromise;

    function init(containerId, latitude, longitude, zoom, items, dotnetRef) {
        return loadNaverMapsAsync()
            .then(() => {
                initMap(containerId, latitude, longitude, zoom, items, dotnetRef);
                return true;
            })
            .catch(error => {
                renderError(containerId, error);
                return false;
            });
    }

    function loadNaverMapsAsync() {
        if (window.naver?.maps) {
            return Promise.resolve(true);
        }

        if (!naverMapsLoadingPromise) {
            naverMapsLoadingPromise = new Promise((resolve, reject) => {
                const clientId = resolveNaverClientId();
                if (!clientId) {
                    naverMapsLoadingPromise = null;
                    reject(new Error('Naver Maps SDK client id is not configured.'));
                    return;
                }

                const callbackName = `__hongdalNaverMapReady_${Date.now()}`;
                window[callbackName] = function () {
                    delete window[callbackName];
                    resolve(true);
                };

                const script = document.createElement('script');
                script.id = 'naver-maps-sdk';
                script.async = true;
                script.src = `https://oapi.map.naver.com/openapi/v3/maps.js?ncpKeyId=${encodeURIComponent(clientId)}&callback=${callbackName}`;
                script.onerror = () => {
                    delete window[callbackName];
                    naverMapsLoadingPromise = null;
                    reject(new Error('Naver Maps SDK could not be loaded.'));
                };

                document.head.appendChild(script);
            });
        }

        return naverMapsLoadingPromise;
    }

    function resolveNaverClientId() {
        const runtimeValue = window.driverMapOptions?.naverClientId;
        if (runtimeValue) {
            return runtimeValue;
        }

        const meta = document.querySelector('meta[name="naver-map-client-id"]');
        return meta?.content?.trim() || '';
    }

    function initMap(containerId, latitude, longitude, zoom, items, dotnetRef) {
        const container = document.getElementById(containerId);
        if (!container || !window.naver?.maps) {
            return;
        }

        clearMap();

        const center = new naver.maps.LatLng(latitude, longitude);
        map = new naver.maps.Map(container, {
            center,
            zoom: zoom ?? 12,
            mapTypeControl: false,
            scaleControl: false,
            logoControl: true,
            mapDataControl: false,
            zoomControl: true,
            zoomControlOptions: {
                position: naver.maps.Position.TOP_RIGHT
            }
        });

        infoWindow = new naver.maps.InfoWindow({
            borderWidth: 0,
            anchorSize: new naver.maps.Size(12, 8),
            backgroundColor: 'transparent'
        });

        const bounds = new naver.maps.LatLngBounds(center, center);
        addDriverLocationMarker(latitude, longitude);

        (items || []).forEach(item => {
            const marker = addPickupMarker(item, dotnetRef);
            if (marker) {
                markers.push(marker);
                bounds.extend(marker.getPosition());
            }
        });

        if (markers.length > 0) {
            map.fitBounds(bounds, {
                top: 96,
                right: 48,
                bottom: 220,
                left: 48
            });
        }
    }

    function clearMap() {
        if (infoWindow) {
            infoWindow.close();
            infoWindow = null;
        }

        markers.forEach(marker => marker.setMap(null));
        markers = [];
        selectedMarker = null;
        selectedItem = null;
        map = null;
    }

    function addDriverLocationMarker(latitude, longitude) {
        const marker = new naver.maps.Marker({
            position: new naver.maps.LatLng(latitude, longitude),
            map,
            title: '기사 현재 위치',
            zIndex: 300,
            icon: {
                content: '<div class="driver-location-marker"><span class="driver-location-marker__dot"></span></div>',
                size: new naver.maps.Size(20, 20),
                anchor: new naver.maps.Point(10, 10)
            }
        });

        markers.push(marker);
        return marker;
    }

    function addPickupMarker(item, dotnetRef) {
        const pickupLatitude = numberValue(item, ['pickupLatitude', 'PickupLatitude', '픽업위도', '상차위도', '픽업_위도', '상차_위도']);
        const pickupLongitude = numberValue(item, ['pickupLongitude', 'PickupLongitude', '픽업경도', '상차경도', '픽업_경도', '상차_경도']);

        if (!Number.isFinite(pickupLatitude) || !Number.isFinite(pickupLongitude)) {
            return null;
        }

        const marker = new naver.maps.Marker({
            position: new naver.maps.LatLng(pickupLatitude, pickupLongitude),
            map,
            title: stringValue(item, ['title', 'Title', '제목', '화물종류']),
            zIndex: 200,
            icon: createPickupMarkerIcon(false)
        });

        naver.maps.Event.addListener(marker, 'click', async () => {
            await selectRequestAsync(item, marker, dotnetRef);
        });

        return marker;
    }

    function createPickupMarkerIcon(selected) {
        const className = selected
            ? 'driver-marker driver-marker--pickup driver-marker--selected'
            : 'driver-marker driver-marker--pickup';

        const size = selected ? 24 : 20;
        const anchor = selected ? 12 : 10;

        return {
            content: `<div class="${className}"><span class="driver-marker__dot"></span></div>`,
            size: new naver.maps.Size(size, size),
            anchor: new naver.maps.Point(anchor, anchor)
        };
    }

    async function selectRequestAsync(item, marker, dotnetRef) {
        if (!map || !item) {
            return;
        }

        if (selectedMarker && selectedMarker !== marker) {
            selectedMarker.setIcon(createPickupMarkerIcon(false));
        }

        selectedMarker = marker;
        selectedItem = item;
        selectedMarker.setIcon(createPickupMarkerIcon(true));
        selectedMarker.setZIndex(400);
        map.panTo(selectedMarker.getPosition());

        if (infoWindow) {
            infoWindow.setContent(createInfoWindowContent(item));
            infoWindow.open(map, selectedMarker);
        }

        if (dotnetRef) {
            await dotnetRef.invokeMethodAsync('SelectRequestFromMap', requestId(item), '픽업지');
        }
    }

    function createInfoWindowContent(item) {
        const title = stringValue(item, ['title', 'Title', '제목', '화물종류']) || '운송 의뢰';
        const pickupAddress = stringValue(item, ['pickupAddress', 'PickupAddress', '픽업지', '상차지']) || '픽업지 정보 없음';
        const summary = stringValue(item, ['summary', 'Summary', '요약', '요약설명']);

        return [
            '<div class="driver-map-info-window">',
            `<strong>${escapeHtml(title)}</strong>`,
            `<span>픽업지: ${escapeHtml(pickupAddress)}</span>`,
            summary ? `<small>${escapeHtml(summary)}</small>` : '',
            `<small>${escapeHtml(requestId(item))}</small>`,
            '</div>'
        ].join('');
    }

    function requestId(item) {
        return stringValue(item, ['requestId', 'RequestId', '의뢰Id', '운송의뢰Id', 'id', 'Id']);
    }

    function stringValue(item, names) {
        for (const name of names) {
            if (item && Object.prototype.hasOwnProperty.call(item, name) && item[name] !== null && item[name] !== undefined) {
                return String(item[name]);
            }
        }

        return '';
    }

    function numberValue(item, names) {
        for (const name of names) {
            if (item && Object.prototype.hasOwnProperty.call(item, name)) {
                const value = Number(item[name]);
                if (Number.isFinite(value) && value !== 0) {
                    return value;
                }
            }
        }

        return Number.NaN;
    }

    function renderError(containerId, error) {
        const container = document.getElementById(containerId);
        if (!container) {
            return;
        }

        container.innerHTML = [
            '<div class="driver-map-error">',
            '<strong>네이버 지도를 불러오지 못했습니다.</strong>',
            `<span>${escapeHtml(error?.message || '지도 초기화 오류')}</span>`,
            '</div>'
        ].join('');
    }

    function escapeHtml(value) {
        return String(value ?? '')
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#39;');
    }

    return {
        init
    };
})();
