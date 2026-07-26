# ShareIT — Bug Report & Schema Change Log

**Project:** ShareIT (Elsewedy Electric employee‑voice portal)
**Scope of this report:** the "Ticket Types" (Phase 1) work — adding the 6 submission
types, the per‑type detail forms, the new create wizard — and every bug fixed along the way.

---

## Part 1 — Bugs we hit and how we fixed them

For each bug: **Symptom → Root cause → Where in the code → The fix → How to spot this class of bug next time.**

### 🥇 The main issue: the wizard got stuck on step 2 ("reload loop")

- **Symptom:** From the create wizard, step 1 → step 2 worked, but on step 2 clicking **Continue** just reloaded step 2. Never reached step 3.
- **Root cause:** The wizard keeps the current step in a hidden field, `<input asp-for="CurrentStep" />`. After a POST, ASP.NET's `asp-for` tag helper renders a field's value from **`ModelState`** (the value the browser just submitted), **not** from the model object. So when step 1 posted `CurrentStep=1` and the controller advanced `model.CurrentStep = 2`, the hidden field still rendered the stale **`1`**. Clicking Continue on step 2 then posted `1` again → the server re‑ran step 1 → bounced back to step 2 → an invisible **step 1 ↔ step 2 loop**.
- **Where:** `Views/Ticket/New.cshtml` (the hidden `CurrentStep` field) + `Controllers/TicketController.cs` (`New` POST, which mutates `model.CurrentStep`).
- **The fix (one line, standard pattern):** clear the stale value so the tag helper falls back to the model:
  ```csharp
  // top of the New POST action
  ModelState.Remove(nameof(model.CurrentStep));
  ModelState.Remove(nameof(model.UploadedFilePaths)); // same reason
  ```
- **How to spot it:** if a value is **both a form field and something the controller changes**, and the page "won't move" / re‑renders with an old value → suspect ModelState‑over‑model. Fingerprint: the *first* transition works (no ModelState yet on a fresh GET) but the *next* one loops. Diagnose by viewing the rendered HTML and comparing the hidden field's `value=""` against `@Model.<field>`; if they disagree, that's it.

### Wizard step 1 wouldn't advance at all

- **Symptom:** Picking Anonymous/Disclose then Continue reloaded step 1.
- **Root cause:** `Description`, `Password`, `ConfirmPassword` on the wizard view model were `[Required]` (and non‑nullable, which adds an *implicit* required rule). Those belong to later steps, so on step 1 they're empty → the **whole model** `ModelState.IsValid` was false → the per‑step advance gate `if (ModelState.IsValid)` never passed.
- **Where:** `Models/ViewModel/TicketModel.cs` (`NewTicketViewModel`).
- **The fix:** made those three `string?` and dropped `[Required]`. The controller validates them **per step** instead (Description at step 3, Password at step 5).
- **How to spot it:** multi‑step forms should **not** put `[Required]` on the whole model. If an early step won't submit, check whether validation attributes on *later‑step* fields are poisoning `ModelState.IsValid`.

### Validation didn't block empty employee details

- **Symptom:** Choosing "Yes, I'm an Employee" and continuing with blank fields still advanced.
- **Root cause:** the employment cards are styled `<div onclick="…">` wrapping a hidden radio. The click showed the employee form but **never checked the radio**, so `IsEmployee` posted empty → the server's `if (IsEmployee == "Yes")` validation branch was skipped.
- **Where:** `Views/Shared/_ReporterStep.cshtml` (`selectEmployment` JS) + `Controllers/TicketController.cs`.
- **The fix:** `selectEmployment` now sets `document.getElementById('employeeYes').checked = …`, and the controller adds an "employment status is required" check.
- **How to spot it:** "validation silently passes" usually means **the field never posted**. Check that custom‑styled inputs (hidden radios/checkboxes toggled by JS) actually set a value the server receives.

### Detail models came out empty

- **Symptom:** Build errors — `IssueDetail` etc. "do not contain a definition for `Ticket`".
- **Root cause:** the detail model files were pasted as empty class shells (`public class IssueDetail { }`), so the DbContext's 1‑to‑1 configuration (`HasOne(d => d.Ticket)…`) had nothing to bind to.
- **Where:** `Models/IssueDetail.cs` … `SafetyDetail.cs`, referenced by `Data/ApplicationDbContext.cs` `OnModelCreating`.
- **The fix:** filled the models with their fields + the `[Key] TicketId` + `Ticket` navigation.
- **How to spot it:** "does not contain a definition for X" on a navigation property → the target class is missing that member (often an empty/half‑saved file). Open the class and confirm it actually has the properties.

### "Track My Case" routed to the admin login

- **Symptom:** On the confirmation page, **Track My Case** redirected to the admin login instead of the public tracking page.
- **Root cause:** the button linked to `/FollowTicket/Index`. `FollowTicketController` is `[Authorize]` (staff case management), so anonymous users get redirected to login. The **public** tracking page is `SearchController` (no `[Authorize]`).
- **Where:** `Views/Ticket/Confirmation.cshtml`.
- **The fix:** changed the link to `/Search` (and later `"/Search?refNo=@refNo"` to pre‑fill the reference).
- **How to spot it:** *"redirects to login"* almost always = the target action is `[Authorize]`‑protected. Check the auth attribute on the **destination** controller/action, and confirm the link points where you intend.

### Document create silently failed + list page crashed (earlier, same class of bugs)

- **Symptoms:** creating a compliance document "cleared the form and did nothing"; `/Document` threw `Sequence contains no elements`.
- **Root causes:** (1) `Document.Logo` was a non‑nullable `string` → EF made the column `NOT NULL`, but Logo is optional → inserting `null` threw `DbUpdateException`, swallowed by a `try/catch`, re‑rendering the cleared form. (2) `Index.cshtml` called `.Max()` on an empty list. (3) A non‑nullable `int DisplayOrder` with `[Range]` failed to bind when left blank.
- **Fixes:** `Logo` → `string?` (nullable column); guarded `.Max()` with `Model.Any() ? … : 0`; `DisplayOrder` → `int?`.
- **How to spot it:** a "form clears and nothing saves" pattern → look for a **swallowed exception** (`catch` that only re‑renders) and a **nullable mismatch** between the C# property (nullable annotations on) and the DB column. Aggregates like `.Max()/.Average()/.First()` on possibly‑empty collections crash — guard them.

### The recurring red herring: stale build / VS file lock (not a code bug)

- **Symptom:** fixes "didn't apply"; the page looked broken/incomplete after a rebuild.
- **Cause:** Visual Studio's IIS Express keeps the compiled DLL locked, so a rebuild while the app is running silently fails to replace it — you keep running an old binary. Razor views are compiled at build time, so stale build = stale views.
- **How to avoid:** **Stop debugging (Shift+F5) before rebuilding.** If a build reports the DLL is "in use," kill `iisexpress.exe`, then Clean + Rebuild. When in doubt, run from the terminal (`dotnet run`) on a fixed port and hard‑refresh the browser (Ctrl+F5) to confirm you're testing the real code.

---

## Part 2 — What changed in the database schema

The old app stored a **generic complaint**: one `Ticket` with a single free‑text description and a
category picked from a lookup table. Phase 1 turns that into **6 typed submissions**, each with
its own structured fields.

### Renamed

| Old | New | Why |
|---|---|---|
| entity `TicketType` (the category lookup: Harassment, Safety Violation…) | **`Category`** | The name `TicketType` was needed for the new enum. This table is now a simple category lookup. |
| entity `SubTicketType` | `SubCategory` | Consistency with the rename above. |
| `Ticket.Kind` (enum `TicketKind`: Complaint/Suggestion/Feedback) | **`Ticket.TicketType`** (enum `TicketType`, 6 values) | The core reclassification. DB column renamed in place via EF `RenameColumn` (data preserved). |
| `Ticket.TicketTypeId` / nav `TicketType` | `Ticket.CategoryId` / nav `Category` | Follows the entity rename. |

### Added

**Enum `TicketType`** (the 6 submission types):
`Issue, Idea, ProjectProposal, SafetyConcern, Feedback, SystemRequest`

**`Ticket.Title`** — every submission now has a short title (`nvarchar`, nullable in DB, required on the form).

**Five per‑type detail tables** — each is **1‑to‑1 with `Ticket`**, sharing the ticket's id as its
primary key (`[Key] int TicketId`, cascade‑delete). Only the table matching the ticket's
`TicketType` is filled; **System Request has no detail table** (it just uses Title + Description).

| Table | Key columns (attributes) |
|---|---|
| **IssueDetail** | `IssueType (string?)`, `ImpactArea (enum IssueImpactArea)`, `ResponsibleDepartmentId (int? → Department)`, `RootCause (string?)`, `ProposedSolution (string?)`, `SolutionCategory (enum? SolutionCategory)` |
| **IdeaDetail** | `Scope (enum IdeaScope)`, `ImpactType (enum)`, `ResponsibleDepartmentId (int?)`, `ImpactDescription (string?)`, `Sustainability (string?)` |
| **ProjectProposalDetail** | `BusinessNeed (string?)`, `EstimatedCost (decimal?)`, `Scope (string?)`, `ExpectedBenefits (string?)`, `RoiReach (enum)`, `KeyStakeholders (string?)`, `TimelineStart (DateTime?)`, `TimelineEnd (DateTime?)` |
| **SafetyDetail** | `PotentialImpact (string?)`, `RiskLevel (enum)`, `HazardCategory (enum SafetyHazard)`, `SuggestedAction (string?)`, `ResponsibleDepartmentId (int?)` |
| **FeedbackDetail** | `Category (enum FeedbackCategory)`, `ResponsibleDepartmentId (int?)`, `ImpactArea (string?)`, `PreferredResolution (enum)`, `Confidentiality (enum)`, `ActionType (enum FeedbackActionType)` |

**Supporting enums added:** `IssueImpactArea`, `SolutionCategory`, `IdeaScope`, `ImpactType`,
`RoiReach`, `RiskLevel`, `SafetyHazard`, `FeedbackCategory`, `PreferredResolution`,
`Confidentiality`, `FeedbackActionType`.

### Removed

- Enum **`TicketKind`** (Complaint/Suggestion/Feedback) — superseded by `TicketType`.
- The old **case‑type lookup values** (Harassment, Safety Violation, Fraud & Corruption,
  Discrimination, Process Improvement, Product Idea, General Feedback) — the `Category` table is
  now seeded with the 6 type names instead (Issue, Idea, Project Proposal, Safety Concern,
  Feedback, System Request).

### The `Ticket` table today (shape)

`Id`, **`Title`**, **`TicketType` (enum)**, `CategoryId → Category`, `ReporterId → Reporter`,
`Status`, `RefNo`, `password` (tracking password), `Desc`, `Concerning{Company,Department,Other,Person}`,
`finalRes`, `langFlag`, `forwarding`, `toMails`, `ccMails`, `messageMail`, `validation`,
`closureDate`, audit columns (`CreatedOn/By`, `UpdatedOn/By`), plus 1‑to‑1 navigations to the five
detail tables and 1‑to‑many `Attachments` / `Chats` / `TicketTimelines`.

### Migration note

Applied via a standard **roll‑forward** migration (`AddTicketTypesAndDetails`) — history preserved,
no squashing (important now that the repo is shared). EF generated a `RenameColumn` for
`Kind → TicketType` (data kept) and `CreateTable` for the five detail tables.

---

## Part 3 — Out of scope (planned Phase 2)

Business Units (BU) as a required dimension, the New/In Review/Actioned/Closed status lifecycle,
Urgency, HRBP/HSE roles + auto‑routing, and the 5‑year retention policy for suggestions.
