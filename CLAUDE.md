# CLAUDE.md — ShareIT

Guidance and context for Claude when working in this repository.

## What ShareIT is (in one paragraph)

ShareIT is **Elsewedy Electric's internal "employee voice" web portal**. Employees use it to
raise six kinds of submissions — **Issue, Idea, Project Proposal, Safety Concern, Feedback, and
System Request** — either with their name or anonymously. Each submission becomes a tracked
**ticket** with a reference number the employee can use to follow its status. Behind the scenes,
staff (HR / HSE / admins) review, respond to, and close tickets, and management sees dashboards of
what's coming in. It evolved from an older complaints‑only tool ("SpeakUp") into a broader
employee‑engagement platform.

## Tech stack (for orientation only)

- ASP.NET Core MVC, **.NET 8**, Entity Framework Core (SQL Server / LocalDB), ASP.NET Identity.
- Bootstrap 5 UI. Multi‑step "create ticket" wizard (`Views/Ticket/New.cshtml` + partials).
- Key folders: `Controllers/`, `Models/` (+ `Models/ViewModel/`), `Views/`, `Data/` (DbContext +
  `Migrations/`), `Seeds/` (demo data), `Constant/` (roles/permissions).
- Setup: `dotnet user-secrets set "Email:Password" "…"` → `dotnet ef database update` → `dotnet run`.
  Default admin: `admin@shareit.com` / `P@ssword123`.

## Working notes for Claude

- **Multi‑step forms:** validate **per step** in the controller; never `[Required]` the whole wizard
  view model. Any value the controller both reads from a form *and* writes back needs
  `ModelState.Remove("…")` so the hidden field re‑renders correctly. (See `docs/BUG-REPORT.md`.)
- **Auth:** `FollowTicketController` and the dashboard are `[Authorize]` (staff). `SearchController`
  (public case tracking) and the ticket‑creation wizard are public. Link accordingly.
- **Types:** the 6 submission types are the `TicketType` enum; the `Category` table is a lookup
  seeded with the same 6 names. Old case types (Harassment, etc.) were removed.
- **Build gotcha:** stop the app (Shift+F5) before rebuilding in Visual Studio, or the DLL lock
  makes rebuilds silently no‑op. When verifying, run `dotnet run` and hard‑refresh the browser.
- Brand color is Elsewedy red `#c4252a` on near‑black `#302e2d`.

---

## 📊 Prompt: generate a business‑focused PowerPoint

> Copy everything in the block below into a new Claude conversation to generate the deck.
> (You can also say "use the artifact‑design and dataviz skills" if presenting charts.)

```
You are creating a PowerPoint‑style presentation for ShareIT, Elsewedy Electric's internal
"employee voice" portal. The audience is NON‑TECHNICAL leadership and staff (HR, OD, management).

GOAL: sell the business value and future prospects of ShareIT. This is a business pitch, not a
technical walkthrough. Keep technical detail to an absolute minimum — no code, no database talk,
no framework names. Speak in outcomes, people, and value.

DESIGN / THEME (Elsewedy Electric brand):
- Primary color: Elsewedy red #C4252A. Secondary: near‑black #302E2D. Background: white / light grey.
- Clean, corporate, confident. Generous whitespace, large readable headings, one idea per slide.
- Use simple icons and 2–4 word bullets. Prefer a chart or a big number over a paragraph.
- Optional tagline to echo the brand: "Belong & Grow."
- Output as a self‑contained HTML slide deck (16:9), each slide a full screen section, arrow/scroll
  navigable. Make it presentation‑ready and printable.

SLIDES (roughly 10–12):
1. Title — "ShareIT — Giving Every Employee a Voice" + Elsewedy branding.
2. The problem — employees have concerns, ideas, and feedback with nowhere structured to put them;
   good ideas and early risks get lost.
3. The solution — one trusted channel; submit in a minute, anonymously if preferred; every
   submission is tracked and answered.
4. What people can share — the six types as friendly cards: Issue, Idea, Project Proposal,
   Safety Concern, Feedback, System Request (one line of value each).
5. How it feels for an employee — 3 simple steps: choose → describe → track by reference number.
   Emphasize confidentiality and follow‑up.
6. For the business / leadership — live dashboards: submissions by business unit, by type, top
   recurring issues, resolution status. (Show illustrative charts with placeholder numbers.)
7. Business value — bullets: safer workplace, faster risk detection, surfaced cost‑saving ideas,
   higher engagement & trust, an auditable trail. Pair each with a benefit, not a feature.
8. Prospects / roadmap — where it grows next: routing straight to HR/HSE owners per business unit,
   mobile‑friendly access, recognition for adopted ideas, analytics on trends over time.
9. Why now — ties to Elsewedy's "Belong & Grow" people commitment and compliance culture.
10. Rollout & ask — simple phased rollout across business units; the ask from leadership
    (sponsorship, communication, encouraging participation).
11. Closing — a strong one‑liner + "Thank You" in brand style.

TONE: warm, confident, human. Every slide should answer "why does this matter to us?"
Use realistic but clearly illustrative figures for any charts, and label them as examples.
```

---

*Related docs:* `docs/BUG-REPORT.md` (bugs + schema changes),
`docs/ShareIT-explained-simply.md` (plain‑English overview),
`docs/superpowers/specs/` (design specs).
