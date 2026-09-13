# English / Arabic language toggle

**Date:** 2026-08-09
**Status:** Approved, implementing

## Goal

A visitor can switch the employee-facing portal between English and Arabic from a navbar
toggle. The choice persists across sessions. Arabic renders as a proper right-to-left layout,
not English layout with Arabic words in it.

## Decisions

- **Scope: employee-facing pages only.** The admin panel stays English. An admin who switches
  to Arabic gets an Arabic public site and an English admin panel — a deliberate trade of this
  scope, not a defect.
- **Full RTL mirroring**, not Arabic-text-in-LTR.
- **Claude drafts the Arabic**; a human reviews before it is treated as final corporate copy.
- **One shared resource file**, not ASP.NET Core's default per-view `.resx` layout. Fifteen
  views would mean fifteen files to hand a translator; one file is reviewable in one sitting.
- **The English string is the resource key.** `@L["Track Case"]` returns `"Track Case"` when
  no Arabic entry exists. Missing translations degrade to English instead of throwing, and the
  neutral `SharedResource.resx` can stay empty.

## Measured scope

| | |
|---|---|
| Files in scope | 15 |
| Lines in those files | 7,393 |
| Visible text strings | ~144 |
| `placeholder` / `title` / `alt` strings | ~20 |
| Directional CSS declarations to convert | 57 |

`wwwroot/lib/bootstrap/dist/css/bootstrap.rtl.min.css` already ships in the project — no
download or CDN needed.

## In scope

`Views/Shared/_Layout.cshtml`, `_LoginPartial.cshtml`, `_Layout.cshtml.css`,
the six wizard partials (`_TypeStep`, `_ReporterStep`, `_DetailsStep`, `_AttachmentStep`,
`_ReviewStep`, `_TicketStep`), `Views/Home/Index.cshtml`, `Guid.cshtml`, `Privacy.cshtml`,
`Views/Ticket/New.cshtml`, `Confirmation.cshtml`, `Views/Search/Index.cshtml`,
`Areas/Identity/Pages/Account/Login.cshtml`.

## Out of scope

The admin panel in full (`_Admin.cshtml`, Dashboard, Videos, Documents, Users, Roles,
Companies, Departments, FollowTicket), `Ticket/CreateFromEmail`, outbound emails, and all
database content (ticket text, video titles, document and department names). Database content
cannot be localized by resource files — it is whatever language the author typed.

## Deferred (YAGNI)

Per-user language preference stored on the user record (the cookie is enough), a third
language, translated email templates, and localized admin pages.

---

# Implementation, file by file

## Step 1 — `SharedResource.cs` (new, project root)

The marker type that `IStringLocalizer<T>` binds to. Empty by design.

```csharp
namespace ShareIT
{
    /// <summary>
    /// Marker class for the single shared resource file. Views and controllers inject
    /// IStringLocalizer&lt;SharedResource&gt;, which resolves to Resources/SharedResource.*.resx.
    /// </summary>
    public class SharedResource
    {
    }
}
```

## Step 2 — `Resources/SharedResource.resx` and `Resources/SharedResource.ar.resx` (new)

Standard `.resx` XML. The neutral file carries the schema header and no `<data>` entries —
English comes from the keys themselves. The `.ar.resx` holds one `<data>` per English string.

SDK-style projects glob `**/*.resx` as `EmbeddedResource` automatically, so `ShareIT.csproj`
needs no change.

## Step 3 — `Controllers/CultureController.cs` (new)

```csharp
[HttpGet]
public IActionResult Set(string culture, string returnUrl)
{
    if (culture is "en" or "ar")
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });
    }

    return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "~/" : returnUrl);
}
```

`LocalRedirect` rejects absolute URLs, so the `returnUrl` round-trip cannot be used as an open
redirect. `IsEssential = true` keeps the cookie working under a consent policy.

## Step 4 — `Program.cs`

Services, before `builder.Build()`:

```csharp
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(SharedResource)));

var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ar") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
```

`AddDataAnnotationsLocalization` is pointed at `SharedResource` so `[Display(Name = "Title")]`
and validation messages resolve from the same single file rather than per-model resources.

Pipeline — `UseRequestLocalization` must run **before** `UseRouting` so the culture is set
before any view executes:

```csharp
app.UseRequestLocalization(app.Services
    .GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
```

## Step 5 — `Views/_ViewImports.cshtml`

Inject the localizer once, globally, instead of fifteen times:

```razor
@using Microsoft.AspNetCore.Mvc.Localization
@using System.Globalization
@inject IHtmlLocalizer<SharedResource> L
```

**Must be `IHtmlLocalizer<SharedResource>`, not `IViewLocalizer`.** `IViewLocalizer` resolves
*per-view* resource files (`Resources/Views/Shared/_Layout.ar.resx`) and will silently return
the English key when pointed at a shared file — the page renders, `dir="rtl"` is correct, and
every string stays English. Verified during implementation: this exact mistake produced an
RTL page with English text and no error anywhere.

Every view in scope can then use `@L["..."]` with no per-file plumbing. Views left out of
scope simply never call it.

## Step 6 — `Views/Shared/_Layout.cshtml`

Three changes.

**Direction and language on `<html>`:**

```razor
@{
    var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
    var isRtl = culture == "ar";
}
<!DOCTYPE html>
<html lang="@culture" dir="@(isRtl ? "rtl" : "ltr")">
```

**Bootstrap stylesheet swap:**

```razor
<link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap@(isRtl ? ".rtl" : "").min.css" />
```

**The toggle**, in the navbar beside the Hello/Admin link — a dropdown offering `English` and
`العربية`, linking to `/Culture/Set?culture=…&returnUrl=@Context.Request.Path`.

Then wrap the four nav labels (Home, New Report, Track Case, User Guide) in `@L[...]`.

## Step 7 — RTL CSS

57 directional declarations across the in-scope files. Convert physical properties to logical
ones so a single stylesheet serves both directions:

| Physical | Logical |
|---|---|
| `margin-left` / `margin-right` | `margin-inline-start` / `margin-inline-end` |
| `padding-left` / `padding-right` | `padding-inline-start` / `padding-inline-end` |
| `text-align: left` / `right` | `text-align: start` / `end` |
| `left:` / `right:` (positioning) | `inset-inline-start:` / `inset-inline-end:` |
| `border-left` / `border-right` | `border-inline-start` / `border-inline-end` |

Logical properties are supported in all current browsers and need no `[dir]` selector.

Leave genuinely symmetric values alone — `margin: 0 auto`, `text-align: center`, and
`transform: translate(-50%, -50%)` on centred elements are direction-neutral. Icon rotations
that imply direction (a "back" arrow) do need flipping under `[dir="rtl"]`.

Add an Arabic-capable font stack:

```css
[dir="rtl"] body {
    font-family: 'Segoe UI', Tahoma, 'Traditional Arabic', sans-serif;
}
```

## Step 8 — Remaining views

Wrap user-visible text in `@L[...]` across `_LoginPartial`, the six wizard partials,
`Home/Index`, `Home/Guid`, `Home/Privacy`, `Ticket/New`, `Ticket/Confirmation`,
`Search/Index`, and `Account/Login`. Attributes take the same treatment:
`placeholder="@L["Search by reference number"]"`.

Do not wrap: CSS class names, `asp-*` tag helper values, JavaScript identifiers, or the
brand name "ShareIT".

## Step 9 — Enum display names

`TicketType` in `Models/TicketEnums.cs` uses `[Display(Name = "Project Proposal")]`. With
`AddDataAnnotationsLocalization` pointed at `SharedResource`, adding `Project Proposal` as a
key to `SharedResource.ar.resx` localizes it wherever the wizard renders it — no model change.

`VideoSection` display names are admin-only and stay English.

---

# Verification

1. Load `/` — English, `dir="ltr"`, `bootstrap.min.css`.
2. Switch to العربية — Arabic text, `dir="rtl"`, `bootstrap.rtl.min.css`, navbar mirrored.
3. Reload — Arabic persists (cookie).
4. Restart the browser — Arabic still persists (one-year expiry).
5. Switch back to English — everything reverts.
6. Walk the ticket wizard in Arabic: type → reporter → details → attachment → review → submit.
7. Confirm the reference number and confirmation page render in Arabic.
8. Visit `/Videos` as admin while Arabic is active — the admin panel is still English and
   still LTR. Expected, per scope.
9. Confirm no string renders as a raw key or an empty span anywhere in scope.

---

# Status as implemented (2026-08-09)

**Done and verified at runtime:** infrastructure (all four new files, `Program.cs`,
`_ViewImports`), the navbar toggle, `dir`/`lang` switching, Bootstrap RTL swap, cookie
persistence, and 93 wrapped strings across the in-scope views. `/`, `/Home/Guid`, `/Search`
and `/Ticket/New` all return 200 in both languages with the correct direction. The admin
panel is untouched, as scoped. `HomeController.Guid()` localizes the three User Guide section
headings via `IStringLocalizer<SharedResource>`, since those come from the controller rather
than the view.

**Remaining, deliberately not finished in this pass:**

- **66 bare `>Text<` strings** still English in the in-scope views — the long tail (mostly
  `Views/Search/Index.cshtml` and the wizard partials). Same mechanical treatment; the
  resource file and the pattern are established.
- **18 `placeholder` / `title` / `alt` attributes** not yet wrapped.
- **`Areas/Identity/Pages/Account/Login.cshtml` and `Views/Home/Privacy.cshtml`** not
  converted. Note Razor Pages under `Areas/Identity` do not inherit `Views/_ViewImports.cshtml`
  — they need their own `@inject IHtmlLocalizer<SharedResource> L`.
- **21 bare `left:` / `right:` CSS declarations** left physical, in `_Layout.cshtml` (1),
  `Home/Index` (3), `Home/Guid` (4), `Search/Index` (7), `Ticket/New` (6). These are
  absolute-positioning values where blind conversion to `inset-inline-*` can move decorative
  elements to the wrong corner. They need a visual check in the browser under RTL, one at a
  time, not a scripted pass.
- **The Arabic is a Claude first draft** and has not been reviewed by a native or professional
  translator.
