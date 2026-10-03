# Digital Memory Map 🗺️

A modern, responsive web application where users record personal memories (photos, stories, mood, tags) and pin them to real-world locations on an interactive Leaflet + OpenStreetMap map.

---

## 🏛️ Architecture

Digital Memory Map is implemented in a strict **3-Tier Architecture**:

```
Browser (HTML5 / Vanilla CSS / Vanilla JS + Leaflet.js)
        │  REST API (JSON / multipart), HttpOnly JWT cookie
        ▼
┌────────────────────────────────────────────────────────┐
│ 1. PRESENTATION LAYER  (DigitalMemoryMap.Web)           │
│    - API Controllers, Exception Middleware, wwwroot   │
├────────────────────────────────────────────────────────┤
│ 2. BUSINESS LOGIC LAYER (DigitalMemoryMap.BLL)          │
│    - Services, DTOs, Input & File Validation, Rules    │
├────────────────────────────────────────────────────────┤
│ 3. DATA ACCESS LAYER   (DigitalMemoryMap.DAL)          │
│    - AppDbContext, 3NF Entities, LINQ Repositories     │
└────────────────────────────────────────────────────────┘
        ▼
     MySQL Database (Pomelo EF Core) + /uploads Directory
```

---

## 🚀 Key Features

- **Interactive Leaflet Map**: Browse all your memories as custom emoji pins. Click to view instant previews and navigate to details.
- **Location Picker & Nominatim Search**: Click directly on the map to pin coordinates with automatic reverse geocoding to fill location names.
- **Timeline View**: Chronological memories feed (newest/oldest) with cover photos, dates, categories, and load more pagination.
- **Search & Multi-Filter**: Search by keyword, location, date range, category, mood, and tag chips (combined with AND logic).
- **Photo Management**: Upload up to 5 photos per memory (JPG, PNG, WEBP, max 5 MB each) with magic-byte security checks, stored safely outside `wwwroot` and served via an authenticated streaming endpoint.
- **Admin Moderation Panel**: Real-time platform statistics, user activation/deactivation, category management, and memory moderation.
- **Secure Authentication**: Password hashing with BCrypt, JWT in HttpOnly + SameSite=Strict cookies to protect against XSS and token leaks.

---

## 🛠️ Tech Stack

- **Backend**: C# (.NET 10.0), ASP.NET Core Web API
- **ORM**: Entity Framework Core with `Pomelo.EntityFrameworkCore.MySql`
- **Database**: MySQL Server 8.0+
- **Security**: JWT Bearer + BCrypt.Net-Next
- **Testing**: xUnit + Moq (19 unit & validation tests passing)
- **Frontend**: Semantic HTML5, Custom Design System (Vanilla CSS with Google Fonts *Outfit* & *Inter*), Vanilla JavaScript ES6+, Leaflet.js 1.9.4 + OpenStreetMap tiles

---

## ⚙️ Setup & Running

### 1. Database Connection
Update your MySQL credentials in `DigitalMemoryMap.Web/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=digital_memory_map;User=root;Password=your_password;CharSet=utf8mb4;"
}
```

### 2. Run the Application
```bash
dotnet run --project DigitalMemoryMap.Web
```
The application will launch on your local port (e.g. `http://localhost:5000` or `https://localhost:7001`).
On first launch, EF Core will automatically initialize the database schema and seed default roles, categories, moods, and the admin user.

### 3. Run Automated Tests
```bash
dotnet test
```

---

## 🔑 Default Accounts

| Role | Email | Password |
|---|---|---|
| **Admin** | `admin@digitalmemory.com` | `Admin@123` |
| **User** | Register any new account via `/register.html` | User-defined |

---

## 📁 Solution Structure

```
├── DigitalMemoryMap.Web/         # Presentation Layer (Controllers, Middleware, wwwroot)
├── DigitalMemoryMap.BLL/         # Business Logic Layer (Services, DTOs, Exceptions)
├── DigitalMemoryMap.DAL/         # Data Access Layer (DbContext, Entities, Repositories)
├── DigitalMemoryMap.Tests/       # Unit & Integration Tests (xUnit, Moq)
├── uploads/                      # Secure server-side photo storage (outside wwwroot)
└── DigitalMemoryMap.slnx         # Solution file
```
