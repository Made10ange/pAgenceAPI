# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Run Commands

```bash
# Build the full solution
dotnet build "AgenceVoyage.Front/pAgenceV/pAgenceV.sln"

# Run the frontend MVC app (starts on https://localhost:7xxx / http://localhost:5xxx)
dotnet run --project "AgenceVoyage.Front/pAgenceV/pAgenceV.csproj"

# Run the API backend (starts on http://localhost:5049)
dotnet run --project "AgenveVoyage.API/pAgenceAPI/pAgenceAPI.csproj"
```

Both projects must be running simultaneously for the application to function. The frontend reads the API base URL from `AgenceVoyage.Front/pAgenceV/appsettings.json` (`ApiBaseUrl: http://localhost:5049/api`).

## Architecture

Two separate ASP.NET Core 8.0 projects in the same solution (`AgenceVoyage.Front/pAgenceV/pAgenceV.sln`):

- **`AgenceVoyage.Front/pAgenceV/`** — ASP.NET Core MVC frontend. Controllers call the API over HTTP using `HttpClientFactory`. Views are Razor pages for CRUD operations.
- **`AgenveVoyage.API/pAgenceAPI/`** — ASP.NET Core REST API backend. Uses the repository pattern with Dapper for all data access (raw SQL against MySQL). Entity Framework Core is wired up via `ApplicationDbContext` but is not actively used for queries.

### Domain Entities

Core entities: `Voyage` (trip), `Agence` (agency), `Chauffeur` (driver), `Vehicule`, `Passager`, `TypeVoyage`, `TypeVehicule`, plus junction/assignment entities: `AffectationChauffeurAgence`, `AffectationVehiculeAgence`, `AssignationChauffeurVoyage`, `EmbarquementVoyagePassager`.

### API Route Convention

Routes follow the pattern `/api/[Controller]/[Action]`:
- `GET /api/Agences/liste` — fetch all records
- `GET /api/Agences/{id}` — fetch by ID
- `POST /api/Agences/ajouter` — create
- `PUT /api/Agences/modifier` — update
- `DELETE /api/Agences/supprimer/{id}` — delete

JSON output uses **PascalCase** to match C# model properties.

### Data Access

All repositories implement an interface (e.g., `IVoyageRepository`) and use Dapper with raw SQL. Repositories are registered in `AgenveVoyage.API/pAgenceAPI/Program.cs`. When adding a new entity:
1. Add model in `Models/`
2. Create interface in `Repositories/`
3. Implement repository in `Repositories/`
4. Register in `Program.cs`
5. Add controller in `Controllers/parametres/`

### Frontend Service Layer

Frontend services (e.g., `Services/parametre/AgenceService.cs`) wrap `HttpClient` calls and handle JSON deserialization. MVC controllers use `TempData` for success/error messages and model state for validation feedback.

## Key Configuration

| Setting | Location | Value |
|---|---|---|
| MySQL connection | `AgenveVoyage.API/pAgenceAPI/appsettings.json` | `localhost`, DB `bd_agence`, user `root`, no password, port `3306` |
| API base URL (frontend) | `AgenceVoyage.Front/pAgenceV/appsettings.json` | `http://localhost:5049/api` |
| CORS | API `Program.cs` | Permissive `AllowAll` policy |
| Swagger UI | API | Enabled, accessible at `/swagger` in development |

## Where to Make Changes

- **UI / views**: `AgenceVoyage.Front/pAgenceV/Views/` and `Controllers/`
- **API endpoints**: `AgenveVoyage.API/pAgenceAPI/Controllers/parametres/`
- **Business/data logic**: `AgenveVoyage.API/pAgenceAPI/Repositories/`
- **Shared models (frontend)**: `AgenceVoyage.Front/pAgenceV/Models/Parametre/`
- **Shared models (API)**: `AgenveVoyage.API/pAgenceAPI/Models/`

Nullable reference types are enabled in both projects.
