# Progress log

A short daily record of what got built, what I learned, and what's next. Newest first.

---

## 2026-11-25: .NET installed, solution created

**Done**
- Installed the .NET 10 SDK and the VS Code C# Dev Kit.
- Created the solution (`LineTrack.slnx`) with Core, Infrastructure, Api and Core.Tests projects, referenced so everything depends on Core and Core depends on nothing.
- Shared build settings in `Directory.Build.props` (nullable on, warnings as errors), SDK pinned in `global.json`, code style in `.editorconfig`.
- Removed the template sample code. The API starts and `/health` returns `Healthy`. Release build has 0 warnings.

**Learned**
- C# records, `with` copies, nullable reference types, `?.` / `??` and switch expressions (practised in a local playground).
- How MSBuild picks up `Directory.Build.props` from parent folders automatically.

**Next**
- C# practice: LINQ and async/await.
- Install Docker Desktop, run PostgreSQL, add the Core entities and the first EF Core migration.

---

## 2026-11-25: Project set up

**Done**
- Wrote the project plan: features, data model, API design and a 4-week roadmap.
- Created the repository with the README, architecture diagram and project structure.

**Learned**
- How to plan a layered .NET solution (Core → Infrastructure → Api) so business rules don't depend on the database.

**Next**
- Install the .NET 10 SDK and Docker Desktop.
- Create the solution and projects, then the Core entities.
