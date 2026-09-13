# ShareIT — What We Changed, Why, and How

*Written 9 August 2026. Covers four pieces of work: admin-managed tutorial videos, separating
the admin dashboard from the employee site, an English/Arabic language toggle, and a branded
account page — plus the bugs found along the way.*

This document is written to be read by anyone, technical or not. Jargon is explained the first
time it appears. If you only read one section, read **"How to spot these problems yourself"**
near the end — that's the transferable part.

---

## Table of contents

1. [The four jobs, in one paragraph each](#the-four-jobs)
2. [Part 1 — Admin-managed tutorial videos](#part-1--admin-managed-tutorial-videos)
3. [Part 2 — Separating the admin dashboard from the employee site](#part-2--separating-the-admin-dashboard)
4. [Part 3 — The broken navbar](#part-3--the-broken-navbar)
5. [Part 4 — English / Arabic language toggle](#part-4--english--arabic-language-toggle)
6. [Part 5 — Branded account page](#part-5--branded-account-page)
7. [How to spot these problems yourself](#how-to-spot-these-problems-yourself)
8. [Every file we added and changed](#every-file-we-added-and-changed)
9. [How to test all of it](#how-to-test-all-of-it)
10. [Known limitations and what's left](#known-limitations-and-whats-left)

---

## The four jobs

**1. Tutorial videos.** The admin could upload a video, but it vanished into nowhere — the
public User Guide page was hand-written HTML that never looked at the database. We connected
the two, so uploading a video now makes a card appear on the User Guide automatically.

**2. Admin dashboard separation.** The dashboard was rendering inside the *employee* interface,
and a "Dashboard" tab was showing in the employee navigation bar. Now admins land in the admin
panel when they log in, and the employee navbar has no admin links.

**3. Language toggle.** The whole employee-facing site can now switch between English and
Arabic, including proper right-to-left layout. The admin panel deliberately stays English.

**4. Account page.** The admin's account page was using Microsoft's stock, unstyled design.
It now matches the rest of the admin panel.

---

## Part 1 — Admin-managed tutorial videos

### The problem, in plain terms

Imagine a **poster on a wall** showing pictures of videos, drawn by hand with markers. Now
imagine a **box in a storage room** where the admin puts uploaded videos.

The poster had no idea the box existed.

That was the situation. Uploading a video saved it to disk and to the database — and the User
Guide page kept showing the same hand-written cards forever.

### The four separate breaks

This wasn't one bug. It was four things that were never connected in the first place.

**Break 1 — The page never read the database.**
`Controllers/HomeController.cs` had:

```csharp
public async Task<IActionResult> Guid()
{
    return View();      // <- no data. At all.
}
```

And `Views/Home/Guid.cshtml` had ~230 lines of hardcoded HTML — 10 tutorial cards typed out by
hand, with fixed titles and descriptions.

**Break 2 — The two halves pointed at different folders.**
Uploads were saved to `/uploads/videos/`. The page's JavaScript looked in `/videos/`:

```javascript
const videoFiles = {
    'file-new-case': '/videos/file-new-case.mp4',   // <- wrong folder
    ...
};
```

Even if a card had existed, clicking play would have found nothing.

**Break 3 — The category dropdown threw away the answer.**
This one is subtle and worth understanding. `Views/Videos/Create.cshtml` had ten options:

```html
<option value="NewCase">File New Case</option>
<option value="NewCase">Document Upload</option>          <- same value!
<option value="NewCase">Confidential Reporting</option>   <- same value!
```

An HTML dropdown sends the **value**, not the label you see. So picking "Document Upload"
saved the text `"NewCase"` to the database. Which specific video it was got destroyed at the
moment of saving and could never be recovered.

**Break 4 — The data model couldn't describe a card.**
The `Video` record had `Section`, `VideoPath`, `FileType`, `FileName`, `FileSize`, `Duration`.
No **Title**. No **Description**. No **Thumbnail**. Even after connecting everything, there'd
have been nothing to write on the card.

**Two more found while looking:**

- **The Edit button did nothing.** `Views/Videos/Edit.cshtml` submitted the form to an action
  called `Edit`, but no "save" version of that action existed — only the "show me the form"
  version. Because of how ASP.NET matches requests, the form submission quietly re-displayed
  the form and saved nothing. It *looked* like it worked.
- **Delete could leave orphans.** The delete code only removed the database row if the video
  also had a file path. A row without a file could never be deleted.

### How we fixed it

| Step | File | Change |
|---|---|---|
| 1 | `Models/VideoEnums.cs` **(new)** | A `VideoSection` list with exactly 3 real values |
| 2 | `Models/Video.cs` | `Section` became a proper category; added `Title`, `Description`, `ThumbnailPath` |
| 3 | Migration | Changed the database column from text to a number (see below) |
| 4 | `Controllers/VideosController.cs` | Two shared helpers; thumbnail upload; **added the missing save-Edit action**; fixed delete |
| 5–7 | `Views/Videos/*.cshtml` | Real dropdown; Title/Description/thumbnail fields; Title column in the list |
| 8 | `Models/ViewModel/UserGuideViewModel.cs` **(new)** | The shape the User Guide page needs |
| 9 | `Controllers/HomeController.cs` | `Guid()` now actually queries the database |
| 10 | `Views/Home/Guid.cshtml` | ~230 hardcoded lines replaced with a loop |

**The dropdown fix** is worth showing, because it's the whole of Break 3:

```html
<select asp-for="Section" asp-items="Html.GetEnumSelectList<VideoSection>()"></select>
```

That one line generates exactly three options with three *different* values. The bug becomes
impossible rather than fixed.

**The play-button fix** is the whole of Break 2:

```html
<a class="btn-tutorial" data-video-path="@v.VideoPath">
```

The file's real location now travels *with the card*, straight from the database. There's no
hand-maintained list to fall out of sync. The JavaScript lookup table was deleted entirely.

### The migration problem (and what a migration is)

A **migration** is a script that changes the database's *structure* — adding a column, changing
a column's type. Entity Framework writes them for you.

We changed `Section` from text (`"ShareITVideos"`) to a number (`2`). One video already existed
in the database, and SQL Server refused:

> `Conversion failed when converting the nvarchar value 'ShareITVideos' to data type int.`

It won't guess that `ShareITVideos` means `2`. **We had to tell it.** So instead of changing the
column in place, we did a four-step swap:

1. Add a new number column beside the old text one
2. Copy the values across, *translating* them (`'ShareITVideos'` → `2`)
3. Delete the old text column
4. Rename the new one into its place

```csharp
migrationBuilder.Sql(@"
    UPDATE Videos SET SectionTmp =
        CASE Section
            WHEN 'NewCase'       THEN 0
            WHEN 'TrackCase'     THEN 1
            WHEN 'ShareITVideos' THEN 2
            ELSE 0
        END;");
```

**A trap worth remembering:** before this could run at all, the *build* had to succeed —
`dotnet ef migrations add` compiles the project first. The code still had `Section` being
compared to text in `VideosController.Index`, so the build failed and **no migration file was
created at all**. The error looked like a migration problem but was actually a compile problem
two steps earlier.

**Result, verified:** the existing row converted to `Section = 2` with its file link intact.

---

## Part 2 — Separating the admin dashboard

### The problem

Three separate causes, and only one is the obvious one.

**Cause 1 — The dashboard never chose a layout.**
A "layout" is the frame around a page — navbar, sidebar, footer. `Views/Home/Dashboard.cshtml`
simply didn't say which frame to use, so it fell back to the default in
`Views/_ViewStart.cshtml` — which is the **employee** layout.

Every other admin page said so explicitly (`Layout = "~/Views/Shared/_Admin.cshtml";`).
Dashboard was the one that forgot. **One missing line caused the main symptom.**

**Cause 2 — A Dashboard tab was hardcoded into the employee navbar.**
`Views/Shared/_Layout.cshtml` had a block that showed a "Dashboard" link to admins — pulling
them back into the employee interface, even though the admin sidebar already had its own.

**Cause 3 — The login redirect ignored roles.**
`Login.cshtml.cs` sent *everyone* to `/Home/Dashboard` after login. But the dashboard only
allows Admin and SuperAdmin — so a Manager or Engineer logging in got an access-denied error.
Admins landed correctly by accident, in the wrong frame.

### How we fixed it

- **`Views/Home/Dashboard.cshtml`** — added the one missing layout line.
- **`Views/Shared/_Layout.cshtml`** — deleted the Dashboard tab block.
- **`Views/Shared/_LoginPartial.cshtml`** — the "Hello *name*!" link now checks your role:
  admins go to the admin panel, everyone else to their account page.
- **`Login.cshtml.cs`** — the redirect decision moved to *after* login succeeds, where the role
  is actually known:

```csharp
if (!hasExplicitReturnUrl && await IsAdminAsync(Input.Email))
{
    return LocalRedirect(Url.Content("~/Home/Dashboard"));
}
return LocalRedirect(returnUrl);
```

That `hasExplicitReturnUrl` flag matters. If an admin clicks a protected link and gets sent to
the login page, they should land on **the page they wanted** — not get yanked to the dashboard.

**Verified with real logins:**

| Situation | Result |
|---|---|
| Admin logs in normally | → admin panel dashboard |
| Admin logs in from a protected page | → that page, not the dashboard |
| Basic user logs in | → public home page (previously: access denied) |

---

## Part 3 — The broken navbar

This one is short but it's the best lesson in the whole document.

### The symptom

Stray text — `ss="nav-item">` — appearing on the page for both admins and employees.

### The cause

When the Dashboard tab block was deleted from `Views/Shared/_Layout.cshtml`, the deletion ran
**seven characters too far** and ate the start of the next line:

```
Intended to delete:   [the Dashboard block]
Actually deleted:     [the Dashboard block] + "<li cla"

Left behind:          ss="nav-item">
```

`<li class="nav-item">` became `ss="nav-item">`. Text stranded outside any HTML tag gets
displayed literally by the browser — so you saw it on screen.

Knock-on effect: the "New Report" item lost its opening tag, leaving four closing tags against
three opening ones. Malformed enough that the navbar styling broke for everyone.

### The lesson

**Deleting a block of code is riskier than it looks.** Always check the line immediately
*before* and *after* what you deleted. A one-line fix restored it.

### A second lesson, from the same bug

While verifying the fix, the check reported *5 remaining stray fragments* — alarming, and
wrong. The search pattern was `ss="nav-item"`, and the word `class="nav-item"` **contains**
that text (cla-**ss="nav-item"**). Every correct tag was being counted as a defect.

**If a check reports something surprising, suspect the check before you suspect the code.**

---

## Part 4 — English / Arabic language toggle

### The two words your mentor used

- **Globalization** = making the code *capable* of another language. No hardcoded English,
  layout that can flip direction. You do this once.
- **Localization** = supplying the actual translations for one language. You do this per language.

### How it works, mechanically

1. **A resource file** (`.resx`) — an XML dictionary mapping each English phrase to Arabic.
2. **Registration in `Program.cs`** — tells the app that English and Arabic exist.
3. **A cookie** — remembers your choice. Set by clicking the toggle; read on every request.
4. **In pages** — `@L["Track Case"]` instead of the literal words.

### The design decision that saved a lot of pain

**The English text *is* the lookup key.** `@L["Track Case"]` returns `"Track Case"` if no Arabic
exists. So a missing translation shows English instead of crashing or showing a blank — and
partial progress is always safe to ship.

### The bug that cost the most time

The first version produced **a perfectly right-to-left Arabic page with entirely English text,
and no error anywhere.**

The cause: ASP.NET has two similar-looking tools.

| Tool | Looks for |
|---|---|
| `IViewLocalizer` | A **separate file per page** (`Resources/Views/Shared/_Layout.ar.resx`) |
| `IHtmlLocalizer<SharedResource>` | The **one shared file** (`Resources/SharedResource.ar.resx`) |

We used the first while storing translations in the second. It found nothing — and because a
missing translation falls back to English *by design*, it failed **completely silently**.

**This is the single most important thing in this document to remember.** A silent failure with
no error message is far harder to diagnose than a crash. If translations aren't appearing and
everything else looks right, check which localizer is injected in
`Views/_ViewImports.cshtml`.

### The measurement mistake

Partway through, the work was reported as complete — "0 strings remaining." That was wrong.

The check used this pattern to find untranslated text:

```
>[A-Z][a-zA-Z]{2,}[a-zA-Z ]{2,}<
```

Look closely: the allowed characters are **letters and spaces only**. No comma. No apostrophe.
So `"Yes, I'm an Employee"` — with both — could never match. The check was blind to exactly the
kind of sentence a real user reads.

Re-running with a permissive pattern found **160 untranslated strings**, not zero.

**Lesson: a passing test proves your test passed. It doesn't prove your code works.** If a
measurement says "perfect," be more suspicious than if it says "80%."

### Right-to-left layout

Arabic reads right to left, and that's a *layout* problem, not a translation problem.

- Bootstrap ships a ready-made RTL stylesheet, already in the project. The page loads
  `bootstrap.rtl.min.css` when Arabic is active.
- `<html dir="rtl">` tells the browser to mirror everything.
- Custom CSS with `margin-left` etc. does **not** flip automatically. We converted 22 of these
  to *logical properties* — `margin-inline-start` means "the side text starts from," which
  flips by itself.

### Final state

| | |
|---|---|
| Translation calls in pages | **306** |
| Arabic entries in the resource file | **287** |
| Untranslated static text remaining | **0** |

Everything still in English is one of three things: **database content** (department names,
video titles an admin typed), **brand names** (ShareIT, ELSEWEDY ELECTRIC), or **live chat
message data**.

### The limit nobody should be surprised by

**A resource file can translate what *we* wrote. It cannot translate what *users* wrote.**

If an admin types "How to submit feedback" as a video title, no translation file will ever turn
that into Arabic. Translating those needs database changes — either a second column per language
or a separate translations table — plus an admin screen for entering both. That was deliberately
left out of scope.

---

## Part 5 — Branded account page

### The problem

The admin's account page looked nothing like the rest of the admin panel.

`Areas/Identity/Pages/_ViewStart.cshtml` *already* pointed at the admin layout — so the page
rendered inside the correct frame. But the page **content** came from a Microsoft package
compiled into the application. You can't restyle content that isn't in your project; you have to
create your own copy.

### How we fixed it

Created local, branded versions:

- `Manage/_Layout.cshtml` — page header, status messages, sidebar-nav + content columns
- `Manage/Index.cshtml` + `.cs` — Profile (username, phone number)
- `Manage/ChangePassword.cshtml` + `.cs` — current / new / confirm password
- `_ManageNav.cshtml` — rebuilt with brand-red highlights and icons

### The thing that was almost missed

Because the "Hello *name*!" link now sends admins to the **dashboard**, the account page had
**no link pointing to it at all.** It would have been beautifully branded and completely
unreachable.

Fixed by adding a **My Account** item to the admin sidebar, under *System*.

**Lesson: after changing where a link goes, check what that link used to be the only route to.**

---

## How to spot these problems yourself

The five most useful habits from this whole session:

### 1. When two things don't connect, check whether they were *ever* connected

Before assuming something broke, check whether it ever worked. The User Guide page never read
the database — not "stopped reading it." That changes the fix from *repair* to *build*.

**How to check:** find the controller action behind the page. If it says `return View();` with
no data, the page cannot be showing live information, no matter what it looks like.

### 2. Read the error message literally, and check what ran *before* it

"There's a problem with the migration because a row exists" was actually a **compile error** —
the migration was never created. `dotnet ef` builds first; a failed build means no migration.

**How to check:** run `dotnet build` on its own. If it fails, fix that first; everything
downstream is noise.

### 3. Suspect your measurement before your code

Twice in this session a check gave a confidently wrong answer:

- `class="nav-item"` matched a search for `ss="nav-item"` → false alarm
- A pattern without commas or apostrophes reported "0 untranslated" → 160 were left

**How to check:** before trusting a search that returns zero results, run it against something
you *know* should match. If it doesn't find that, your pattern is broken.

### 4. Silent failures are worse than crashes

The `IViewLocalizer` mistake produced a page that loaded fine, looked right, and was simply
wrong. Nothing logged an error.

**How to check:** when something "doesn't work" but nothing errors, look for a fallback. Code
that degrades gracefully hides its own failures.

### 5. Verify by running it, not by reading it

Everything in this document was checked against a running application — real logins, real HTTP
requests, real database queries. Reading code tells you what you *think* it does.

**How to check:** `dotnet run`, then actually load the page. For database changes, query the
table afterwards and look at the row.

### A practical note for this project

Per `CLAUDE.md`: **stop the app before rebuilding.** If Visual Studio or IIS Express is running,
the build fails with a file-lock error that looks alarming but has nothing to do with your code:

```
error MSB3021: Unable to copy file ... because it is being used by another process
```

Press **Shift+F5** in Visual Studio first.

---

## Every file we added and changed

### Added (17 files)

**Tutorial videos**
```
Models/VideoEnums.cs                                    the 3 User Guide sections
Models/ViewModel/UserGuideViewModel.cs                  shape for the User Guide page
Data/Migrations/20260809085556_AddVideoCardFields.cs    the database change
Data/Migrations/20260809085556_AddVideoCardFields.Designer.cs
wwwroot/images/tutorials/placeholder.svg                fallback thumbnail
```

**Language toggle**
```
SharedResource.cs                     marker class the translation system binds to
Resources/SharedResource.resx         English (intentionally empty — keys are the English text)
Resources/SharedResource.ar.resx      287 Arabic translations
Controllers/CultureController.cs      handles the toggle, writes the language cookie
```

**Account page**
```
Areas/Identity/Pages/Account/Manage/_Layout.cshtml
Areas/Identity/Pages/Account/Manage/_ViewStart.cshtml
Areas/Identity/Pages/Account/Manage/Index.cshtml
Areas/Identity/Pages/Account/Manage/Index.cshtml.cs
Areas/Identity/Pages/Account/Manage/ChangePassword.cshtml
Areas/Identity/Pages/Account/Manage/ChangePassword.cshtml.cs
```

**Documentation**
```
docs/superpowers/specs/2026-08-06-admin-tutorial-videos-design.md
docs/superpowers/specs/2026-08-09-arabic-localization-design.md
```

### Changed

**Code**
```
Program.cs                                  registered localization + the language middleware
Controllers/VideosController.cs             upload helpers, missing Edit action, delete fix
Controllers/HomeController.cs               Guid() now queries the database; localized headings
Areas/Identity/Pages/Account/Login.cshtml.cs   role-aware redirect after login
Models/Video.cs                             Title, Description, ThumbnailPath; Section is now a real category
```

**Views — admin**
```
Views/Videos/Create.cshtml       fixed dropdown, added Title/Description/thumbnail
Views/Videos/Edit.cshtml         same fields, real dropdown instead of free text
Views/Videos/Index.cshtml        Title column, readable section names
Views/Home/Dashboard.cshtml      the one missing layout line
Views/Shared/_Admin.cshtml       added "My Account" to the sidebar
Areas/Identity/Pages/Account/Manage/_ManageNav.cshtml   branded nav
```

**Views — employee (also all translated)**
```
Views/_ViewImports.cshtml        injects the translator into every page
Views/Shared/_Layout.cshtml      language toggle, RTL support, removed Dashboard tab, navbar fix
Views/Shared/_LoginPartial.cshtml   role-aware "Hello" link
Views/Home/Guid.cshtml           230 hardcoded lines → a loop over the database
Views/Home/Index.cshtml
Views/Home/Privacy.cshtml
Views/Search/Index.cshtml
Views/Ticket/New.cshtml
Views/Ticket/Confirmation.cshtml
Views/Shared/_TypeStep.cshtml
Views/Shared/_ReporterStep.cshtml
Views/Shared/_DetailsStep.cshtml
Views/Shared/_AttachmentStep.cshtml
Views/Shared/_ReviewStep.cshtml
Views/Shared/_TicketStep.cshtml
Views/Shared/_Layout.cshtml.css
```

---

## How to test all of it

Stop the app first (**Shift+F5**), then `dotnet run`, then hard-refresh the browser.

### Tutorial videos
1. Log in as `admin@shareit.com`, go to **Tutorials**
2. Upload a video into **"How to File a New Case"** with a title, description and thumbnail
3. Open the **User Guide** — the card should appear under that exact heading
4. Click it — the video should play in the popup
5. Edit the video's title, save, reload the User Guide — the title should change
   *(this is the test for the Edit action that never existed)*
6. Upload one with **no** thumbnail — the placeholder should appear
7. Delete it — the card disappears and the file is removed from disk

### Admin separation
1. Log in as admin → should land on the **admin panel** dashboard, not the employee site
2. The employee navbar should have **no Dashboard tab**
3. Click "Hello *name*!" from the public site → back to the admin panel
4. Log in as `basicuser@domain.com` → should land on the **public home page**

### Language toggle
1. Click the globe icon → **العربية**
2. Text becomes Arabic and the layout mirrors right-to-left
3. Reload — Arabic persists. Close and reopen the browser — still Arabic
4. Walk the full ticket wizard in Arabic and submit
5. Switch back to English — everything reverts
6. Visit the admin panel while Arabic is active — **still English**, by design

### Account page
1. Admin sidebar → **My Account**
2. It should look like the rest of the admin panel — same red, same cards
3. Change the phone number, save — a green confirmation appears and the value persists
4. Switch to the **Password** tab — the nav highlight should follow

---

## Known limitations and what's left

### Deliberate — not bugs

- **The admin panel is English only.** Chosen scope: employees need Arabic, the handful of
  trained staff using the admin panel don't.
- **Database content is never translated.** Department names, company names, video titles.
  Explained in Part 4.
- **Video cards are ordered by upload date.** No manual ordering. Adding it later means one
  number column and one input box.

### Genuinely unfinished

- **The Arabic is an unreviewed first draft.** 287 entries, machine-written. For a compliance
  portal this matters — wording about anonymity and false reporting is a promise the company is
  making. **Have a native speaker review `Resources/SharedResource.ar.resx` before employees
  see it.**
- **21 positioning CSS rules still don't mirror.** In `_Layout`, `Home/Index`, `Home/Guid`,
  `Search/Index` and `Ticket/New`. These place things at an exact left/right position; flipping
  them without *looking* at the result can move a badge to the wrong corner. Needs one pass
  through the site in Arabic with human eyes.
- **The login and privacy pages aren't translated.** Note: pages under `Areas/Identity` do
  **not** inherit `Views/_ViewImports.cshtml`, so `Login.cshtml` needs its own
  `@inject IHtmlLocalizer<SharedResource> L` line before `@L[...]` will work there.
- **The Email tab was removed from the account page.** Changing an email address needs a
  confirmation flow; leaving the unstyled stock page linked would have looked worse than
  removing it.
- **Two-factor login ignores the admin redirect.** If two-factor is ever switched on, an admin
  completing it lands on the home page instead of the dashboard. `Login.cshtml.cs`, the
  `RequiresTwoFactor` branch.
