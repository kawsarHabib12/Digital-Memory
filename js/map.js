/* ==========================================================================
   Digital Memory Map - Leaflet & Nominatim Map Helper
   ========================================================================== */

const MapHelper = {
    lastSearchTime: 0,

    initMap(elementId, options = {}) {
        const center = options.center || [23.6850, 90.3563]; // Default Bangladesh
        const zoom = options.zoom || 7;

        const map = L.map(elementId, {
            center: center,
            zoom: zoom,
            zoomControl: true
        });

        // OpenStreetMap Tile Layer with required attribution
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright" target="_blank">OpenStreetMap</a> contributors'
        }).addTo(map);

        return map;
    },

    createMarkerIcon(emoji) {
        return L.divIcon({
            className: 'custom-map-pin',
            html: `<div class="archival-pin-marker" title="Memory Pin">${emoji || '📍'}</div>`,
            iconSize: [36, 36],
            iconAnchor: [18, 36],
            popupAnchor: [0, -36]
        });
    },

    renderMemoryMarkers(map, memoryPins, markerLayerGroup) {
        if (markerLayerGroup) {
            markerLayerGroup.clearLayers();
        } else {
            markerLayerGroup = L.layerGroup().addTo(map);
        }

        const markers = [];

        memoryPins.forEach(pin => {
            const icon = this.createMarkerIcon(pin.moodEmoji || '📍');
            const marker = L.marker([pin.latitude, pin.longitude], { icon: icon });

            const formattedDate = new Date(pin.memoryDate).toLocaleDateString('en-GB', {
                year: 'numeric',
                month: '2-digit',
                day: '2-digit'
            }).replace(/\//g, '.');

            const popupContent = `
                <div class="map-popup-card">
                    ${pin.thumbnailUrl 
                        ? `<img src="${pin.thumbnailUrl}" class="map-popup-thumb" alt="${escapeHtml(pin.title)}" onerror="this.style.display='none'" />` 
                        : `<div style="height:90px; background:#0a0a0c; display:flex; align-items:center; justify-content:center; color:#ffffff; font-size:2rem; border-bottom:2px solid #0a0a0c;">${pin.moodEmoji || '🗺️'}</div>`
                    }
                    <div class="map-popup-body">
                        <div class="map-popup-meta">
                            <span class="badge badge-primary">${escapeHtml(pin.category)}</span>
                            <span class="mono-text" style="font-weight:700;">${formattedDate}</span>
                        </div>
                        <h4 class="map-popup-title">${escapeHtml(pin.title)}</h4>
                        ${pin.locationName ? `<div style="font-size:0.82rem; color:var(--text-secondary); margin-bottom:0.75rem; font-family:'Space Grotesk',sans-serif;">📍 ${escapeHtml(pin.locationName)}</div>` : ''}
                        <div style="margin-top:0.75rem;">
                            <a href="${typeof Auth !== 'undefined' && Auth.url ? Auth.url('memory-details.html?id=' + pin.memoryId) : 'memory-details.html?id=' + pin.memoryId}" class="btn btn-primary btn-sm" style="width:100%; letter-spacing:0.06em;">View Memory &rarr;</a>
                        </div>
                    </div>
                </div>
            `;

            marker.bindPopup(popupContent);
            markerLayerGroup.addLayer(marker);
            markers.push(marker);
        });

        if (markers.length > 0 && memoryPins.length > 1) {
            const group = new L.featureGroup(markers);
            map.fitBounds(group.getBounds().pad(0.15));
        }

        return markerLayerGroup;
    },

    // Debounced Nominatim Place Search (respecting max 1 req/sec policy)
    async searchLocation(query) {
        if (!query || query.trim().length < 3) return [];

        const now = Date.now();
        const timeSinceLast = now - this.lastSearchTime;
        if (timeSinceLast < 1000) {
            await new Promise(r => setTimeout(r, 1000 - timeSinceLast));
        }

        this.lastSearchTime = Date.now();

        const url = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}&limit=5`;
        try {
            const res = await fetch(url, {
                headers: {
                    'Accept-Language': 'en'
                }
            });
            if (!res.ok) return [];
            return await res.json();
        } catch (err) {
            console.error('Nominatim search failed:', err);
            return [];
        }
    },

    // Reverse geocode lat/lng to human-readable address
    async reverseGeocode(lat, lng) {
        const now = Date.now();
        const timeSinceLast = now - this.lastSearchTime;
        if (timeSinceLast < 1000) {
            await new Promise(r => setTimeout(r, 1000 - timeSinceLast));
        }

        this.lastSearchTime = Date.now();

        const url = `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}`;
        try {
            const res = await fetch(url, {
                headers: {
                    'Accept-Language': 'en'
                }
            });
            if (!res.ok) return null;
            const data = await res.json();
            return data.display_name || data.name || null;
        } catch (err) {
            console.error('Reverse geocode failed:', err);
            return null;
        }
    }
};
