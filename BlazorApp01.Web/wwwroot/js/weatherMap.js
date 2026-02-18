window.weatherMap = (function () {
    const maps = new Map();

    function init(elementId, lat, lon, dotNetRef) {
        const el = document.getElementById(elementId);
        if (!el) return;

        // If Leaflet isn't loaded yet, dynamically load CSS and script, then initialize.
        function ensureLeafletLoaded(callback) {
            if (typeof L !== 'undefined') {
                callback();
                return;
            }

            // Load CSS
            if (!document.querySelector('link[data-weather-leaflet]')) {
                const link = document.createElement('link');
                link.rel = 'stylesheet';
                link.href = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.css';
                link.setAttribute('data-weather-leaflet', '1');
                document.head.appendChild(link);
            }

            // Load script
            if (!document.querySelector('script[data-weather-leaflet]')) {
                const script = document.createElement('script');
                script.src = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.js';
                script.async = true;
                script.setAttribute('data-weather-leaflet', '1');
                script.onload = function () { callback(); };
                script.onerror = function () { console.error('Failed to load Leaflet script'); };
                document.head.appendChild(script);
            } else {
                // Script tag exists but leaflet may not be ready yet; wait briefly
                const check = setInterval(function () {
                    if (typeof L !== 'undefined') {
                        clearInterval(check);
                        callback();
                    }
                }, 50);
            }
        }

        ensureLeafletLoaded(function () {
            // If map was already initialized for this element, update marker/view instead
            const existing = maps.get(elementId);
            if (existing) {
                existing.map.setView([lat, lon]);
                existing.marker.setLatLng([lat, lon]);
                existing.dotNetRef = dotNetRef;
                return;
            }

            // Create map
            const map = L.map(el).setView([lat, lon], 10);

            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                maxZoom: 19,
                attribution: '© OpenStreetMap'
            }).addTo(map);

            // Add marker
            const marker = L.marker([lat, lon], { draggable: true }).addTo(map);

            marker.on('dragend', function (e) {
                const p = e.target.getLatLng();
                if (dotNetRef && dotNetRef.invokeMethodAsync) {
                    dotNetRef.invokeMethodAsync('NotifyLocationChanged', p.lat, p.lng).catch(function (err) { console.error(err); });
                }
            });

            map.on('click', function (e) {
                const p = e.latlng;
                marker.setLatLng(p);
                if (dotNetRef && dotNetRef.invokeMethodAsync) {
                    dotNetRef.invokeMethodAsync('NotifyLocationChanged', p.lat, p.lng).catch(function (err) { console.error(err); });
                }
            });

            maps.set(elementId, { map, marker, dotNetRef });
        });
    }

    function setLocation(elementId, lat, lon) {
        const entry = maps.get(elementId);
        if (!entry) return;
        entry.map.setView([lat, lon]);
        entry.marker.setLatLng([lat, lon]);
    }

    return {
        init: init,
        setLocation: setLocation
    };
})();
