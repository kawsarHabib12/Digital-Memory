/* ==========================================================================
   Digital Memory Map - Unified API Client & Toast Notification System
   ========================================================================== */

const API = {
    async request(url, options = {}) {
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
            credentials: 'same-origin' // Ensures HttpOnly cookie dmm_token is sent
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
                    window.location.href = '/login.html?expired=1';
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
