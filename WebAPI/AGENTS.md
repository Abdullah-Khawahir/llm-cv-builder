**Backend (C# .NET Web API)**
- Run API locally: `dotnet run --project WebAPI.csproj`
- Apply migrations (SQLite): `dotnet ef database update --project WebAPI.csproj`
- Add migration: `dotnet ef migrations add <Name> --project WebAPI.csproj`
- View OpenAPI docs: `http://localhost:<port>/docs` (Scalar UI) after starting the API in dev mode.

**Database**
- SQLite file located at `app.db`.
- Connection string read from `appsettings.json` under `ConnectionStrings:DefaultConnection`.

**Testing**
- No test projects detected; add tests under a `*.Tests` project and run with `dotnet test`.

**General**
- All commands should be run from the `backend/WebAPI` folder unless a different path is required.
- Use `dotnet --info` to verify .NET SDK version (requires .NET 10.0).
- See detailed backend development guidelines in [GUIDELINES.md](GUIDELINES.md).