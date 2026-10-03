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

    requireAuth() {
        if (!this.isLoggedIn()) {
            const currentPath = window.location.pathname;
            window.location.href = `/login.html?returnUrl=${encodeURIComponent(currentPath)}`;
            return false;
        }
        return true;
    },

    requireAdmin() {
        if (!this.requireAuth()) return false;
        if (!this.isAdmin()) {
            window.location.href = '/dashboard.html';
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
            window.location.href = '/login.html';
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
            <a href="${loggedIn ? '/dashboard.html' : '/index.html'}" class="nav-brand">
                <span class="brand-icon">DM</span>
                <div>
                    <div>DIGITAL MEMORY MAP</div>
                    <span class="brand-sub">ARCHIVAL EDITION</span>
                </div>
            </a>
            <ul class="nav-links">`;

        if (loggedIn) {
            navHtml += `
                <li><a href="/dashboard.html" class="nav-link ${activePage === 'dashboard' ? 'active' : ''}">DASHBOARD</a></li>
                <li><a href="/map.html" class="nav-link ${activePage === 'map' ? 'active' : ''}">MAP</a></li>
                <li><a href="/timeline.html" class="nav-link ${activePage === 'timeline' ? 'active' : ''}">TIMELINE</a></li>
                <li><a href="/search.html" class="nav-link ${activePage === 'search' ? 'active' : ''}">SEARCH</a></li>
                <li><a href="/memory-form.html" class="nav-link ${activePage === 'add-memory' ? 'active' : ''}">+ ADD MEMORY</a></li>
                ${isAdmin ? `<li><a href="/admin/dashboard.html" class="nav-link ${activePage.startsWith('admin') ? 'active' : ''}"><span class="admin-tag">ADMIN</span></a></li>` : ''}
            </ul>
            <div class="nav-user">
                <a href="/profile.html" class="user-badge" title="View Profile">
                    <span>PROFILE: ${escapeHtml(user.fullName || 'MY ACCOUNT')}</span>
                </a>
                <button onclick="Auth.logout()" class="btn btn-secondary btn-sm" title="Log out">LOGOUT</button>
            </div>`;
        } else {
            navHtml += `
                <li><a href="/index.html" class="nav-link ${activePage === 'home' ? 'active' : ''}">HOME</a></li>
                <li><a href="/map.html" class="nav-link ${activePage === 'map' ? 'active' : ''}">MAP</a></li>
                <li><a href="/login.html" class="nav-link ${activePage === 'login' ? 'active' : ''}">LOGIN</a></li>
            </ul>
            <div class="nav-user">
                <a href="/register.html" class="btn btn-primary btn-sm">REGISTER</a>
            </div>`;
        }

        navHtml += `</nav>`;
        navContainer.innerHTML = navHtml;
    }
};

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
