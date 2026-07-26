# Ticket Types (Phase 1) — Design

**Date:** 2026-07-24
**Scope:** Lean. Add the 6 ticket types + per-type detail forms + a new create-wizard flow.
Business Units, the New/In Review/Actioned/Closed status lifecycle, and auto-routing to
HRBP/HSE are **Phase 2** (out of scope here).

## Decisions (locked)

1. Keep the entity named **`Ticket`** (no rename to Submission).
2. Replace `Ticket.Kind` (enum: Complaint/Suggestion/Feedback) with **`Ticket.TicketType`**
   (enum, 6 values). Remove the old `TicketKind` enum.
3. The name `TicketType` currently belongs to the category lookup entity → **rename that
   entity to `Category`** to free the name.
4. Add **`Ticket.Title`** (short title, required on the new forms).
5. **Base + per-type detail tables** (1:1). Five detail tables; **System Request has no
   detail** (just Title + Description).
6. Per-type category/option lists are modeled as **enums inside the detail models**.
7. New **5-step create wizard**: Reporter → Pick Type (6 panels) → Type Details → Attachments → Review.
8. Dev data is disposable → **squash migrations, reset + reseed**.

## Enums

```csharp
enum TicketType { Issue, Idea, ProjectProposal, SafetyConcern, Feedback, SystemRequest }

enum IssueImpactArea   { Production, Quality, SLA, Cost, Delivery }
enum SolutionCategory  { Operational, People, Process, Material, Equipment }
enum IdeaScope         { Functional, Departmental, BU }
enum ImpactType        { Improve, Save, Reduce }
enum RoiReach          { BU, MultiBU, Corporate }
enum RiskLevel         { Low, Medium, High, Critical }
enum SafetyHazard      { UnsafeAct, UnsafeCondition, Environmental, Health }
enum FeedbackCategory  { SupervisorBehavior, Workload, Communication, Facilities, Policy, HRProcess }
enum PreferredResolution { Personal, Departmental, Organizational }
enum Confidentiality   { Public, Private, Anonymous }
enum FeedbackActionType { ActionRequired, FeedbackOnly, AnonymousShare }
```

## Entity changes

**`Ticket`** (base, existing) — add:
- `TicketType TicketType` (replaces `Kind`)
- `string Title`
- Keep existing: RefNo, Desc (main description), Reporter, Status, password, Attachments, etc.

**`Category`** (renamed from `TicketType` lookup) — class/table/DbSet/controller/views/FK renamed.
Retained for admin use; the new per-type forms use enum options instead. `Ticket.CategoryId`
(renamed from `TicketTypeId`) stays optional.

## Per-type detail tables (1:1, PK = TicketId)

| Model | Fields |
|---|---|
| **IssueDetail** | ImpactArea `IssueImpactArea` · RootCause `string?` · ProposedSolution `string?` · SolutionCategory `SolutionCategory?` · ResponsibleDepartmentId `int?` |
| **IdeaDetail** | Scope `IdeaScope` · ImpactType `ImpactType` · ImpactDescription `string?` · Sustainability `string?` · ResponsibleDepartmentId `int?` |
| **ProjectProposalDetail** | BusinessNeed `string?` · EstimatedCost `decimal?` · Scope `string?` · ExpectedBenefits `string?` · RoiReach `RoiReach` · KeyStakeholders `string?` (multi, CSV) · TimelineStart `DateTime?` · TimelineEnd `DateTime?` |
| **SafetyDetail** | Hazard `SafetyHazard` · PotentialImpact `string?` · RiskLevel `RiskLevel` · SuggestedAction `string?` · ResponsibleDepartmentId `int?` |
| **FeedbackDetail** | Category `FeedbackCategory` · ImpactArea `string?` · PreferredResolution `PreferredResolution` · Confidentiality `Confidentiality` · ActionType `FeedbackActionType` · ResponsibleDepartmentId `int?` |
| **SystemRequest** | *(none — Title + Description only)* |

Each detail has a required 1:1 back-reference to `Ticket` (cascade delete).

## Create wizard (TicketController.New state machine)

| Step | Screen |
|---|---|
| 1 | Reporter (employee/anonymous choice + details) — existing |
| 2 | **Pick Ticket Type** — 6 clickable panels, sets `TicketType` |
| 3 | **Type Details** — renders the detail partial for the chosen type (System Request = just Description) |
| 4 | Attachments — existing |
| 5 | Review — existing, now shows type + detail summary |

`NewTicketViewModel` gains: `TicketType`, `Title`, and nested detail view models
(`Issue`, `Idea`, `ProjectProposal`, `Safety`, `Feedback`). Step 3 renders/binds only the
one matching the chosen type. On submit, the matching detail entity is created and linked.

## Validation

- Title required (all types). Description required (all types).
- Per-type required fields: the enum selectors (ImpactArea, Scope+ImpactType, RoiReach,
  Hazard+RiskLevel, Category+PreferredResolution+Confidentiality+ActionType). Text fields optional.

## Migration & seed

- Squash to a fresh `InitialCreate` (dev DB reset).
- Update `DefaultData` seeder: sample tickets across the 6 types with their detail rows;
  rename seeded `TicketType` rows → `Category`.

## Out of scope (Phase 2)

Business Units, status lifecycle (New/In Review/Actioned/Closed), Urgency, roles
(HRBP/HSE) + auto-routing, retention dates.
