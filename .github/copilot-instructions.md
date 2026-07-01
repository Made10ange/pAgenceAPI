# Workspace instructions for AI assistance

## What this repo contains
- `AgenceVoyage.Front/pAgenceV`: ASP.NET Core MVC front-end application.
- `AgenveVoyage.API/pAgenceAPI`: ASP.NET Core Web API backend.
- `AgenceVoyage.Front/pAgenceV/pAgenceV.sln`: Visual Studio solution that includes both the frontend and API projects.

## Recommended workflow for code changes
- Use `AgenceVoyage.Front/pAgenceV` for UI and MVC controller work.
- Use `AgenveVoyage.API/pAgenceAPI` for backend API logic, data access, and repository changes.
- Keep application behavior aligned with the existing MVC + API split.

## Build and run commands
- Build solution:
  - `dotnet build "AgenceVoyage.Front/pAgenceV/pAgenceV.sln"`
- Run front-end app:
  - `dotnet run --project "AgenceVoyage.Front/pAgenceV/pAgenceV.csproj"`
- Run API app:
  - `dotnet run --project "AgenveVoyage.API/pAgenceAPI/pAgenceAPI.csproj"`

## Key architecture details
- Both projects target `.NET 8.0`.
- The front-end app is MVC-based and calls the API using `HttpClientFactory`.
- The API uses Entity Framework Core with `Pomelo.EntityFrameworkCore.MySql`.
- `AgenveVoyage.API/pAgenceAPI/appsettings.json` configures MySQL via `DefaultConnection`.
- API controllers and repository interfaces are the main extension points for backend changes.

## Conventions and important notes
- The API JSON output uses PascalCase to match C# model properties.
- The API currently enables a permissive CORS policy named `AllowAll`.
- The default DB connection string assumes MySQL on `localhost`, database `bd_agence`, user `root`, empty password, port `3306`.
- Nullable reference types are enabled in both projects.

## When editing code
- For UI changes: modify `Controllers`, `Views`, or `wwwroot` inside `AgenceVoyage.Front/pAgenceV`.
- For API changes: modify `Controllers`, `Repositories`, `Models`, and `ApplicationDbContext` inside `AgenveVoyage.API/pAgenceAPI`.
- Preserve existing folder layout and avoid creating a separate root solution unless explicitly requested.

## Helpful prompts for this repo
- "Add a new API endpoint in `pAgenceAPI` to update `Voyage` status and return the updated model."
- "Update the `Agence` edit view in `pAgenceV` to show model validation messages and preserve user input on errors."
- "Refactor repository registration in `AgenveVoyage.API/pAgenceAPI/Program.cs` so dependencies are registered in a dedicated extension method."

## What this file is not
- This is not a full project README.
- It does not replace deeper architectural documentation; use the source code when details are missing.
