/* ==========================================================================
   Digital Memory Map - Auth Management & Dynamic Navigation Header
   ========================================================================== */

const Auth = {
    getUser() {
        const stored = localStorage.getItem('dmm_user');
        if (!stored) return null;
        try {
            return JSON.parse(stored);
        } catch {
            return null;
        }
    },

    setUser(user) {
        localStorage.setItem('dmm_user', JSON.stringify(user));
    },

    clearUser() {
        localStorage.removeItem('dmm_user');
    },

    isLoggedIn() {
        return !!this.getUser();
    },

    isAdmin() {
        const user = this.getUser();
        return user && user.role === 'Admin';
    },

    getBasePath() {
        const pathname = window.location.pathname;
        if (window.location.hostname.endsWith('github.io')) {
            const repoName = pathname.split('/')[1];
            return repoName ? `/${repoName}/` : '/';
        }
        return '/';
    },

    url(path) {
        if (!path) return this.getBasePath();
        if (path.startsWith('http://') || path.startsWith('https://')) return path;
        const clean = path.startsWith('/') ? path.substring(1) : path;
        return this.getBasePath() + clean;
    },

    requireAuth() {
        if (!this.isLoggedIn()) {
            const currentPath = window.location.pathname;
            window.location.href = this.url(`login.html?returnUrl=${encodeURIComponent(currentPath)}`);
            return false;
        }
        return true;
    },

    requireAdmin() {
        if (!this.requireAuth()) return false;
        if (!this.isAdmin()) {
            window.location.href = this.url('dashboard.html');
            return false;
        }
        return true;
    },

    async logout() {
        try {
            await API.post('/api/auth/logout');
        } catch (e) {
            console.error('Logout error:', e);
        } finally {
            this.clearUser();
            window.location.href = this.url('login.html');
        }
    },

    renderNavbar(activePage = '') {
        const navContainer = document.getElementById('navbar-container');
        if (!navContainer) return;

        const user = this.getUser();
        const loggedIn = !!user;
        const isAdmin = user && user.role === 'Admin';

        let navHtml = `
        <div class="archive-ticker-bar">
            <span>ARCHIVE REFERENCE: ED. 2026 // PERSONAL GEOGRAPHY &amp; RETROSPECTIVE JOURNAL</span>
            <span>SYSTEM STATUS: ARCHIVE ACTIVE</span>
        </div>
        <nav class="navbar">
            <a href="${loggedIn ? this.url('dashboard.html') : this.url('index.html')}" class="nav-brand">
                <span class="brand-icon">DM</span>
                <div>
                    <div>DIGITAL MEMORY MAP</div>
                    <span class="brand-sub">ARCHIVAL EDITION</span>
                </div>
            </a>
            <ul class="nav-links">`;

        if (loggedIn) {
            navHtml += `
                <li><a href="${this.url('dashboard.html')}" class="nav-link ${activePage === 'dashboard' ? 'active' : ''}">DASHBOARD</a></li>
                <li><a href="${this.url('map.html')}" class="nav-link ${activePage === 'map' ? 'active' : ''}">MAP</a></li>
                <li><a href="${this.url('journey.html')}" class="nav-link ${activePage === 'journey' ? 'active' : ''}">MY JOURNEY</a></li>
                <li><a href="${this.url('timeline.html')}" class="nav-link ${activePage === 'timeline' ? 'active' : ''}">TIMELINE</a></li>
                <li><a href="${this.url('search.html')}" class="nav-link ${activePage === 'search' ? 'active' : ''}">SEARCH</a></li>
                <li><a href="${this.url('memory-form.html')}" class="nav-link ${activePage === 'add-memory' ? 'active' : ''}">+ ADD MEMORY</a></li>
                ${isAdmin ? `<li><a href="${this.url('admin/dashboard.html')}" class="nav-link ${activePage.startsWith('admin') ? 'active' : ''}"><span class="admin-tag">ADMIN</span></a></li>` : ''}
            </ul>
            <div class="nav-user">
                <a href="${this.url('profile.html')}" class="user-badge" title="View Profile">
                    <span>PROFILE: ${escapeHtml(user.fullName || 'MY ACCOUNT')}</span>
                </a>
                <button onclick="Auth.logout()" class="btn btn-secondary btn-sm" title="Log out">LOGOUT</button>
            </div>`;
        } else {
            navHtml += `
                <li><a href="${this.url('index.html')}" class="nav-link ${activePage === 'home' ? 'active' : ''}">HOME</a></li>
                <li><a href="${this.url('map.html')}" class="nav-link ${activePage === 'map' ? 'active' : ''}">MAP</a></li>
                <li><a href="${this.url('login.html')}" class="nav-link ${activePage === 'login' ? 'active' : ''}">LOGIN</a></li>
            </ul>
            <div class="nav-user">
                <a href="${this.url('register.html')}" class="btn btn-primary btn-sm">REGISTER</a>
            </div>`;
        }

        navHtml += `</nav>`;
        navContainer.innerHTML = navHtml;
    },

    initPasswordToggles() {
        // Auto-wrap any unwrapped password inputs
        document.querySelectorAll('input[type="password"]').forEach(input => {
            if (input.closest('.password-toggle-wrapper')) return;
            const wrapper = document.createElement('div');
            wrapper.className = 'password-toggle-wrapper';
            input.parentNode.insertBefore(wrapper, input);
            wrapper.appendChild(input);

            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'password-toggle-btn';
            btn.setAttribute('aria-label', 'Show password');
            btn.setAttribute('title', 'Show password');
            btn.innerHTML = `
                <svg class="eye-icon" xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                    <path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z"></path>
                    <circle cx="12" cy="12" r="3"></circle>
                </svg>
                <svg class="eye-off-icon" xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" style="display: none;">
                    <path d="M9.88 9.88a3 3 0 1 0 4.24 4.24"></path>
                    <path d="M10.73 5.08A10.43 10.43 0 0 1 12 5c7 0 10 7 10 7a13.16 13.16 0 0 1-1.67 2.68"></path>
                    <path d="M6.61 6.61A13.526 13.526 0 0 0 2 12s3 7 10 7a9.74 9.74 0 0 0 5.39-1.61"></path>
                    <line x1="2" y1="2" x2="22" y2="22"></line>
                </svg>`;
            wrapper.appendChild(btn);
        });

        // Attach click handlers to all toggle buttons
        document.querySelectorAll('.password-toggle-btn').forEach(btn => {
            if (btn.dataset.initialized === 'true') return;
            btn.dataset.initialized = 'true';

            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const wrapper = btn.closest('.password-toggle-wrapper');
                if (!wrapper) return;
                const input = wrapper.querySelector('input');
                if (!input) return;

                const isCurrentlyPassword = input.type === 'password';
                input.type = isCurrentlyPassword ? 'text' : 'password';

                const eyeIcon = btn.querySelector('.eye-icon');
                const eyeOffIcon = btn.querySelector('.eye-off-icon');

                if (eyeIcon) eyeIcon.style.display = isCurrentlyPassword ? 'none' : 'block';
                if (eyeOffIcon) eyeOffIcon.style.display = isCurrentlyPassword ? 'block' : 'none';

                const newLabel = isCurrentlyPassword ? 'Hide password' : 'Show password';
                btn.setAttribute('aria-label', newLabel);
                btn.setAttribute('title', newLabel);

                input.focus();
                try {
                    const len = input.value.length;
                    input.setSelectionRange(len, len);
                } catch {
                    // ignore if not supported
                }
            });
        });
    }
};

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => Auth.initPasswordToggles());
} else {
    Auth.initPasswordToggles();
}

function escapeHtml(text) {
    if (!text) return '';
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.toString().replace(/[&<>"']/g, m => map[m]);
}

