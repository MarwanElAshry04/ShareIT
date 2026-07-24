# ShareIT Transition & Refactor — Design

**Date:** 2026-07-22
**Project:** SpeakUp (employee complaints) → **ShareIT** (complaints, suggestions, general feedback)
**Stack:** ASP.NET Core MVC, .NET 8, EF Core (SQLite + SqlServer), ASP.NET Identity

## Goal

Complete the in-progress rename of the app from **SpeakUp** to **ShareIT**, broaden the
domain from complaints-only to complaints + suggestions + feedback, and refactor the file
structure so there are no redundant files and the layout is clear and correct.

## Decisions (locked)

1. **Depth:** Full rename — both brand (`SpeakUp` → `ShareIT`) and domain (`Complaint` → `Ticket`).
2. **Entity name:** `Ticket` is the umbrella record (neutral across all three kinds).
3. **Kind discriminator:** Add a required `Kind` enum (`Complaint | Suggestion | Feedback`),
   chosen by the user at submission; surfaced in nav/filter/dashboard.
4. **Cleanup:** Remove empty `Services/`, dead `Models/other` files, and old `SpeakUp.png`.
5. **DB:** Dev DB is disposable — **squash** all migrations into one clean `InitialCreate`.
6. **Project files:** Keep `SpeakUp.sln` / `SpeakUp.csproj` filenames; set `RootNamespace`
   and `AssemblyName` to `ShareIT`. `.sln` folder stays `speakup` (working dir).
7. **Version control:** No git (proceeding without commits).

## 1. Brand rename: `SpeakUp` → `ShareIT`

- Global replace of the `SpeakUp` root namespace segment → `ShareIT` across all `.cs` and
  `.cshtml` files: `namespace` declarations, `using`, `@using`, `@model`, `@inject`. (~127 files.)
- `SpeakUp.csproj`: add `<RootNamespace>ShareIT</RootNamespace>`, `<AssemblyName>ShareIT</AssemblyName>`;
  update the `UserSecretsId` label. Filename stays `SpeakUp.csproj`.
- Finish remaining "SpeakUp" UI strings in views → "ShareIT".
- Standardize logo filenames; ensure all view `img` references resolve.

## 2. Domain rename: `Complaint` → `Ticket`

| Old | New |
|---|---|
| `Complaint` (model) | `Ticket` |
| `ComplaintType` | `TicketType` |
| `ComplaintTimeline` | `TicketTimeline` |
| `SubComplaintType` | `SubTicketType` |
| `ComplaintController` | `TicketController` |
| `ComplaintTypesController` | `TicketTypesController` |
| `FollowComplaintController` | `FollowTicketController` |
| `Views/Complaint/*` | `Views/Ticket/*` |
| `Views/ComplaintTypes/*` | `Views/TicketTypes/*` |
| `Views/FollowComplaint/*` | `Views/FollowTicket/*` |
| `_ComplaintStep.cshtml` | `_TicketStep.cshtml` |
| `AllComplaints.cshtml` | `AllTickets.cshtml` |
| `wwwroot/js/complaint.js` | `wwwroot/js/ticket.js` |
| ViewModels `Complaint*` | `Ticket*` |
| Nav props `ComplaintChats`, `ComplaintAttachments`, `ComTypeId`, `ChatComplaintId`, `ComplaintTimelines` | `TicketChats`, `TicketAttachments`, `TicketTypeId`, `ChatTicketId`, `TicketTimelines` |
| DbSets in `ApplicationDbContext` | renamed to match |

Routes shift `/Complaint` → `/Ticket`, `/FollowComplaint` → `/FollowTicket`, etc. All internal
`asp-controller`, `Url.Action`, `RedirectToAction` references updated to match.

**Not renamed (intentional):** `Reporter` (the submitting person), `Chats`, `Attachment`,
`Document`, `Video`, `Company`, `Department`, `Relation`, `Employee`, `Mail`. `AgCompany`/
`AgDepartment`/`AgPerson` ("against …") fields stay — nullable, complaint-specific but harmless.

## 3. New `Kind` discriminator

- `enum TicketKind { Complaint, Suggestion, Feedback }` (stored as `int`, default `0` = Complaint).
- `Ticket.Kind` added to the model + `InitialCreate` migration.
- Create wizard: add a **Kind selector** as the first step (radio/segmented control).
- `FollowTicket` list views: add a **Kind column + filter**.
- Dashboard: add a **per-Kind count** breakdown to the existing stats.
- Scope guard: no other UI redesign; existing complaint fields remain optional for the
  suggestion/feedback kinds.

## 4. File-structure cleanup

- Delete empty `Services/`; rename `Service/` → `Services/` (conventional plural) with its
  `IEmailService.cs`, `TimelineService.cs`. Update `Program.cs` / usings.
- Delete dead code: `Models/other/{Exam,Training,Question,Answer,Paragraph}.cs`
  (referenced nowhere). Move `SubTicketType.cs` up to `Models/`; remove empty `Models/other/`.
- Delete `wwwroot/SpeakUp.png`; keep/standardize `ShareIt*` logos.
- Leave `bin/`, `obj/`, `.vs/` untouched (regenerated build/IDE artifacts).

## 5. EF migrations — squash

- Delete all 30 files under `Data/Migrations/` **and** `ApplicationDbContextModelSnapshot.cs`.
- Generate a single fresh `InitialCreate` migration from the renamed `Ticket` model
  (tables named `Tickets`, `TicketTypes`, etc., with the `Kind` column).
- Drop and recreate the dev database so schema matches.

## 6. Verification

1. `dotnet build` — zero errors (namespace + rename integrity).
2. `grep` sweep confirms no stray `SpeakUp` / `Complaint` identifiers remain in source
   (excluding intentional keeps and historical prose in docs).
3. `dotnet ef database update` (fresh DB) succeeds.
4. App boots; create-ticket wizard shows the Kind selector; FollowTicket + Dashboard render.

## Out of scope

- Renaming the `SpeakUp.sln` / `SpeakUp.csproj` filenames or the project folder.
- Reworking auth, email, chat, document, or video features beyond the rename.
- Any new suggestion/feedback-specific workflows beyond the Kind discriminator + filtering.
