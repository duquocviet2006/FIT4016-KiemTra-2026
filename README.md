# School Management (EF Core)

This sample ASP.NET Core MVC app demonstrates an English-language School Management system with EF Core (SQLite).

Features:
- Models: `School`, `Student` with constraints and timestamps
- CRUD for students with server-side validation
- Pagination (10 students per page)
- Seed data: 10 schools and 20 students

To run:

```powershell
cd "d:/Back-end/Back/SchoolManagement"
dotnet restore
dotnet run
```

Open http://localhost:5000 (or printed URL) to use the app.
