# Admin-managed tutorial videos → User Guide

**Date:** 2026-08-06
**Status:** Approved, ready to implement

## Goal

An admin uploads a tutorial video in the admin panel, picks which User Guide section it
belongs to, and the video appears automatically as a card in that section of the public
User Guide page (`/Home/Guid`). No code edits needed to add, change, or remove a tutorial.

## Why it doesn't work today

Four independent breaks between the two halves:

1. **The User Guide never reads the database.** `HomeController.Guid()` is `return View();`
   with no model. All 10 tutorial cards in `Views/Home/Guid.cshtml` are hardcoded HTML.
2. **Mismatched paths.** Uploads save to `/uploads/videos/{guid}.ext`; the page's JS
   dictionary (`Guid.cshtml:696-707`) points at `/videos/{name}.mp4`.
3. **The section dropdown discards the choice.** `Views/Videos/Create.cshtml:33-45` has 10
   `<option>` elements sharing 3 duplicated `value` attributes. A `<select>` posts the
   *value*, not the label, so "Document Upload" is stored as `"NewCase"`.
4. **`Video` can't describe a card.** No `Title`, no `Description`, no thumbnail.

Two extra defects found while surveying:

- **`[HttpPost] Edit` does not exist.** `Views/Videos/Edit.cshtml:27` posts to `Edit`, which
  matches the attribute-less GET action, so the form silently re-renders and saves nothing.
- **`Views/Videos/Edit.cshtml:39` uses a free-text `<input>` for Section**, letting an admin
  type a value that matches no section.

## Decisions

- **Categories = the 3 existing User Guide sections** (`NewCase`, `TrackCase`,
  `ShareITVideos`) as a C# enum. No new lookup table — the retired `Category` table is not
  coming back.
- **Hardcoded cards are removed entirely.** The page renders only from the database. Sections
  with no videos show an empty state.
- **All existing CSS, the video modal, the FAQ accordion, and the Quick Start block stay
  untouched.** Only the 10 card blocks become a loop.

## Deferred (YAGNI)

Manual card ordering (cards order by upload date), draft/publish toggle, auto-generated
thumbnails, video duration extraction. Add later if actually needed.

---

# Implementation, file by file

## Step 1 — `Models/VideoEnums.cs` (new file)

```csharp
using System.ComponentModel.DataAnnotations;

namespace ShareIT.Models
{
    /// <summary>
    /// The three sections of the public User Guide page. Display names are the
    /// headings rendered on that page.
    /// </summary>
    public enum VideoSection
    {
        [Display(Name = "How to File a New Case")] NewCase,
        [Display(Name = "How to Track Your Case")] TrackCase,
        [Display(Name = "ShareIT Videos")] ShareITVideos
    }
}
```

Same `[Display]` pattern as `TicketType` in `Models/TicketEnums.cs`, so the existing
`GetDisplayName()` extension (`Models/EnumExtensions.cs:13`) renders it everywhere.

## Step 2 — `Models/Video.cs`

Change `Section` from `string` to the enum, and add the three card fields.

```csharp
[Required(ErrorMessage = "Section is required")]
[Display(Name = "User Guide Section")]
public VideoSection Section { get; set; }          // was: string

[Required(ErrorMessage = "Title is required")]
[Display(Name = "Title")]
[StringLength(120)]
public string Title { get; set; }                  // new — the card heading

[Display(Name = "Description")]
[StringLength(400)]
public string? Description { get; set; }           // new — the card body text

[Display(Name = "Thumbnail Image")]
public string? ThumbnailPath { get; set; }         // new

[NotMapped]
[Display(Name = "Thumbnail Image")]
public IFormFile? ThumbnailFile { get; set; }      // new — upload only
```

Delete the `[StringLength(100)]` that was on `Section`; it's meaningless on an enum.

## Step 3 — Migration

The `Section` column is `nvarchar(100)` and must become `int`. **Check for existing rows
first:**

```
dotnet ef migrations add AddVideoCardFields
```

Then open the generated migration. If `dbo.Videos` is empty (likely — the uploader has had
nowhere useful to save to), the scaffolded `AlterColumn` is fine as-is.

If it has rows, EF's generated `AlterColumn` will fail on the string→int cast. Replace it
with an explicit conversion placed **before** the `AlterColumn`:

```csharp
migrationBuilder.AddColumn<int>(
    name: "SectionTmp", table: "Videos", nullable: false, defaultValue: 0);

migrationBuilder.Sql(@"
    UPDATE Videos SET SectionTmp =
        CASE Section
            WHEN 'NewCase'       THEN 0
            WHEN 'TrackCase'     THEN 1
            WHEN 'ShareITVideos' THEN 2
            ELSE 0
        END;");

migrationBuilder.DropColumn(name: "Section", table: "Videos");
migrationBuilder.RenameColumn(
    name: "SectionTmp", table: "Videos", newName: "Section");
```

EF adds `defaultValue: ""` to the required `Title` column on its own — no edit needed there.
Existing rows get an empty title; set a real one from the admin panel afterwards.

**Do Step 4d before this step.** `dotnet ef migrations add` builds the project first, and the
`Section` string→enum change breaks `VideosController.Index` until 4d is applied. A build
failure produces no migration file at all.

```
dotnet ef database update
```

**Applied 2026-08-09** as `20260809085556_AddVideoCardFields`. The one existing row
(`'ShareITVideos'`) converted to `2` with its `VideoPath` intact.

## Step 4 — `Controllers/VideosController.cs`

**4a. Add a thumbnail allow-list** next to `_allowedVideoExtensions`:

```csharp
private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
```

**4b. `[HttpPost] Create`** — add `ModelState.Remove("ThumbnailPath")` and
`ModelState.Remove("ThumbnailFile")` alongside the existing removals. Then replace the inline
file-saving block (lines 110-132) with calls to the `SaveUploadAsync` helper defined at the end
of this step:

```csharp
var videoPath = await SaveUploadAsync(
    video.VideoFile, "videos", _allowedVideoExtensions, _maxFileSize);
if (videoPath == null)
{
    ModelState.AddModelError("VideoFile",
        $"Invalid file type, or size exceeds {_maxFileSize / (1024 * 1024)}MB.");
    return View(video);
}

video.VideoPath = videoPath;
video.FileType  = Path.GetExtension(video.VideoFile.FileName)
                      .TrimStart('.').ToUpperInvariant();
video.FileName  = video.VideoFile.FileName;
video.FileSize  = video.VideoFile.Length;
video.CreatedOn = DateTime.Now;

if (video.ThumbnailFile != null && video.ThumbnailFile.Length > 0)
{
    var thumbPath = await SaveUploadAsync(
        video.ThumbnailFile, "thumbnails", _allowedImageExtensions, 5 * 1024 * 1024);
    if (thumbPath == null)
    {
        ModelState.AddModelError("ThumbnailFile",
            "Thumbnail must be a JPG, PNG or WEBP under 5MB.");
        return View(video);
    }
    video.ThumbnailPath = thumbPath;
}
```

Thumbnail stays optional — the view falls back to a placeholder (Step 10).

**4c. Add the missing `[HttpPost] Edit`.** This action does not exist today. Add it after the
GET `Edit`:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, Video video)
{
    if (id != video.Id) return NotFound();

    ModelState.Remove("VideoFile");
    ModelState.Remove("ThumbnailFile");
    ModelState.Remove("FileType");

    var existing = await _context.Videos.FindAsync(id);
    if (existing == null) return NotFound();

    if (!ModelState.IsValid) return View(video);

    existing.Section     = video.Section;
    existing.Title       = video.Title;
    existing.Description = video.Description;
    existing.UpdatedOn   = DateTime.Now;

    if (video.VideoFile != null && video.VideoFile.Length > 0)
    {
        var saved = await SaveUploadAsync(
            video.VideoFile, "videos", _allowedVideoExtensions, _maxFileSize);
        if (saved == null)
        {
            ModelState.AddModelError("VideoFile", "Invalid video file or size limit exceeded.");
            return View(video);
        }
        DeleteFileIfExists(existing.VideoPath);
        existing.VideoPath = saved;
        existing.FileType  = Path.GetExtension(video.VideoFile.FileName)
                                 .TrimStart('.').ToUpperInvariant();
        existing.FileName  = video.VideoFile.FileName;
        existing.FileSize  = video.VideoFile.Length;
    }

    if (video.ThumbnailFile != null && video.ThumbnailFile.Length > 0)
    {
        var saved = await SaveUploadAsync(
            video.ThumbnailFile, "thumbnails", _allowedImageExtensions, 5 * 1024 * 1024);
        if (saved == null)
        {
            ModelState.AddModelError("ThumbnailFile", "Thumbnail must be a JPG, PNG or WEBP under 5MB.");
            return View(video);
        }
        DeleteFileIfExists(existing.ThumbnailPath);
        existing.ThumbnailPath = saved;
    }

    await _context.SaveChangesAsync();
    TempData["Success"] = "Video updated successfully!";
    return RedirectToAction(nameof(Index));
}
```

Note it loads the tracked entity and copies fields onto it, rather than `_context.Update(video)`
— that avoids blanking `VideoPath`/`FileSize`/`CreatedOn` when the form doesn't post them.

**4f. Two private helpers** used by 4b, 4c and 4e — **write these first**, before 4b.

Put them in `VideosController` next to the existing `CalculateTotalStorageSize()` helper
(line 162). This mirrors the convention already in the codebase: `DocumentController.cs:284`
has `UploadFileAsync` / `DeleteFile`, and `TicketController.cs:382` has `SaveUploadedFile`.
Each controller owns its own upload helper — keep the same names and the same
"return null when rejected" contract as `DocumentController` so this reads like its neighbours.

```csharp
/// Validates extension + size, saves under wwwroot/uploads/{folderName},
/// returns the web path, or null if the file was rejected.
private async Task<string> UploadFileAsync(
    IFormFile file, string folderName, string[] allowedExtensions, long maxSize)
{
    if (file == null || file.Length == 0)
        return null;

    var extension = Path.GetExtension(file.FileName).ToLower();
    if (!allowedExtensions.Contains(extension))
        return null;

    if (file.Length > maxSize)
        return null;

    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folderName);
    if (!Directory.Exists(uploadsFolder))
    {
        Directory.CreateDirectory(uploadsFolder);
    }

    var fileName = $"{Guid.NewGuid()}{extension}";
    var filePath = Path.Combine(uploadsFolder, fileName);

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    return $"/uploads/{folderName}/{fileName}";
}

// Helper method to delete files
private void DeleteFile(string filePath)
{
    if (string.IsNullOrEmpty(filePath))
        return;

    var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('/'));
    if (System.IO.File.Exists(fullPath))
    {
        System.IO.File.Delete(fullPath);
    }
}
```

This differs from `DocumentController.UploadFileAsync` in one way: extensions and size limit
are parameters rather than switched on `folderName`, because Videos needs two different
profiles (videos at 500MB, thumbnails at 5MB).

In 4b, 4c and 4e, use `UploadFileAsync(...)` and `DeleteFile(...)` in place of the
`SaveUploadAsync` / `DeleteFileIfExists` names used in those code blocks above.

These two helpers are why 4b, 4c and 4e stay short — there is exactly one copy of the
save-a-file and delete-a-file logic in the controller.

**4d. `Index`** — `Section` is now an enum, so the string filter breaks. Change the signature
to `string searchString, VideoSection? sectionFilter` and:

```csharp
if (!String.IsNullOrEmpty(searchString))
{
    videos = videos.Where(v => v.Title.Contains(searchString) ||
                               v.FileName.Contains(searchString));
}

if (sectionFilter.HasValue)
{
    videos = videos.Where(v => v.Section == sectionFilter.Value);
}
```

Delete the `ViewBag.Sections` distinct query (lines 45-48) — the section list is now the enum,
not whatever happens to be in the table.

**4e. `DeleteConfirmed` bug** — the delete is nested inside
`if (video != null && !string.IsNullOrEmpty(video.VideoPath))`, so a row with no file is never
removed. Restructure:

```csharp
var video = await _context.Videos.FindAsync(id);
if (video == null) return RedirectToAction(nameof(Index));

DeleteFileIfExists(video.VideoPath);
DeleteFileIfExists(video.ThumbnailPath);

_context.Videos.Remove(video);
await _context.SaveChangesAsync();
TempData["Success"] = "Video deleted successfully!";
return RedirectToAction(nameof(Index));
```

with a small private helper that null-checks the path and calls `System.IO.File.Delete`.

## Step 5 — `Views/Videos/Create.cshtml`

Replace the broken `<select>` at lines 33-45 with the enum-driven one, and add Title,
Description and thumbnail inputs above the video file input:

```html
<div class="form-group mb-3">
    <label asp-for="Title" class="control-label fw-bold"></label>
    <input asp-for="Title" class="form-control" placeholder="e.g. How to submit a Safety Concern" />
    <span asp-validation-for="Title" class="text-danger"></span>
    <small class="form-text text-muted">Shown as the card heading on the User Guide page.</small>
</div>

<div class="form-group mb-3">
    <label asp-for="Description" class="control-label fw-bold"></label>
    <textarea asp-for="Description" class="form-control" rows="3"></textarea>
    <span asp-validation-for="Description" class="text-danger"></span>
</div>

<div class="form-group mb-3">
    <label asp-for="Section" class="control-label fw-bold"></label>
    <select asp-for="Section" class="form-control"
            asp-items="Html.GetEnumSelectList<VideoSection>()"></select>
    <span asp-validation-for="Section" class="text-danger"></span>
    <small class="form-text text-muted">Which User Guide section this video appears under.</small>
</div>

<div class="form-group mb-3">
    <label asp-for="ThumbnailFile" class="control-label fw-bold"></label>
    <input asp-for="ThumbnailFile" class="form-control" type="file" accept="image/*" />
    <span asp-validation-for="ThumbnailFile" class="text-danger"></span>
    <small class="form-text text-muted">Optional. JPG, PNG or WEBP. A placeholder is used if omitted.</small>
</div>
```

`Html.GetEnumSelectList<VideoSection>()` emits exactly 3 options with distinct values `0/1/2`
and the `[Display(Name)]` text as labels — this is the fix for defect #3.

The inline `<script>` at line 112 does `document.querySelector('input[type="file"]')`, which
now grabs the *thumbnail* input if it comes first in the DOM. Scope it:
`document.querySelector('input[name="VideoFile"]')`.

## Step 6 — `Views/Videos/Edit.cshtml`

- Replace the free-text `<input asp-for="Section">` (line 39) with the same `<select>` block
  from Step 5.
- Add the Title and Description fields from Step 5.
- Add an optional "Replace Thumbnail" file input, and show the current one if present.
- Add `<input type="hidden" asp-for="ThumbnailPath" />` beside the other hidden fields
  (lines 30-35) so it survives the round trip.
- Fix the same `querySelector('input[type="file"]')` scoping issue (line 141).

## Step 7 — `Views/Videos/Index.cshtml`

- Line 38's filter dropdown: replace `new SelectList(ViewBag.Sections, …)` with
  `Html.GetEnumSelectList<VideoSection>()`, keeping the `<option value="">All Sections</option>`.
- Line 150's badge: `@item.Section.GetDisplayName()` instead of `@item.Section`.
- Add a **Title** column before the Section column — that's now the primary identifier, not
  the raw filename.

## Step 8 — `Models/ViewModel/UserGuideViewModel.cs` (new file)

```csharp
using ShareIT.Models;

namespace ShareIT.Models.ViewModel
{
    public class UserGuideViewModel
    {
        public List<GuideSectionViewModel> Sections { get; set; } = new();
    }

    public class GuideSectionViewModel
    {
        public VideoSection Section { get; set; }
        public string Heading { get; set; } = "";
        public string IconClass { get; set; } = "";
        public List<Video> Videos { get; set; } = new();
    }
}
```

`IconClass` keeps the existing per-section Font Awesome icons (`fa-plus-circle`, `fa-search`,
`fa-video`) out of the view's markup.

## Step 9 — `Controllers/HomeController.cs`

Replace `Guid()` (line 113):

```csharp
public async Task<IActionResult> Guid()
{
    var videos = await _context.Videos
        .OrderBy(v => v.CreatedOn)
        .ToListAsync();

    var icons = new Dictionary<VideoSection, string>
    {
        [VideoSection.NewCase]       = "fas fa-plus-circle",
        [VideoSection.TrackCase]     = "fas fa-search",
        [VideoSection.ShareITVideos] = "fas fa-video"
    };

    var model = new UserGuideViewModel
    {
        Sections = Enum.GetValues<VideoSection>()
            .Select(s => new GuideSectionViewModel
            {
                Section   = s,
                Heading   = s.GetDisplayName(),
                IconClass = icons[s],
                Videos    = videos.Where(v => v.Section == s).ToList()
            })
            .ToList()
    };

    return View(model);
}
```

Building from `Enum.GetValues` rather than from the data means the three headings always
render in a fixed order, even when a section is empty.

`Guid()` stays **public** (no `[Authorize]`) — the User Guide is employee-facing. That is safe:
`VideosController` remains `[Authorize]` so only staff can upload, and the files themselves are
static content under `wwwroot`, already served without auth.

## Step 10 — `Views/Home/Guid.cshtml`

**Keep:** the entire `<style>` block (lines 6-336), the page header, the Quick Start Guide,
the FAQ accordion, and the video modal markup.

**Delete:** the three hardcoded `guide-section` blocks (lines 373-606) and the `videoFiles`
JS dictionary (lines 696-707).

Add at the top:

```razor
@model ShareIT.Models.ViewModel.UserGuideViewModel
@using ShareIT.Models
```

Replace the deleted card blocks with:

```razor
@foreach (var section in Model.Sections)
{
    <div class="guide-section">
        <div class="section-header">
            <i class="@section.IconClass"></i>
            <h2>@section.Heading</h2>
        </div>

        @if (!section.Videos.Any())
        {
            <p class="tutorial-description">No tutorials in this section yet.</p>
        }
        else
        {
            <div class="tutorial-grid">
                @foreach (var v in section.Videos)
                {
                    <div class="tutorial-card">
                        <div class="card-header"><h3>@v.Title</h3></div>
                        <div class="card-body">
                            <div class="video-play-overlay">
                                <img src="@(v.ThumbnailPath ?? "/images/tutorials/placeholder.png")"
                                     alt="@v.Title" class="tutorial-image" />
                            </div>
                            <p class="tutorial-description">@v.Description</p>
                            <div class="tutorial-action">
                                <a href="#" class="btn-tutorial" data-video-path="@v.VideoPath">
                                    <i class="fas fa-play-circle"></i> Watch Tutorial
                                </a>
                            </div>
                        </div>
                    </div>
                }
            </div>
        }
    </div>
    <hr class="hr-divider" />
}
```

Then simplify the click handler (lines 710-724) — the path now travels with the card, which
is what permanently fixes defect #2:

```javascript
$('.btn-tutorial').click(function (e) {
    e.preventDefault();
    const videoPath = $(this).data('video-path');
    if (!videoPath) return;

    tutorialVideo.src = videoPath;
    tutorialVideo.load();
    $('#videoModalTitle').text($(this).closest('.tutorial-card').find('.card-header h3').text());
    videoModal.show();
});
```

Add a `wwwroot/images/tutorials/placeholder.png` — a plain `#302e2d` tile with the brand-red
play glyph is enough.

---

# Verification

Stop the app first (Shift+F5) — per `CLAUDE.md`, a running app locks the DLL and rebuilds
silently no-op. Then `dotnet run` and hard-refresh.

1. Upload a video with section **"How to File a New Case"**, a title, a description and a
   thumbnail → it appears under that exact heading on `/Home/Guid`.
2. Click its card → the modal plays the uploaded file, and the modal title matches the card.
3. Upload with **no** thumbnail → the placeholder renders, card still plays.
4. Upload one video into each of the 3 sections → each lands under the right heading, none
   cross over.
5. Edit a video's title and section → both change on `/Home/Guid` after refresh
   (this is the regression test for the missing `[HttpPost] Edit`).
6. Delete a video → the card disappears and the file is gone from
   `wwwroot/uploads/videos/`.
7. Leave a section empty → its heading still renders with the empty-state line.
8. Visit `/Home/Guid` while **signed out** → cards and playback still work.
9. Visit `/Videos` while signed out → redirected to login.
