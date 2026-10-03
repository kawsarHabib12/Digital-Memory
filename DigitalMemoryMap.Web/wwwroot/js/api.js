/* ==========================================================================
   Digital Memory Map - Unified API Client & Offline/GitHub Pages Demo Mock
   ========================================================================== */

const DEMO_STORAGE_KEY = 'dmm_demo_memories_v2';
const DEMO_CATEGORIES = [
    { categoryId: 1, name: 'Travel' },
    { categoryId: 2, name: 'University' },
    { categoryId: 3, name: 'Family' },
    { categoryId: 4, name: 'Friends' },
    { categoryId: 5, name: 'Food' },
    { categoryId: 6, name: 'Events' },
    { categoryId: 7, name: 'Childhood' },
    { categoryId: 8, name: 'Nature' },
    { categoryId: 9, name: 'Other' }
];

const DEMO_MOODS = [
    { moodId: 1, name: 'Happy', emoji: '😊' },
    { moodId: 2, name: 'Loved', emoji: '❤️' },
    { moodId: 3, name: 'Funny', emoji: '😂' },
    { moodId: 4, name: 'Sad', emoji: '😢' },
    { moodId: 5, name: 'Excited', emoji: '😍' },
    { moodId: 6, name: 'Peaceful', emoji: '😌' },
    { moodId: 7, name: 'Normal', emoji: '😐' }
];

const DEMO_TAGS = [
    { tagId: 1, name: '35mm' },
    { tagId: 2, name: 'Summer' },
    { tagId: 3, name: 'Roadtrip' },
    { tagId: 4, name: 'Friends' },
    { tagId: 5, name: 'Coast' }
];

function getInitialDemoMemories() {
    return [
        {
            memoryId: 1,
            title: "SAM'S APARTMENT ROOFTOP & VINYL NIGHT",
            description: "Analog records spinning, late night tea and deep conversation overlooking the cityscape.",
            memoryDate: "2026-08-14T20:30:00Z",
            latitude: 23.8103,
            longitude: 90.4125,
            locationName: "Dhanmondi, Dhaka",
            categoryId: 4,
            categoryName: "Friends",
            moodId: 5,
            moodName: "Excited",
            moodEmoji: "😍",
            tags: ["35mm", "Friends", "Night"],
            photos: [
                { photoId: 1, photoUrl: "images/house_party.jpg", isCover: true, caption: "Living room record session" }
            ],
            photoCount: 1,
            coverPhotoUrl: "images/house_party.jpg"
        },
        {
            memoryId: 2,
            title: "COASTAL TIDE LINE AT DAWN",
            description: "Golden hour sea foam rolling over black volcanic sands. Complete silence and morning salt breeze.",
            memoryDate: "2026-06-22T06:15:00Z",
            latitude: 21.4272,
            longitude: 92.0058,
            locationName: "Cox's Bazar Sea Beach",
            categoryId: 1,
            categoryName: "Travel",
            moodId: 6,
            moodName: "Peaceful",
            moodEmoji: "😌",
            tags: ["Coast", "Summer", "Morning"],
            photos: [
                { photoId: 2, photoUrl: "images/beach.jpg", isCover: true, caption: "Tide coming in" }
            ],
            photoCount: 1,
            coverPhotoUrl: "images/beach.jpg"
        },
        {
            memoryId: 3,
            title: "CROSSING THE ALPINE HIGH PASS",
            description: "A 400-mile road trip cutting through fog and high elevation pines. Crisp crisp alpine air.",
            memoryDate: "2026-04-10T14:45:00Z",
            latitude: 28.5355,
            longitude: 83.8780,
            locationName: "Mountain Passway",
            categoryId: 8,
            categoryName: "Nature",
            moodId: 1,
            moodName: "Happy",
            moodEmoji: "😊",
            tags: ["Roadtrip", "Mountains"],
            photos: [
                { photoId: 3, photoUrl: "images/roadtrip.jpg", isCover: true, caption: "High mountain road" }
            ],
            photoCount: 1,
            coverPhotoUrl: "images/roadtrip.jpg"
        }
    ];
}

function getStoredMemories() {
    const raw = localStorage.getItem(DEMO_STORAGE_KEY);
    if (!raw) {
        const initial = getInitialDemoMemories();
        localStorage.setItem(DEMO_STORAGE_KEY, JSON.stringify(initial));
        return initial;
    }
    try {
        return JSON.parse(raw);
    } catch {
        return getInitialDemoMemories();
    }
}

function saveStoredMemories(memories) {
    localStorage.setItem(DEMO_STORAGE_KEY, JSON.stringify(memories));
}

const isGitHubPagesOrStatic = () => {
    return window.location.hostname.endsWith('github.io') || window.location.protocol === 'file:';
};

const MockBackend = {
    handle(url, options = {}) {
        const method = (options.method || 'GET').toUpperCase();
        const urlObj = new URL(url, window.location.origin);
        const path = urlObj.pathname.replace(/^\/Digital-Memory/, '');
        const params = urlObj.searchParams;

        // Auth
        if (path === '/api/auth/me') {
            const user = Auth.getUser();
            if (!user) throw new Error('Unauthorized');
            return user;
        }
        if (path === '/api/auth/login' || path === '/api/auth/register') {
            let body = {};
            if (options.body) {
                try { body = typeof options.body === 'string' ? JSON.parse(options.body) : options.body; } catch {}
            }
            const email = body.email || "curator@digitalmemory.io";
            const isAdmin = email.toLowerCase().includes('admin');
            const user = {
                userId: 1,
                fullName: body.fullName || (email.split('@')[0].toUpperCase()),
                email: email,
                role: isAdmin ? "Admin" : "User"
            };
            Auth.setUser(user);
            return user;
        }
        if (path === '/api/auth/logout') {
            Auth.clearUser();
            return { message: "Logged out" };
        }

        // Lookups
        if (path === '/api/categories') return DEMO_CATEGORIES;
        if (path === '/api/moods') return DEMO_MOODS;
        if (path === '/api/tags') return DEMO_TAGS;

        // Memories Stats
        if (path === '/api/memories/stats') {
            const list = getStoredMemories();
            return {
                totalMemories: list.length,
                totalLocations: new Set(list.map(m => m.locationName)).size,
                totalPhotos: list.reduce((sum, m) => sum + (m.photoCount || (m.photos ? m.photos.length : 1)), 0),
                favoriteCategory: "Travel & Expeditions"
            };
        }

        // Memories List & Filter
        if (path === '/api/memories') {
            const list = getStoredMemories();
            if (method === 'GET') {
                let filtered = [...list];
                const search = params.get('search');
                const catId = params.get('categoryId');
                const moodId = params.get('moodId');

                if (search) {
                    const q = search.toLowerCase();
                    filtered = filtered.filter(m => 
                        (m.title && m.title.toLowerCase().includes(q)) ||
                        (m.description && m.description.toLowerCase().includes(q)) ||
                        (m.locationName && m.locationName.toLowerCase().includes(q))
                    );
                }
                if (catId) {
                    filtered = filtered.filter(m => m.categoryId == catId);
                }
                if (moodId) {
                    filtered = filtered.filter(m => m.moodId == moodId);
                }
                return filtered;
            }

            if (method === 'POST') {
                let newMem = {};
                if (options.body instanceof FormData) {
                    newMem = {
                        title: options.body.get('title') || 'Untitled Memory',
                        description: options.body.get('description') || '',
                        memoryDate: options.body.get('memoryDate') || new Date().toISOString(),
                        latitude: parseFloat(options.body.get('latitude')) || 23.8103,
                        longitude: parseFloat(options.body.get('longitude')) || 90.4125,
                        locationName: options.body.get('locationName') || 'Pinned Location',
                        categoryId: parseInt(options.body.get('categoryId')) || 1,
                        moodId: parseInt(options.body.get('moodId')) || 1
                    };
                } else if (typeof options.body === 'string') {
                    try { newMem = JSON.parse(options.body); } catch {}
                }

                const cat = DEMO_CATEGORIES.find(c => c.categoryId === newMem.categoryId) || DEMO_CATEGORIES[0];
                const mood = DEMO_MOODS.find(m => m.moodId === newMem.moodId) || DEMO_MOODS[0];

                const created = {
                    memoryId: Date.now(),
                    title: newMem.title,
                    description: newMem.description,
                    memoryDate: newMem.memoryDate,
                    latitude: newMem.latitude,
                    longitude: newMem.longitude,
                    locationName: newMem.locationName,
                    categoryId: cat.categoryId,
                    categoryName: cat.name,
                    moodId: mood.moodId,
                    moodName: mood.name,
                    moodEmoji: mood.emoji,
                    tags: ['Archive'],
                    photos: [
                        { photoId: Date.now(), photoUrl: 'images/beach.jpg', isCover: true, caption: 'New Archival Photo' }
                    ],
                    photoCount: 1,
                    coverPhotoUrl: 'images/beach.jpg'
                };

                list.unshift(created);
                saveStoredMemories(list);
                return created;
            }
        }

        // Memory By ID
        const memDetailMatch = path.match(/^\/api\/memories\/(\d+)/);
        if (memDetailMatch) {
            const id = parseInt(memDetailMatch[1]);
            const list = getStoredMemories();
            const idx = list.findIndex(m => m.memoryId === id);

            if (method === 'GET') {
                if (idx === -1) throw new Error('Memory not found');
                return list[idx];
            }
            if (method === 'DELETE') {
                if (idx !== -1) {
                    list.splice(idx, 1);
                    saveStoredMemories(list);
                }
                return { success: true };
            }
            if (method === 'PUT') {
                let updateData = {};
                try { updateData = JSON.parse(options.body); } catch {}
                if (idx !== -1) {
                    list[idx] = { ...list[idx], ...updateData };
                    saveStoredMemories(list);
                    return list[idx];
                }
            }
        }

        // Profile
        if (path === '/api/profile') {
            return Auth.getUser() || { userId: 1, fullName: "Guest Archivist", email: "curator@digitalmemory.io" };
        }
        if (path === '/api/profile/change-password') {
            return { message: "Password updated successfully." };
        }

        // Admin Endpoints
        if (path === '/api/admin/stats') {
            const list = getStoredMemories();
            return {
                totalUsers: 14,
                activeUsers: 12,
                newUsersThisWeek: 4,
                totalMemories: list.length,
                totalPhotos: list.reduce((sum, m) => sum + (m.photoCount || (m.photos ? m.photos.length : 1)), 0),
                categoryStats: DEMO_CATEGORIES.map(c => ({
                    categoryName: c.name,
                    memoryCount: list.filter(m => m.categoryId === c.categoryId).length
                }))
            };
        }
        if (path === '/api/admin/users') {
            return [
                { userId: 1, fullName: "System Admin", email: "admin@digitalmemory.com", role: "Admin", isActive: true, createdAt: "2026-01-01T00:00:00Z" },
                { userId: 2, fullName: "Kawsar Habib", email: "kawsar@digitalmemory.io", role: "User", isActive: true, createdAt: "2026-02-15T00:00:00Z" },
                { userId: 3, fullName: "Archivist Member", email: "member@digitalmemory.io", role: "User", isActive: true, createdAt: "2026-03-10T00:00:00Z" }
            ];
        }
        if (path === '/api/admin/memories') {
            return getStoredMemories();
        }

        return {};
    }
};

const API = {
    async request(url, options = {}) {
        // If deployed to GitHub Pages or static host, use mock handler
        if (isGitHubPagesOrStatic()) {
            return MockBackend.handle(url, options);
        }

        const defaultHeaders = {
            'Accept': 'application/json'
        };

        if (!(options.body instanceof FormData)) {
            defaultHeaders['Content-Type'] = 'application/json';
        }

        const config = {
            ...options,
            headers: {
                ...defaultHeaders,
                ...options.headers
            },
            credentials: 'same-origin'
        };

        try {
            const response = await fetch(url, config);

            if (response.status === 204) {
                return null;
            }

            let data;
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                data = await response.json();
            } else {
                data = await response.text();
            }

            if (!response.ok) {
                if (response.status === 401 && !url.includes('/api/auth/login') && !url.includes('/api/auth/register')) {
                    localStorage.removeItem('dmm_user');
                    window.location.href = Auth.url ? Auth.url('login.html?expired=1') : '/login.html?expired=1';
                    throw new Error('Please log in.');
                }

                const errorMsg = data?.message || (typeof data === 'string' ? data : 'An error occurred.');
                const fieldErrors = data?.errors;
                const err = new Error(errorMsg);
                err.status = response.status;
                err.errors = fieldErrors;
                throw err;
            }

            return data;
        } catch (error) {
            // If offline/fetch error occurs, fallback to mock demo
            if (error.name === 'TypeError' && error.message.includes('fetch')) {
                console.warn('Backend server not reachable, switching to local demo mode for request:', url);
                return MockBackend.handle(url, options);
            }
            console.error(`API Error [${url}]:`, error);
            throw error;
        }
    },

    get(url, params = null) {
        if (params) {
            const query = new URLSearchParams();
            Object.entries(params).forEach(([key, val]) => {
                if (val !== undefined && val !== null && val !== '') {
                    query.append(key, val);
                }
            });
            const qs = query.toString();
            if (qs) {
                url += (url.includes('?') ? '&' : '?') + qs;
            }
        }
        return this.request(url, { method: 'GET' });
    },

    post(url, body) {
        return this.request(url, {
            method: 'POST',
            body: body instanceof FormData ? body : JSON.stringify(body)
        });
    },

    put(url, body) {
        return this.request(url, {
            method: 'PUT',
            body: JSON.stringify(body)
        });
    },

    delete(url) {
        return this.request(url, { method: 'DELETE' });
    }
};

/* Toast Notifications */
const Toast = {
    container: null,

    init() {
        if (!this.container) {
            this.container = document.createElement('div');
            this.container.className = 'toast-container';
            document.body.appendChild(this.container);
        }
    },

    show(message, type = 'info', duration = 3500) {
        this.init();
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        
        let icon = 'ℹ️';
        if (type === 'success') icon = '✅';
        if (type === 'error') icon = '❌';

        toast.innerHTML = `<span>${icon}</span><div style="flex:1; font-size:0.9rem;">${message}</div>`;
        this.container.appendChild(toast);

        setTimeout(() => {
            toast.style.opacity = '0';
            toast.style.transform = 'translateX(100%)';
            toast.style.transition = 'all 0.3s ease';
            setTimeout(() => toast.remove(), 300);
        }, duration);
    },

    success(msg) { this.show(msg, 'success'); },
    error(msg) { this.show(msg, 'error', 4500); },
    info(msg) { this.show(msg, 'info'); }
};
