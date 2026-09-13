# ShareIT

Employee-voice web app for **Elsewedy Electric** — captures **complaints, suggestions, and
general feedback** as *tickets*, with confidentiality options, follow-up tracking, role-based
admin tools, and dashboards. (Evolved from the older complaints-only "SpeakUp" project.)

## Tech stack

- ASP.NET Core MVC, **.NET 8**
- Entity Framework Core (SQL Server / LocalDB)
- ASP.NET Core Identity (role-based access)
- Bootstrap 5 UI

## Getting started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download) (or newer)
- SQL Server **LocalDB** (ships with Visual Studio) — or update the connection string
- EF Core tools: `dotnet tool install --global dotnet-ef`

### 1. Clone
```bash
git clone https://github.com/MarwanElAshry04/ShareIT.git
cd ShareIT
```

### 2. Configure secrets (required for email sending)
The SMTP password is **not** stored in the repo. Set it locally via user-secrets:
```bash
dotnet user-secrets set "Email:Password" "<the-smtp-password>"
```
Non-secret email settings (host, port, sender address) live in `appsettings.json` under
`"Email"` and can be overridden the same way. Ask a teammate for the current password
(and consider rotating it — it was previously committed in plain text).

### 3. Create the database
```bash
dotnet ef database update
```

### 4. Run
```bash
dotnet run
```
On first launch the app seeds demo data (companies, departments, ticket types, and sample
tickets) plus the default accounts below.

## Default accounts (dev seed)

| Role       | Email                  | Password       |
|------------|------------------------|----------------|
| Super Admin| `admin@shareit.com`    | `P@ssword123`  |
| Basic user | `basicuser@domain.com` | `P@ssword123`  |

> Change these before any non-local deployment.

## Project layout

| Path            | Purpose                                             |
|-----------------|-----------------------------------------------------|
| `Controllers/`  | MVC controllers (Ticket, FollowTicket, Home, …)     |
| `Models/`       | Entities + `ViewModel/` for view models             |
| `Views/`        | Razor views                                         |
| `Data/`         | `ApplicationDbContext` and EF `Migrations/`         |
| `Services/`     | Email + timeline services                           |
| `Seeds/`        | Roles, users, and demo-data seeders                 |
| `Constant/`     | Roles, permissions, module constants                |
| `wwwroot/`      | Static assets (uploads are git-ignored)             |

## Notes

- A ticket is one of six types (`Models/TicketEnums.cs`): **Issue**, **Idea**, **Project
  Proposal**, **Safety Concern**, **Feedback**, **System Request**. The submission wizard shows
  a different details step per type, so the fields required depend on the type chosen.
- The employee-facing site switches between English and Arabic (with right-to-left layout) via
  `CultureController`; the admin panel stays English.
- If you drop the database, re-run `dotnet ef database update` then `dotnet run` to reseed.

## Further reading

`docs/WHAT-WE-CHANGED.md` is a plain-language walkthrough of the most recent work (tutorial
videos, admin/employee separation, the language toggle, the account page) and the bugs found
along the way. `docs/ShareIT-presentation.pptx` is the slide deck.
