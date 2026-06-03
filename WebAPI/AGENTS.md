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

## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

When the user types `/graphify`, invoke the `skill` tool with `skill: "graphify"` before doing anything else.

Rules:
- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- Dirty graphify-out/ files are expected after hooks or incremental updates; dirty graph files are not a reason to skip graphify. Only skip graphify if the task is about stale or incorrect graph output, or the user explicitly says not to use it.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).
