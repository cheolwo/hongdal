# Naver Maps in DriverApp

DriverApp uses NAVER Maps through the Android native map path.

- Android native map: `DriverNativeMapViewHandler.Android.cs` renders `MapView` from the NAVER Maps Android SDK binding project.
- The previous Blazor `driverMap.js` JavaScript map path has been removed so the driver app has one map implementation.

## Current Android Native Map Scope

- SDK key is declared through Android manifest metadata: `com.naver.maps.map.CLIENT_ID`.
- SDK dependency is supplied by `DriverApp.NaverMaps.Android`.
- Driver current location is represented by `DriverNativeMapView.CenterLatitude` and `CenterLongitude`.
- Pickup and dropoff markers are supplied through `DriverMapMarkerItem`.
- Traffic layer, compass, scale bar, zoom controls, location button, zoom limits, and current location overlay are controlled by `DriverNativeMapView`.
- Map locale is forced to `ko-KR` when the NAVER map is ready so base map labels prefer Korean in the driver experience.

## Required Android Permissions

- `INTERNET`
- `ACCESS_NETWORK_STATE`
- `ACCESS_COARSE_LOCATION`
- `ACCESS_FINE_LOCATION`

The current implementation declares the permissions and renders a location overlay from app state. Runtime permission request and live GPS tracking should be connected later when dispatch receiving depends on actual device location.

## Next Work

- Request runtime location permission before enabling live GPS tracking.
- Connect MAUI `Geolocation` updates to `DriverNativeMapView.CenterLatitude` and `CenterLongitude`.
- Add route polyline overlays between pickup and dropoff.
- Add marker clustering when recommendation volume grows.
- Move the NAVER Maps key out of committed sample strings for production builds.
