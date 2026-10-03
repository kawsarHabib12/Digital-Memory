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
            userId: 1,
            userName: "Atlas Curator",
            userEmail: "demo@digitalmemory.io",
            visibility: 1, // Public
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
            userId: 1,
            userName: "Atlas Curator",
            userEmail: "demo@digitalmemory.io",
            visibility: 1, // Public
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
            userId: 1,
            userName: "Atlas Curator",
            userEmail: "demo@digitalmemory.io",
            visibility: 1, // Public
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

const DEMO_USERS_KEY = 'dmm_demo_users_v2';

function getStoredUsers() {
    const raw = localStorage.getItem(DEMO_USERS_KEY);
    if (!raw) {
        const initial = [
            { userId: 999, email: "admin@digitalmemory.com", fullName: "Chief Archivist", role: "Admin", password: "Admin@123" },
            { userId: 1, email: "demo@digitalmemory.io", fullName: "Atlas Curator", role: "User", password: "User@123" }
        ];
        localStorage.setItem(DEMO_USERS_KEY, JSON.stringify(initial));
        return initial;
    }
    try {
        return JSON.parse(raw);
    } catch {
        return [];
    }
}

function saveStoredUsers(users) {
    localStorage.setItem(DEMO_USERS_KEY, JSON.stringify(users));
}

function getStoredMemories() {
    const raw = localStorage.getItem(DEMO_STORAGE_KEY);
    let list = [];
    if (!raw) {
        list = getInitialDemoMemories();
        localStorage.setItem(DEMO_STORAGE_KEY, JSON.stringify(list));
        return list;
    }
    try {
        list = JSON.parse(raw);
    } catch {
        list = getInitialDemoMemories();
    }

    // Ensure all stored records have valid security classification (0 Private, 1 Public) & user metadata
    let hasChanges = false;
    list = list.map(m => {
        let vis = m.visibility;
        if (vis === undefined || vis === null) {
            vis = 1; // Default legacy sample items to public
            hasChanges = true;
        }
        let uId = m.userId;
        if (!uId) {
            uId = 1;
            hasChanges = true;
        }
        return {
            ...m,
            visibility: parseInt(vis),
            userId: parseInt(uId),
            userName: m.userName || 'Archivist',
            userEmail: m.userEmail || 'curator@digitalmemory.io'
        };
    });

    if (hasChanges) {
        localStorage.setItem(DEMO_STORAGE_KEY, JSON.stringify(list));
    }
    return list;
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

        if (path === '/api/auth/register') {
            let body = {};
            if (options.body) {
                try { body = typeof options.body === 'string' ? JSON.parse(options.body) : options.body; } catch {}
            }
            const email = (body.email || "").trim().toLowerCase();
            if (!email) throw new Error("Email is required.");
            const users = getStoredUsers();
            const existing = users.find(u => u.email.toLowerCase() === email);
            if (existing) {
                throw new Error("An account with this email address already exists.");
            }
            const isAdmin = email.includes('admin');
            const newUser = {
                userId: Date.now(),
                email: email,
                fullName: body.fullName || (email.split('@')[0].toUpperCase()),
                password: body.password || "Password@123",
                role: isAdmin ? "Admin" : "User"
            };
            users.push(newUser);
            saveStoredUsers(users);

            const userToReturn = {
                userId: newUser.userId,
                fullName: newUser.fullName,
                email: newUser.email,
                role: newUser.role
            };
            Auth.setUser(userToReturn);
            return userToReturn;
        }

        if (path === '/api/auth/login') {
            let body = {};
            if (options.body) {
                try { body = typeof options.body === 'string' ? JSON.parse(options.body) : options.body; } catch {}
            }
            const email = (body.email || "").trim().toLowerCase();
            const users = getStoredUsers();
            let matched = users.find(u => u.email.toLowerCase() === email);
            if (!matched) {
                const isAdmin = email.includes('admin');
                const uId = isAdmin ? 999 : (email === 'demo@digitalmemory.io' ? 1 : Math.abs(email.split('').reduce((acc, char) => ((acc << 5) - acc) + char.charCodeAt(0), 0)));
                matched = {
                    userId: uId,
                    email: email || "curator@digitalmemory.io",
                    fullName: body.fullName || (email ? email.split('@')[0].toUpperCase() : "CURATOR"),
                    password: body.password || "Password@123",
                    role: isAdmin ? "Admin" : "User"
                };
                users.push(matched);
                saveStoredUsers(users);
            }
            const userToReturn = {
                userId: matched.userId,
                fullName: matched.fullName,
                email: matched.email,
                role: matched.role
            };
            Auth.setUser(userToReturn);
            return userToReturn;
        }

        if (path === '/api/auth/logout') {
            Auth.clearUser();
            return { message: "Logged out" };
        }

        // Lookups
        if (path === '/api/categories') return DEMO_CATEGORIES;
        if (path === '/api/moods') return DEMO_MOODS;
        if (path === '/api/tags') return DEMO_TAGS;

        // Current User & Security Context
        const currentUser = Auth.getUser();
        const isAdmin = currentUser && currentUser.role === 'Admin';

        // Memories Stats (Scoped to current user's archive or admin total)
        if (path === '/api/memories/stats') {
            const list = getStoredMemories();
            const userList = list.filter(m => {
                if (isAdmin) return true;
                if (currentUser) return m.userId === currentUser.userId;
                return m.visibility === 1;
            });
            return {
                totalMemories: userList.length,
                totalLocations: new Set(userList.map(m => m.locationName)).size,
                totalPhotos: userList.reduce((sum, m) => sum + (m.photoCount || (m.photos ? m.photos.length : 1)), 0),
                favoriteCategory: "Travel & Expeditions"
            };
        }

        // Map Pins endpoint:
        // - Admin: sees all pins
        // - Logged in User: sees THEIR OWN pins (both private 🔒 & public 🌐) + OTHER USERS' PUBLIC pins (🌐)
        // - NEVER shows another user's private pin (visibility === 0)
        if (path === '/api/memories/map') {
            const list = getStoredMemories();
            let filtered = list.filter(m => {
                if (isAdmin) return true;
                if (currentUser) {
                    return m.userId === currentUser.userId || m.visibility === 1;
                }
                return m.visibility === 1;
            });

            const catId = params.get('categoryId');
            const moodId = params.get('moodId');
            if (catId) filtered = filtered.filter(m => m.categoryId == catId);
            if (moodId) filtered = filtered.filter(m => m.moodId == moodId);

            return filtered.map(m => ({
                memoryId: m.memoryId,
                title: m.title,
                memoryDate: m.memoryDate,
                latitude: parseFloat(m.latitude),
                longitude: parseFloat(m.longitude),
                locationName: m.locationName,
                visibility: m.visibility,
                category: m.categoryName || m.category || 'Travel',
                moodEmoji: m.moodEmoji || (m.mood ? m.mood.emoji : '📍'),
                thumbnailUrl: m.coverPhotoUrl || (m.photos && m.photos[0] ? m.photos[0].photoUrl : null)
            }));
        }

        // Nearby endpoint (Filtered with privacy rule)
        if (path === '/api/memories/nearby') {
            const list = getStoredMemories();
            const filtered = list.filter(m => {
                if (isAdmin) return true;
                if (currentUser) {
                    return m.userId === currentUser.userId || m.visibility === 1;
                }
                return m.visibility === 1;
            });
            return filtered.map(m => ({
                memoryId: m.memoryId,
                title: m.title,
                memoryDate: m.memoryDate,
                latitude: parseFloat(m.latitude),
                longitude: parseFloat(m.longitude),
                locationName: m.locationName,
                distanceKm: 2.5
            }));
        }

        // Memories List & Search:
        // - /api/memories (Personal Archive & Dashboard):
        //     Admin -> sees all
        //     User  -> sees ONLY their own memories (dashboard shows user's personal entries)
        //     Guest -> sees only public memories
        // - /api/memories/search (Global Search Registry):
        //     Admin -> searches all
        //     User  -> searches their own (private + public) AND other users' public entries
        //     Guest -> searches only public entries
        if (path === '/api/memories' || path === '/api/memories/search') {
            const list = getStoredMemories();
            if (method === 'GET') {
                let filtered = [];
                if (path === '/api/memories/search') {
                    filtered = list.filter(m => {
                        if (isAdmin) return true;
                        if (currentUser) {
                            return m.userId === currentUser.userId || m.visibility === 1;
                        }
                        return m.visibility === 1;
                    });
                } else {
                    // /api/memories: user dashboard / personal dossier stream
                    filtered = list.filter(m => {
                        if (isAdmin) return true;
                        if (currentUser) {
                            return m.userId === currentUser.userId;
                        }
                        return m.visibility === 1;
                    });
                }

                const search = params.get('search') || params.get('keyword');
                const loc = params.get('location');
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
                if (loc) {
                    const q = loc.toLowerCase();
                    filtered = filtered.filter(m => m.locationName && m.locationName.toLowerCase().includes(q));
                }
                if (catId) {
                    filtered = filtered.filter(m => m.categoryId == catId);
                }
                if (moodId) {
                    filtered = filtered.filter(m => m.moodId == moodId);
                }

                const page = parseInt(params.get('page') || '1');
                const pageSize = parseInt(params.get('pageSize') || '10');
                const startIndex = (page - 1) * pageSize;
                const paged = filtered.slice(startIndex, startIndex + pageSize);

                return {
                    items: paged.map(m => ({
                        ...m,
                        category: m.categoryName || m.category || 'Travel',
                        moodEmoji: m.moodEmoji || (m.mood ? m.mood.emoji : '📌'),
                        snippet: m.description ? (m.description.substring(0, 120) + (m.description.length > 120 ? '...' : '')) : ''
                    })),
                    totalItems: filtered.length,
                    page: page,
                    pageSize: pageSize,
                    totalPages: Math.max(1, Math.ceil(filtered.length / pageSize))
                };
            }

            if (method === 'POST') {
                let newMem = {};
                let visValue = 0;
                if (options.body instanceof FormData) {
                    newMem = {
                        title: options.body.get('title') || 'Untitled Memory',
                        description: options.body.get('description') || '',
                        memoryDate: options.body.get('memoryDate') || new Date().toISOString(),
                        latitude: parseFloat(options.body.get('latitude')) || 23.8103,
                        longitude: parseFloat(options.body.get('longitude')) || 90.4125,
                        locationName: options.body.get('locationName') || 'Pinned Location',
                        categoryId: parseInt(options.body.get('categoryId')) || 1,
                        moodId: parseInt(options.body.get('moodId')) || null
                    };
                    visValue = options.body.get('visibility') !== null ? parseInt(options.body.get('visibility')) : 0;
                } else {
                    try {
                        newMem = typeof options.body === 'string' ? JSON.parse(options.body) : (options.body || {});
                    } catch {}
                    visValue = newMem.visibility !== undefined && newMem.visibility !== null ? parseInt(newMem.visibility) : 0;
                }

                const author = Auth.getUser() || { userId: 1, fullName: 'Atlas Curator', email: 'demo@digitalmemory.io', role: 'User' };
                const cat = DEMO_CATEGORIES.find(c => c.categoryId === parseInt(newMem.categoryId)) || DEMO_CATEGORIES[0];
                const mood = DEMO_MOODS.find(m => m.moodId === parseInt(newMem.moodId)) || null;

                const created = {
                    memoryId: Date.now(),
                    userId: author.userId,
                    userName: author.fullName,
                    userEmail: author.email,
                    visibility: visValue, // 0 = Private Dossier, 1 = Public Registry
                    title: newMem.title || 'Untitled Memory',
                    description: newMem.description || '',
                    memoryDate: newMem.memoryDate || new Date().toISOString(),
                    latitude: parseFloat(newMem.latitude) || 23.8103,
                    longitude: parseFloat(newMem.longitude) || 90.4125,
                    locationName: newMem.locationName || 'Pinned Location',
                    categoryId: cat.categoryId,
                    categoryName: cat.name,
                    category: cat.name,
                    moodId: mood ? mood.moodId : null,
                    moodName: mood ? mood.name : '',
                    moodEmoji: mood ? mood.emoji : '📌',
                    mood: mood,
                    tags: Array.isArray(newMem.tags) ? newMem.tags : ['Archive'],
                    photos: [
                        { photoId: Date.now(), photoUrl: 'images/beach.jpg', isCover: true, caption: 'Archival Print' }
                    ],
                    photoCount: 1,
                    coverPhotoUrl: 'images/beach.jpg'
                };

                list.unshift(created);
                saveStoredMemories(list);
                return created;
            }
        }

        // Photo Upload endpoint
        const photoUploadMatch = path.match(/^\/api\/memories\/(\d+)\/photos/);
        if (photoUploadMatch) {
            return { message: 'Photo uploaded successfully' };
        }

        // Memory By ID (Protected with privacy access verification)
        const memDetailMatch = path.match(/^\/api\/memories\/(\d+)/);
        if (memDetailMatch) {
            const id = parseInt(memDetailMatch[1]);
            const list = getStoredMemories();
            const idx = list.findIndex(m => m.memoryId === id);

            if (method === 'GET') {
                if (idx === -1) throw new Error('Memory not found');
                const mem = list[idx];

                // If private memory (visibility === 0), verify authorization:
                if (mem.visibility === 0) {
                    if (!currentUser) {
                        throw new Error('This dossier is private. Please sign in to view.');
                    }
                    if (!isAdmin && mem.userId !== currentUser.userId) {
                        throw new Error('This dossier is confidential to its author.');
                    }
                }
                return mem;
            }

            if (method === 'DELETE') {
                if (idx === -1) throw new Error('Memory not found');
                const mem = list[idx];
                if (!isAdmin && (!currentUser || mem.userId !== currentUser.userId)) {
                    throw new Error('Unauthorized: You can only remove your own memories.');
                }
                list.splice(idx, 1);
                saveStoredMemories(list);
                return { success: true };
            }

            if (method === 'PUT') {
                if (idx === -1) throw new Error('Memory not found');
                const mem = list[idx];
                if (!isAdmin && (!currentUser || mem.userId !== currentUser.userId)) {
                    throw new Error('Unauthorized: You can only update your own memories.');
                }
                let updateData = {};
                try { updateData = JSON.parse(options.body); } catch {}
                if (updateData.visibility !== undefined) {
                    updateData.visibility = parseInt(updateData.visibility);
                }
                list[idx] = { ...list[idx], ...updateData };
                saveStoredMemories(list);
                return list[idx];
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
            const users = getStoredUsers();
            return {
                totalUsers: users.length,
                activeUsers: users.length,
                newUsersThisWeek: 2,
                totalMemories: list.length,
                totalPhotos: list.reduce((sum, m) => sum + (m.photoCount || (m.photos ? m.photos.length : 1)), 0),
                categoryStats: DEMO_CATEGORIES.map(c => ({
                    categoryName: c.name,
                    memoryCount: list.filter(m => m.categoryId === c.categoryId).length
                }))
            };
        }
        if (path === '/api/admin/users') {
            const users = getStoredUsers();
            return users.map(u => ({
                userId: u.userId,
                fullName: u.fullName,
                email: u.email,
                role: u.role,
                isActive: true,
                createdAt: "2026-01-01T00:00:00Z"
            }));
        }
        if (path === '/api/admin/memories') {
            const list = getStoredMemories();
            const keyword = (params.get('keyword') || '').toLowerCase();
            let filtered = [...list];
            if (keyword) {
                filtered = filtered.filter(m => 
                    (m.title && m.title.toLowerCase().includes(keyword)) ||
                    (m.locationName && m.locationName.toLowerCase().includes(keyword)) ||
                    ((m.userName || '').toLowerCase().includes(keyword)) ||
                    ((m.userEmail || '').toLowerCase().includes(keyword))
                );
            }
            const page = parseInt(params.get('page') || '1');
            const pageSize = parseInt(params.get('pageSize') || '15');
            const startIndex = (page - 1) * pageSize;
            const paged = filtered.slice(startIndex, startIndex + pageSize);

            return {
                items: paged.map(m => ({
                    ...m,
                    userName: m.userName || 'Archivist Member',
                    userEmail: m.userEmail || 'user@digitalmemory.io',
                    category: m.categoryName || m.category || 'Travel',
                    moodEmoji: m.moodEmoji || (m.mood ? m.mood.emoji : '📌'),
                    snippet: m.description ? (m.description.substring(0, 100) + (m.description.length > 100 ? '...' : '')) : (m.title || '')
                })),
                totalItems: filtered.length,
                page: page,
                pageSize: pageSize,
                totalPages: Math.max(1, Math.ceil(filtered.length / pageSize))
            };
        }

        const adminDeleteMatch = path.match(/^\/api\/admin\/memories\/(\d+)/);
        if (adminDeleteMatch) {
            const id = parseInt(adminDeleteMatch[1]);
            const list = getStoredMemories();
            const idx = list.findIndex(m => m.memoryId === id);
            if (idx !== -1) {
                list.splice(idx, 1);
                saveStoredMemories(list);
            }
            return { message: "Memory removed by admin." };
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
