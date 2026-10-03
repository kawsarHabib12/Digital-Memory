# Digital Memory Map — Product Requirements Document (PRD)

| | |
|---|---|
| **Version** | 1.0 |
| **Project type** | University CSE software project |
| **Stack** | ASP.NET Core Web API (C#), MySQL, EF Core, HTML/CSS/JS, Leaflet.js + OpenStreetMap |
| **Architecture** | 3-Tier (Presentation / Business Logic / Data Access) |

---

## 1. Project Overview

**Digital Memory Map** is a web application where users save personal memories (photos, notes, mood, tags) and pin them to a real location on an interactive map. Users can later explore the map, browse a timeline, search/filter memories, and rediscover old memories when they revisit a place.

**Example:** A user visits Cox's Bazar, saves 4 photos and a note. The memory is pinned on the map. A year later, the user opens the map near Cox's Bazar and sees the old memory.

### 1.1 Problem Statement
Personal memories are scattered across phone galleries, chat apps, and notes. Galleries sort by date only, so place-based recall ("what did I do in Sylhet?") is hard. There is no simple personal tool that connects **where + when + how it felt** in one view.

### 1.2 Objectives
1. Let users create memories tied to a map location, date, mood, category, tags, and photos.
2. Provide a map view, timeline view, and search/filter to rediscover memories.
3. Keep data private by default with strict ownership checks.
4. Demonstrate ASP.NET Core, C#, MySQL, EF Core, REST, 3-tier, CRUD, authentication, LINQ, and map integration.

### 1.3 Target Users
| Group | Need |
|---|---|
| Students / young adults | Save trips, university moments, friend hangouts |
| Travelers | Keep a personal travel log on a map |
| Families | Store childhood and family moments by place |

### 1.4 Assumptions
- Users have a modern browser and internet access (map tiles need internet).
- Single web server and single MySQL instance (no clustering).
- Photos are stored on the server file system; only the path is stored in MySQL.
- The map uses free OpenStreetMap tiles; place search uses Nominatim (free, rate-limited).
- Emotion is manually selected. There is no AI analysis.

### 1.5 Constraints
- Student team/solo developer, limited time → strict MVP.
- Free or low-cost services only.
- Must follow 3-tier architecture and use EF Core with MySQL.
- Nominatim usage policy: max ~1 request/second, no autocomplete-on-every-keystroke, show OSM attribution.

> **SMS note:** SMS is not part of this scope. If your course still requires an SMS API, the simplest fit is an optional "On This Day" reminder (see Future Scope).

---

## 2. User Roles

| Role | Capabilities |
|---|---|
| **User** | Register, login/logout, manage profile, full CRUD on **own** memories, upload/delete photos, view map, timeline, search, filter |
| **Admin** | Login, view/manage users (activate/deactivate), remove inappropriate memories, manage categories, view statistics and basic activity |

Admin accounts are created by database seeding (not by public registration).

---

## 3. Functional Requirements

Priority: **M** = Must (MVP), **S** = Should, **F** = Future.

### 3.1 Authentication Module
| ID | Requirement | Priority |
|---|---|---|
| AUTH-1 | Register with name, email, password | M |
| AUTH-2 | Login with email + password; receive JWT | M |
| AUTH-3 | Logout | M |
| AUTH-4 | Change password (requires current password) | M |
| AUTH-5 | Role-based access (User / Admin) | M |
| AUTH-6 | Forgot/reset password by email | F |

### 3.2 Memory Management Module
| ID | Requirement | Priority |
|---|---|---|
| MEM-1 | Create memory (title, description, date, lat/lng, location name, category, mood, tags, visibility) | M |
| MEM-2 | View own memory details | M |
| MEM-3 | Edit own memory | M |
| MEM-4 | Delete own memory (also deletes its photos/tag links) | M |
| MEM-5 | Memory has Created/Updated timestamps | M |
| MEM-6 | Visibility field (Private default, Public stored but public viewing is Future) | S |

### 3.3 Interactive Map Module
| ID | Requirement | Priority |
|---|---|---|
| MAP-1 | Show all of the user's memories as pins | M |
| MAP-2 | Click a pin → popup preview (thumbnail, title, date, mood) | M |
| MAP-3 | "View details" link from popup | M |
| MAP-4 | Click map (or search a place) to pick a location for a new memory | M |
| MAP-5 | Place search (Nominatim) and zoom/pan | M |
| MAP-6 | Filter pins by category, mood, date range | M |
| MAP-7 | "Nearby memories" (within X km of a point or user's current location) | S |
| MAP-8 | Pin clustering (Leaflet.markercluster) | S |

### 3.4 Timeline
| ID | Requirement | Priority |
|---|---|---|
| TL-1 | Chronological list (newest first, toggle oldest first) showing date, location, title, photo, short description | M |
| TL-2 | Paged loading (10 per page / "load more") | M |
| TL-3 | Group by year/month headers | S |

### 3.5 Search and Filter
Search by keyword (title + description), location name, date, date range, category, mood, tag. Filters combine with AND. (M)

### 3.6 Photo Management
Upload multiple photos per memory, view, delete individual photo, set first photo as cover. (M)

### 3.7 Categories (Admin-managed)
Seeded: Travel, University, Family, Friends, Food, Events, Childhood, Nature, Other. Admin can add, rename, deactivate. A category in use cannot be hard-deleted (deactivate instead). (M)

### 3.8 Mood / Emotion (predefined, manual)
😊 Happy · ❤️ Loved · 😂 Funny · 😢 Sad · 😍 Excited · 😌 Peaceful · 😐 Normal. Stored in a `Moods` table (seeded, not editable in MVP). (M)

### 3.9 Admin Module
| ID | Requirement | Priority |
|---|---|---|
| ADM-1 | Dashboard: total users, memories, photos, memories per category, new users this week | M |
| ADM-2 | List/search users; activate/deactivate | M |
| ADM-3 | List all memories; remove (soft remove) inappropriate ones | M |
| ADM-4 | Manage categories | M |
| ADM-5 | Flag/report memories by users | F |

### 3.10 User Profile
View/edit name and bio; change password; optional profile photo (S).

---

## 4. Acceptance Criteria (Major Features)

| Feature | Acceptance criteria |
|---|---|
| **Register** | Valid input creates a User-role account and returns 201. Duplicate email returns 409. Password is stored hashed. |
| **Login** | Correct credentials return a JWT and set it in an HttpOnly cookie. Wrong credentials return 401 with a generic message. Deactivated users cannot log in (403). |
| **Create memory** | With valid data, memory is saved, pin appears on the map without page reload. Invalid coordinates return 400. Missing title returns 400 with field errors. |
| **Ownership** | User A requesting/editing/deleting User B's memory gets 404 (or 403). Data is never returned. |
| **Photo upload** | JPG/PNG/WEBP up to 5 MB, max 5 per memory accepted. Other types/sizes rejected (400/413). Files are renamed to GUIDs. |
| **Map** | Map loads only the logged-in user's pins. Clicking a pin shows preview. Filters update the pins within 1 second for up to 500 memories. |
| **Timeline** | Memories appear sorted by date; each item shows date, location, title, cover photo, snippet. Paging works. |
| **Search/filter** | Each filter works alone and in combination; empty result shows a friendly "no memories found" message. |
| **Delete memory** | Memory, its photo records, photo files and tag links are removed; pin disappears. |
| **Admin** | Non-admin calling admin endpoints gets 403. Admin can deactivate a user and remove a memory. Statistics match database counts. |

---

## 5. MVP Definition

### Must Have
Register/login/logout/change password · Role-based access (User/Admin) · Memory CRUD with location, date, category, mood, tags · Photo upload (multiple) · Interactive Leaflet map with pins, popups, place search, location picking · Map filters (category, mood, date range) · Timeline with paging · Search by keyword/location/date/range/category/mood/tag · Admin: dashboard stats, user activate/deactivate, remove memories, manage categories · Validation, ownership checks, secure error handling.

### Should Have
Nearby memories · Marker clustering · Timeline grouping by year/month · Profile photo · Visibility field · Image thumbnails · Dark mode.

### Future (not required)
See Section 19.

---

## 6. System Architecture

### 6.1 Overview
```
Browser (HTML/CSS/JS + Leaflet)
        │  REST (JSON / multipart), HttpOnly JWT cookie
        ▼
┌───────────────────────────────────────────┐
│ PRESENTATION  – API Controllers, wwwroot   │  HTTP, auth, DTO binding, status codes
├───────────────────────────────────────────┤
│ BUSINESS LOGIC – Services + DTOs + rules   │  validation, ownership, mapping, file rules
├───────────────────────────────────────────┤
│ DATA ACCESS – Repositories + EF Core       │  LINQ queries, transactions, migrations
└───────────────────────────────────────────┘
        ▼
     MySQL            File system (/uploads)
```

### 6.2 Key technical decisions
| Decision | Choice | Reason |
|---|---|---|
| App style | ASP.NET Core Web API + static frontend in `wwwroot` | Clean REST demo; same origin avoids CORS |
| Authentication | JWT stored in **HttpOnly, SameSite=Strict cookie** | `<img>` tags work with cookies; token not readable by JS (XSS-safer) |
| MySQL provider | Pomelo.EntityFrameworkCore.MySql | Most used EF Core provider for MySQL |
| Password hashing | `PasswordHasher<T>` (PBKDF2) or BCrypt.Net | Never store plain text |
| Map | Leaflet.js + OSM tiles; Nominatim for place search | Free, simple |
| Photos | Files on disk, path in DB; served through an authenticated endpoint | Private photos stay private |

### 6.3 Layer Responsibilities
| Layer | Contains | Responsibility | Must NOT |
|---|---|---|---|
| **Presentation** | `AuthController`, `MemoriesController`, `PhotosController`, `CategoriesController`, `TagsController`, `ProfileController`, `AdminController`, `wwwroot` pages | Receive requests, read current user id from claims, call services, return HTTP codes | Contain business rules or EF queries |
| **Business Logic** | `UserService`, `MemoryService`, `CategoryService`, `TagService`, `PhotoService`, `AdminService`, DTOs, validators | Validation, ownership checks, tag handling, file validation, mapping entity ↔ DTO | Know about HTTP or SQL |
| **Data Access** | `AppDbContext`, entities, `UserRepository`, `MemoryRepository`, `CategoryRepository`, `TagRepository`, `PhotoRepository` | LINQ queries, CRUD, includes, paging, migrations | Contain business decisions |

Use **dependency injection** for every service and repository (interfaces: `IMemoryService`, `IMemoryRepository`, ...).

---

## 7. Project Folder Structure

```
DigitalMemoryMap/                      (solution)
├── DigitalMemoryMap.Web/              ← Presentation
│   ├── Controllers/
│   ├── Middleware/                    (global exception handler)
│   ├── wwwroot/
│   │   ├── index.html, login.html, register.html
│   │   ├── dashboard.html, map.html, memory-form.html
│   │   ├── memory-details.html, timeline.html, search.html, profile.html
│   │   ├── admin/ (dashboard, users, memories, categories)
│   │   ├── css/   js/ (api.js, map.js, auth.js, ...)   lib/
│   ├── Program.cs, appsettings.json
├── DigitalMemoryMap.BLL/              ← Business Logic
│   ├── Interfaces/   (IMemoryService, ...)
│   ├── Services/     (MemoryService, ...)
│   ├── DTOs/         (Auth, Memory, Photo, Category, Admin ...)
│   ├── Validators/   Mappings/   Exceptions/   (NotFoundException, ...)
├── DigitalMemoryMap.DAL/              ← Data Access
│   ├── Entities/     (User, Role, Memory, MemoryPhoto, Category, Mood, Tag, MemoryTag)
│   ├── Data/         (AppDbContext, SeedData)
│   ├── Configurations/ (Fluent API per entity)
│   ├── Interfaces/   Repositories/   Migrations/
├── DigitalMemoryMap.Tests/            ← xUnit tests
├── uploads/                           (outside wwwroot; git-ignored)
└── README.md
```
Dependency direction: `Web → BLL → DAL`. DAL never references BLL or Web.

---

## 8. Database Design (MySQL)

### 8.1 Relationship Summary (ERD-style)
```
Roles (1) ────< (N) Users (1) ────< (N) Memories (1) ────< (N) MemoryPhotos
                       │                   │  │
                       │                   │  └── (N) ──> (1) Moods
                       │                   └───── (N) ──> (1) Categories
                       └────< (N) Tags (N) >────< MemoryTags >────< (N) Memories
```
- One Role → many Users. One User → many Memories, many Tags.
- One Memory → many Photos. Memory ↔ Tag is **many-to-many** via `MemoryTags`.
- Each Memory has one Category (required) and one Mood (optional).
- Tags are **per user**, so one user's tags never leak to another.

### 8.2 Tables

**Roles**
| Column | Type | Key | Null | Notes |
|---|---|---|---|---|
| RoleId | TINYINT | PK, AI | No | |
| RoleName | VARCHAR(20) | Unique | No | `User`, `Admin` |

**Users**
| Column | Type | Key | Null | Notes |
|---|---|---|---|---|
| UserId | INT | PK, AI | No | |
| FullName | VARCHAR(100) | | No | |
| Email | VARCHAR(150) | Unique | No | stored lowercase |
| PasswordHash | VARCHAR(255) | | No | |
| RoleId | TINYINT | FK→Roles | No | default User |
| Bio | VARCHAR(300) | | Yes | |
| ProfilePhotoPath | VARCHAR(255) | | Yes | |
| IsActive | BOOLEAN | | No | default 1 |
| CreatedAt | DATETIME | | No | UTC |
| UpdatedAt | DATETIME | | Yes | |

**Categories**
| Column | Type | Key | Null | Notes |
|---|---|---|---|---|
| CategoryId | INT | PK, AI | No | |
| Name | VARCHAR(50) | Unique | No | |
| IsActive | BOOLEAN | | No | default 1 |

**Moods**
| Column | Type | Key | Null | Notes |
|---|---|---|---|---|
| MoodId | TINYINT | PK, AI | No | |
| Name | VARCHAR(30) | Unique | No | Happy, Loved, ... |
| Emoji | VARCHAR(10) | | No | |

**Memories**
| Column | Type | Key | Null | Notes |
|---|---|---|---|---|
| MemoryId | INT | PK, AI | No | |
| UserId | INT | FK→Users (CASCADE) | No | owner |
| Title | VARCHAR(100) | | No | |
| Description | TEXT | | Yes | max 2000 chars (enforced in BLL) |
| MemoryDate | DATE | | No | not in future |
| Latitude | DECIMAL(9,6) | | No | -90..90 |
| Longitude | DECIMAL(9,6) | | No | -180..180 |
| LocationName | VARCHAR(200) | | Yes | from Nominatim or typed |
| CategoryId | INT | FK→Categories | No | |
| MoodId | TINYINT | FK→Moods | Yes | |
| Visibility | TINYINT | | No | 0 Private (default), 1 Public |
| Status | TINYINT | | No | 1 Active, 2 RemovedByAdmin |
| CreatedAt | DATETIME | | No | |
| UpdatedAt | DATETIME | | Yes | |

Indexes: `(UserId, MemoryDate)`, `(UserId, CategoryId)`, `(UserId, Latitude, Longitude)`; optional FULLTEXT on `Title, Description`.

**MemoryPhotos**
| Column | Type | Key | Null | Notes |
|---|---|---|---|---|
| PhotoId | INT | PK, AI | No | |
| MemoryId | INT | FK→Memories (CASCADE) | No | |
| FilePath | VARCHAR(255) | | No | GUID-based name |
| OriginalFileName | VARCHAR(200) | | No | display only |
| FileSizeKb | INT | | No | |
| IsCover | BOOLEAN | | No | default 0 |
| UploadedAt | DATETIME | | No | |

**Tags**
| Column | Type | Key | Null | Notes |
|---|---|---|---|---|
| TagId | INT | PK, AI | No | |
| UserId | INT | FK→Users (CASCADE) | No | |
| Name | VARCHAR(30) | Unique with UserId | No | lowercase |

**MemoryTags**
| Column | Type | Key | Null | Notes |
|---|---|---|---|---|
| MemoryId | INT | PK part, FK→Memories (CASCADE) | No | |
| TagId | INT | PK part, FK→Tags (CASCADE) | No | |

**Normalization:** All tables are in 3NF. Mood, category, and role names live in lookup tables. Tags are not repeated per memory. Deleting a user cascades to memories, tags, photos.

**Seed data:** Roles, Moods, 9 default Categories, one Admin user.

### 8.3 Sample LINQ (for the DAL)
```csharp
var query = _db.Memories
    .Where(m => m.UserId == userId && m.Status == 1)
    .Where(m => categoryId == null || m.CategoryId == categoryId)
    .Where(m => from == null || m.MemoryDate >= from)
    .Where(m => to == null || m.MemoryDate <= to)
    .Where(m => tag == null || m.MemoryTags.Any(t => t.Tag.Name == tag))
    .Where(m => keyword == null || m.Title.Contains(keyword) || m.Description.Contains(keyword));
var page = await query.OrderByDescending(m => m.MemoryDate)
    .Skip((pageNo - 1) * size).Take(size).ToListAsync();
```
Nearby search: compute a bounding box (lat ± r/111, lng ± r/(111·cos lat)) in SQL, then refine with the Haversine formula in C#.

---

## 9. API Specification

Base URL: `/api`. All responses are JSON. 🔒 = requires login, 👑 = Admin only.

### 9.1 Endpoint Summary
| Area | Method | URL | Purpose | Auth |
|---|---|---|---|---|
| Auth | POST | `/auth/register` | Create account | Public |
| | POST | `/auth/login` | Login, set cookie | Public |
| | POST | `/auth/logout` | Clear cookie | 🔒 |
| | POST | `/auth/change-password` | Change password | 🔒 |
| Profile | GET | `/profile` | Get own profile | 🔒 |
| | PUT | `/profile` | Update name/bio | 🔒 |
| Memories | GET | `/memories` | List own memories (paged, filtered; used by timeline & search) | 🔒 |
| | GET | `/memories/{id}` | Memory details with photos and tags | 🔒 |
| | POST | `/memories` | Create | 🔒 |
| | PUT | `/memories/{id}` | Update | 🔒 |
| | DELETE | `/memories/{id}` | Delete | 🔒 |
| Map | GET | `/memories/map` | Lightweight pin data with filters | 🔒 |
| | GET | `/memories/nearby?lat=&lng=&radiusKm=` | Memories near a point | 🔒 |
| Search | GET | `/memories/search` | Same filters as list (alias) | 🔒 |
| Photos | POST | `/memories/{id}/photos` | Upload 1–5 photos (multipart) | 🔒 |
| | DELETE | `/photos/{photoId}` | Delete photo | 🔒 |
| | PUT | `/photos/{photoId}/cover` | Set cover | 🔒 |
| | GET | `/photos/{photoId}/file` | Stream image (owner/admin only) | 🔒 |
| Categories | GET | `/categories` | Active categories | 🔒 |
| | POST / PUT / DELETE | `/admin/categories[/{id}]` | Manage categories | 👑 |
| Moods | GET | `/moods` | Mood list | 🔒 |
| Tags | GET | `/tags` | Own tags (for suggestions) | 🔒 |
| Admin | GET | `/admin/stats` | Dashboard numbers | 👑 |
| | GET | `/admin/users` | List/search users | 👑 |
| | PUT | `/admin/users/{id}/status` | Activate/deactivate | 👑 |
| | GET | `/admin/memories` | List all memories | 👑 |
| | DELETE | `/admin/memories/{id}` | Remove (sets Status=2) | 👑 |

**Query parameters for `GET /memories`:** `keyword, location, date, from, to, categoryId, moodId, tag, sort (date_desc|date_asc), page, pageSize (max 50)`.

### 9.2 Request/Response Examples

**Register** — `POST /api/auth/register`
```json
// Request
{ "fullName": "Koushick", "email": "k@example.com", "password": "Strong@123", "confirmPassword": "Strong@123" }
// 201 Created
{ "userId": 12, "fullName": "Koushick", "email": "k@example.com", "role": "User" }
// 409 Conflict
{ "status": 409, "message": "Email is already registered." }
```

**Login** — `POST /api/auth/login`
```json
// Request
{ "email": "k@example.com", "password": "Strong@123" }
// 200 OK (+ Set-Cookie: dmm_token=...; HttpOnly; SameSite=Strict)
{ "userId": 12, "fullName": "Koushick", "role": "User", "expiresAt": "2026-10-03T10:00:00Z" }
// 401 Unauthorized
{ "status": 401, "message": "Invalid email or password." }
```

**Create memory** — `POST /api/memories`
```json
// Request
{
  "title": "Sunset at Laboni Beach",
  "description": "Walked along the shore with friends.",
  "memoryDate": "2026-03-14",
  "latitude": 21.4272, "longitude": 91.9788,
  "locationName": "Cox's Bazar",
  "categoryId": 1, "moodId": 1,
  "tags": ["beach", "friends"],
  "visibility": 0
}
// 201 Created  (Location: /api/memories/45)
{ "memoryId": 45, "title": "Sunset at Laboni Beach", "memoryDate": "2026-03-14",
  "latitude": 21.4272, "longitude": 91.9788, "locationName": "Cox's Bazar",
  "category": "Travel", "mood": { "name": "Happy", "emoji": "😊" },
  "tags": ["beach", "friends"], "photos": [], "createdAt": "2026-10-02T09:30:00Z" }
// 400 Bad Request
{ "status": 400, "message": "Validation failed.",
  "errors": { "title": ["Title is required."], "latitude": ["Latitude must be between -90 and 90."] } }
```

**Upload photos** — `POST /api/memories/45/photos` (`multipart/form-data`, field `files`)
```json
// 201 Created
{ "photos": [ { "photoId": 101, "url": "/api/photos/101/file", "isCover": true } ] }
// 400 → "Only JPG, PNG, WEBP images are allowed."   413 → "Each file must be 5 MB or less."
```

**Map pins** — `GET /api/memories/map?categoryId=1&from=2026-01-01&to=2026-12-31`
```json
// 200 OK
[ { "memoryId": 45, "title": "Sunset at Laboni Beach", "memoryDate": "2026-03-14",
    "latitude": 21.4272, "longitude": 91.9788, "category": "Travel",
    "moodEmoji": "😊", "thumbnailUrl": "/api/photos/101/file" } ]
```

**List / timeline / search** — `GET /api/memories?keyword=beach&page=1&pageSize=10`
```json
// 200 OK
{ "page": 1, "pageSize": 10, "totalItems": 3, "totalPages": 1,
  "items": [ { "memoryId": 45, "title": "Sunset at Laboni Beach", "memoryDate": "2026-03-14",
               "locationName": "Cox's Bazar", "snippet": "Walked along the shore...",
               "coverPhotoUrl": "/api/photos/101/file", "category": "Travel", "moodEmoji": "😊" } ] }
```

### 9.3 How the Map Talks to the Backend
1. `map.js` initializes Leaflet with OSM tiles.
2. On load it calls `GET /api/memories/map` and adds one marker per item.
3. Marker click → popup built from the pin data (no extra call). "View details" → `memory-details.html?id=45`, which calls `GET /api/memories/45`.
4. Filter controls re-call `/memories/map` with query params and redraw markers.
5. "Add Memory": the user clicks the map → latitude/longitude are copied into hidden form fields; Nominatim (called from the browser) fills `locationName` and supports place search.
6. On submit the form calls `POST /api/memories`, then `POST /api/memories/{id}/photos`, then adds the new marker immediately.

---

## 10. UI/UX Requirements

Design: clean, modern, responsive (mobile-friendly), map-first. Use plain CSS or Bootstrap. Navbar: Dashboard · Map · Timeline · Search · Add Memory · Profile · Logout.

### Public pages
| Page | Content |
|---|---|
| Home | Hero text, short feature highlights, screenshot of the map, Login/Register buttons |
| Login | Email, password, error message, link to Register |
| Register | Name, email, password, confirm password, inline validation messages |

### User pages
| Page | Content |
|---|---|
| **Dashboard** | Large map (≈70% of the screen) with pins, quick stats (total memories, places, this year), "Add Memory" button, 5 recent memories |
| **Memory Map** | Full-screen map, place search box, filter bar (category, mood, date range), pin popup preview |
| **Create / Edit Memory** | Mini-map for picking location (with pin), title, description, date, category dropdown, mood selector (emoji buttons), tag input (chips), photo uploader with previews, visibility, Save/Cancel |
| **Memory Details** | Title, date, location name + small map, mood, category, tags, photo gallery, description, Edit/Delete buttons (confirm dialog) |
| **Timeline** | Vertical timeline cards (date, location, title, cover photo, snippet), sort toggle, paging |
| **Search Results** | Search bar + filters, results list/cards, empty state, paging |
| **Profile** | Name, email (read-only), bio, change password form |

### Admin pages
| Page | Content |
|---|---|
| Admin Dashboard | Stat cards, memories-per-category chart (Chart.js), recent signups |
| User Management | Table: name, email, joined, memory count, status; activate/deactivate |
| Memory Management | Table: title, owner, date, category; view and remove |
| Category Management | Table with add/rename/deactivate |
| Reports / Statistics | Counts by category, memories per month, active users |

### UX rules
Loading spinners on API calls · toast messages for success/error · confirmation before delete · form errors shown next to fields · empty states with guidance.

---

## 11. Memory Creation Flow

| Step | Actor | Action |
|---|---|---|
| 1 | User | Clicks **Add Memory** |
| 2 | User | Clicks the map or searches for a place |
| 3 | Frontend | Captures latitude/longitude, fills location name via Nominatim |
| 4–10 | User | Enters title, description, date, category, mood, tags; chooses photos; selects visibility |
| 11 | Frontend | Client-side validation (required fields, file type/size) |
| 12 | Frontend | `POST /api/memories` (JSON) |
| 13 | Backend | Controller → `MemoryService` validates (rules in §13), resolves/creates tags, checks category exists |
| 14 | DAL | Saves memory + MemoryTags in one transaction |
| 15 | Frontend | Receives `memoryId`, then `POST /api/memories/{id}/photos` |
| 16 | Backend | `PhotoService` validates each file, saves with GUID name, stores records |
| 17 | Frontend | Adds the pin to the map and redirects to Memory Details |

**Partial failure:** if photo upload fails, the memory is kept and the user is told which photos failed and can retry from the Edit page.

---

## 12. Security Requirements

| Area | Requirement |
|---|---|
| Password hashing | PBKDF2/BCrypt with salt; never log or return hashes |
| Authentication | JWT (expiry 60 min) in HttpOnly + SameSite=Strict cookie; `[Authorize]` on all private endpoints; secret key in user-secrets/environment, not in Git |
| Authorization | `[Authorize(Roles="Admin")]` on admin endpoints |
| Ownership | Every memory/photo/tag query filters by `UserId` from the JWT claim, never from the request body; return 404 for others' data |
| Input validation | DTO validation attributes + service-level checks on every endpoint |
| SQL injection | EF Core/LINQ only (parameterized); no string-concatenated SQL |
| XSS | Never insert user text with `innerHTML` (use `textContent`) or encode it; ASP.NET returns JSON; add a Content-Security-Policy header |
| File upload | Check extension, content type and file signature (magic bytes); size limit; rename to GUID; store outside `wwwroot`; serve via authorized endpoint |
| CSRF | SameSite=Strict cookie; JSON-only endpoints |
| Rate limiting (S) | Limit login attempts (ASP.NET Core rate limiter) |
| Error handling | Global exception middleware returns generic messages; details logged server-side only |
| Transport | HTTPS + HSTS in production |

---

## 13. Validation Rules

| Field | Rules |
|---|---|
| **Full name** | Required, 2–100 chars |
| **Email** | Required, valid format, max 150, unique (case-insensitive) |
| **Password** | Required, 8–64 chars, at least 1 uppercase, 1 lowercase, 1 digit; confirm must match |
| **Login** | Email and password required; generic error on failure |
| **Memory title** | Required, 3–100 chars, trimmed |
| **Description** | Optional, max 2000 chars |
| **Date** | Required, valid date, not in the future, not before 1900-01-01 |
| **Latitude / Longitude** | Required, numeric; lat −90..90, lng −180..180 |
| **Location name** | Optional, max 200 chars |
| **Category** | Required, must exist and be active |
| **Mood** | Optional, must exist if provided |
| **Tags** | Max 10 per memory; each 2–30 chars; letters, numbers, `-`, `_`; stored lowercase; duplicates removed |
| **Images** | JPG/JPEG/PNG/WEBP; max **5 MB** each; max **5 photos per memory**; valid image signature |
| **Paging** | `pageSize` 1–50 (default 10) |
| **Bio** | Optional, max 300 chars |

---

## 14. Error Handling

Standard error body: `{ "status": 400, "message": "...", "errors": { "field": ["..."] } }`

| Situation | HTTP code | Message |
|---|---|---|
| Required field missing / validation fail | 400 | Validation failed (with field errors) |
| Invalid coordinates | 400 | Latitude/Longitude out of range |
| Invalid image type | 400 | Only JPG, PNG, WEBP images are allowed |
| Too many photos | 400 | Maximum 5 photos per memory |
| Invalid login | 401 | Invalid email or password |
| Not logged in / token expired | 401 | Please log in |
| Deactivated account | 403 | Account is deactivated |
| Not admin | 403 | You do not have permission |
| Memory/photo not found (or not yours) | 404 | Memory not found |
| Duplicate email / duplicate category | 409 | Email is already registered |
| File too large | 413 | File must be 5 MB or less |
| Unexpected server error | 500 | Something went wrong. Please try again |

---

## 15. Non-Functional Requirements

| Category | Requirement |
|---|---|
| Performance | API responds within 1 s for typical queries; map endpoint returns lightweight data; paging on lists; indexes on frequently filtered columns; thumbnails (S) |
| Security | As per Section 12 |
| Scalability | Stateless API (JWT) so it can scale later; photo storage abstracted behind `IPhotoStorage` to switch to cloud storage later |
| Maintainability | 3-tier separation, DI, interfaces, consistent naming, EF migrations, README with setup steps |
| Usability | Responsive layout, ≤3 clicks to add a memory, clear messages |
| Availability | Target "demo-ready" uptime; daily MySQL backup script (S) |
| Data integrity | Foreign keys, unique constraints, transactions for memory + tags, cascade rules, server-side validation |

---

## 16. Testing Strategy

| Type | Tool | Scope |
|---|---|---|
| Unit | xUnit + Moq | Services (validation, ownership, tag handling) |
| Repository/Integration | xUnit + EF Core InMemory or test MySQL DB | Repositories, filters, cascade delete |
| API | Postman collection / Swagger | All endpoints, status codes |
| Validation | xUnit + API tests | Boundary values |
| Authentication / Authorization | Postman | Missing/expired token, wrong role |
| File upload | Postman | Type, size, count, fake extension |
| Map | Manual | Pins, popup, filters, location pick, place search |
| CRUD | API + UI | Full create → read → update → delete |

### Sample Test Cases
| ID | Area | Test | Expected |
|---|---|---|---|
| T01 | Register | Valid data | 201, user in DB with hashed password |
| T02 | Register | Duplicate email | 409 |
| T03 | Register | Password "abc" | 400 with password error |
| T04 | Login | Correct credentials | 200, cookie set |
| T05 | Login | Wrong password | 401 generic message |
| T06 | Login | Deactivated user | 403 |
| T07 | Create memory | Valid data | 201, appears in `/memories/map` |
| T08 | Create memory | Missing title | 400 |
| T09 | Create memory | Latitude 95 | 400 |
| T10 | Create memory | Future date | 400 |
| T11 | Create memory | 11 tags | 400 |
| T12 | Get memory | Another user's id | 404 |
| T13 | Update memory | Another user's id | 404, data unchanged |
| T14 | Delete memory | Own memory | 204; photos/tags removed |
| T15 | Auth | No cookie on `/memories` | 401 |
| T16 | Authorization | User calls `/admin/stats` | 403 |
| T17 | Photo | 6 MB JPG | 413 |
| T18 | Photo | `.exe` renamed to `.jpg` | 400 (signature check) |
| T19 | Photo | 6th photo on a memory | 400 |
| T20 | Photo | Open another user's photo URL | 404/403 |
| T21 | Search | Keyword + category + date range | Only matching memories |
| T22 | Timeline | Sort oldest first | Ascending dates |
| T23 | Map | Filter by mood "Sad" | Only those pins shown |
| T24 | Map | Click map in Create page | Lat/lng fields filled |
| T25 | Admin | Deactivate user | User cannot log in |
| T26 | Admin | Remove memory | Disappears from owner's map |

---

## 17. Development Roadmap

| Phase | Tasks | Expected output | Depends on | Considerations |
|---|---|---|---|---|
| **1. Requirement & DB design** | Finalize PRD, draw ERD, list endpoints, sketch wireframes | ERD, wireframes | – | Freeze MVP scope here |
| **2. Project setup** | Create solution (Web/BLL/DAL/Tests), add packages (EF Core, Pomelo, JWT, Swagger), Git repo, `.gitignore` | Running empty API | 1 | Keep secrets out of Git |
| **3. Authentication (design)** | Decide cookie+JWT flow, password rules, role setup | Auth design notes | 2 | Done before coding controllers |
| **4. Database & EF Core** | Entities, `AppDbContext`, Fluent API, migrations, seed data | MySQL schema created | 2 | Check cascade rules and indexes |
| **5. Repository layer** | Interfaces + repositories (User, Memory, Category, Tag, Photo) | Tested data access | 4 | Return `IQueryable` only inside DAL |
| **6. Business logic layer** | Services, DTOs, validators, mapping, custom exceptions | Working services (unit tested) | 5 | Ownership checks live here |
| **7. API controllers** | Auth, Memories, Photos, Categories, Tags, Profile, Admin controllers; global error middleware | Swagger-testable API | 6 | Consistent error format |
| **8. Map integration** | Leaflet page, load pins, popups, click-to-pick, Nominatim search | Interactive map | 7 | Respect Nominatim limits, add attribution |
| **9. Memory CRUD (UI)** | Create/Edit/Details/Delete pages connected to API | End-to-end CRUD | 7, 8 | Confirm dialogs, error display |
| **10. Search & filter** | Filter API + search UI + map filter bar | Combined filters work | 9 | Index tuning |
| **11. Photo upload** | Multipart upload, validation, storage, secured file endpoint, gallery | Photos on memories | 9 | Magic-byte check, GUID names |
| **12. UI/UX polish** | Dashboard, timeline, responsive CSS, empty states, toasts | Complete user interface | 9–11 | Test on mobile screen |
| **13. Admin panel** | Stats, user/memory/category management pages | Admin features | 7 | Role checks |
| **14. Testing** | Unit, API, auth, upload, manual map tests; bug fixing | Test report | 1–13 | Use test table in §16 |
| **15. Deployment** | Publish build, host (IIS / Azure / VPS), production MySQL, HTTPS, env variables, demo data | Live link / demo package | 14 | Backup DB; set production secrets |

Suggested timeline: Phases 1–7 ≈ 40% of time, 8–12 ≈ 40%, 13–15 ≈ 20%.

---

## 18. Risks and Mitigation

| Risk | Impact | Mitigation |
|---|---|---|
| Scope creep | Project not finished | Strict MVP; Should/Future list |
| Map/Nominatim limits or downtime | Search fails | Debounce, 1 req/s, allow click-to-pick as fallback |
| Large photo uploads | Slow server, disk usage | 5 MB limit, 5 photos max, optional resize |
| Security mistakes (ownership, upload) | Data leak | Test cases T12, T13, T18, T20; code review checklist |
| MySQL/EF provider issues | Migration errors | Pin package versions; test migrations early |
| Time shortage | Missing features | Follow roadmap; demo-ready build after Phase 9 |
| Data loss | Lost memories | Backup script, no hard delete of users in MVP |

---

## 19. Future Scope (NOT part of MVP)

Shared memories and family memory map · Public/private map profiles · Comments/reactions · Memory sharing links · "On This Day" memories (optionally with email/SMS reminder) · Memory reminders · Export (PDF/ZIP) · Reporting/flagging by users · Mobile app/PWA · Offline support · AI-generated summaries.

---

## 20. Executive Summary (One Page)

**Product.** *Digital Memory Map* is a web app that lets users save personal memories — title, story, date, photos, category, mood and tags — pinned to a real location on an interactive OpenStreetMap/Leaflet map. Users revisit memories through three views: the **map**, a **chronological timeline**, and **search/filters** (keyword, location, date range, category, mood, tag).

**Problem.** Memories are scattered across galleries and chats, and galleries cannot answer place-based questions. This project connects *where, when and how it felt* in one personal, private space.

**Users and roles.** *Users* manage their own memories and photos. *Admins* manage users, categories, remove inappropriate memories, and view statistics. Memories are private by default; ownership is enforced on every request.

**Technology and architecture.** ASP.NET Core Web API (C#) with a 3-tier structure: Presentation (controllers + HTML/CSS/JS frontend), Business Logic (Memory, User, Category, Tag, Photo, Admin services with DTOs and validation), and Data Access (repositories using EF Core/LINQ over MySQL). Authentication uses JWT in an HttpOnly cookie, passwords are hashed, and photos are stored on disk with only paths in the database.

**Data model.** Eight core tables: Roles, Users, Categories, Moods, Memories, MemoryPhotos, Tags, MemoryTags (3NF, tags per user, many-to-many via MemoryTags).

**API.** RESTful endpoints for auth, profile, memories (CRUD, list, map pins, nearby), photos, categories, moods, tags and admin operations, with a consistent error format and proper HTTP codes (400, 401, 403, 404, 409, 413, 500).

**MVP.** Authentication, memory CRUD, photo upload (JPG/PNG/WEBP, 5 MB, 5 per memory), interactive map with pins/popups/place search/location picking, map filters, timeline, search, and a simple admin panel. *Should have:* nearby memories, clustering, timeline grouping, profile photo. *Future:* sharing, reactions, reminders, export, mobile app, offline mode, AI summaries.

**Security and quality.** Hashing, JWT + roles, ownership validation, strict input and file validation, EF parameterized queries, XSS-safe rendering, secure error handling; 26 sample test cases covering CRUD, auth, authorization, upload and map.

**Delivery.** A 15-phase roadmap from requirements and database design to deployment, with the core working version ready after Phase 9 and polish, admin, testing and deployment in the remaining phases.

**Success criteria.** A user can register, add a memory by clicking the map with photos and a mood, see it as a pin, find it later by search or timeline, and no other user can access it; an admin can moderate content and view statistics.
