using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareIT.Data;
using ShareIT.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
public class VideosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string[] _allowedVideoExtensions = { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm" };
    private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly long _maxFileSize = 500 * 1024 * 1024; // 500MB max file size
    private readonly long _maxImageSize = 5 * 1024 * 1024;  // 5MB max thumbnail size

    public VideosController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: Videos
    public async Task<IActionResult> Index(string searchString, VideoSection? sectionFilter)
    {
        var videos = from v in _context.Videos select v;

        if (!String.IsNullOrEmpty(searchString))
        {
            videos = videos.Where(v => v.Title.Contains(searchString) ||
                                       v.FileName.Contains(searchString));
        }

        if (sectionFilter.HasValue)
        {
            videos = videos.Where(v => v.Section == sectionFilter.Value);
        }

        ViewBag.CurrentFilter = searchString;
        ViewBag.CurrentSection = sectionFilter;

        // Calculate statistics for sidebar
        ViewBag.TotalVideos = await _context.Videos.CountAsync();
        ViewBag.TotalSize = await CalculateTotalStorageSize();
        ViewBag.LastUpload = await _context.Videos
            .OrderByDescending(v => v.CreatedOn)
            .Select(v => v.CreatedOn)
            .FirstOrDefaultAsync();

        return View(await videos.OrderByDescending(v => v.CreatedOn).ToListAsync());
    }

    // GET: Videos/Create
    public async Task<IActionResult> Create()
    {
        // Add statistics for the create view
        ViewBag.TotalVideos = await _context.Videos.CountAsync();
        ViewBag.TotalSize = await CalculateTotalStorageSize();
        ViewBag.LastUpload = await _context.Videos
            .OrderByDescending(v => v.CreatedOn)
            .Select(v => v.CreatedOn)
            .FirstOrDefaultAsync();

        return View();
    }

    // POST: Videos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Video video)
    {
        // Remove validation for properties that shouldn't be validated from form
        ModelState.Remove("VideoPath");
        ModelState.Remove("FileType");
        ModelState.Remove("FileName");
        ModelState.Remove("FileSize");
        ModelState.Remove("Duration");
        ModelState.Remove("UploadDate");
        ModelState.Remove("ThumbnailPath");
        ModelState.Remove("ThumbnailFile");

        if (ModelState.IsValid)
        {
            if (video.VideoFile != null && video.VideoFile.Length > 0)
            {
                // Save the video file
                var videoPath = await UploadFileAsync(
                    video.VideoFile, "videos", _allowedVideoExtensions, _maxFileSize);

                if (videoPath == null)
                {
                    ModelState.AddModelError("VideoFile",
                        $"Invalid file type, or size exceeds {_maxFileSize / (1024 * 1024)}MB limit.");
                    return View(video);
                }

                video.VideoPath = videoPath;
                video.FileType = Path.GetExtension(video.VideoFile.FileName)
                                     .TrimStart('.').ToUpperInvariant();
                video.FileName = video.VideoFile.FileName;
                video.FileSize = video.VideoFile.Length;
                video.CreatedOn = DateTime.Now;

                // Save the thumbnail if one was provided (optional)
                if (video.ThumbnailFile != null && video.ThumbnailFile.Length > 0)
                {
                    var thumbnailPath = await UploadFileAsync(
                        video.ThumbnailFile, "thumbnails", _allowedImageExtensions, _maxImageSize);

                    if (thumbnailPath == null)
                    {
                        ModelState.AddModelError("ThumbnailFile",
                            "Thumbnail must be a JPG, PNG or WEBP under 5MB.");
                        return View(video);
                    }

                    video.ThumbnailPath = thumbnailPath;
                }

                _context.Add(video);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Video uploaded successfully!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("VideoFile", "Please select a video file.");
            }
        }

        // Reload statistics for the view
        ViewBag.TotalVideos = await _context.Videos.CountAsync();
        ViewBag.TotalSize = await CalculateTotalStorageSize();
        ViewBag.LastUpload = await _context.Videos
            .OrderByDescending(v => v.CreatedOn)
            .Select(v => v.CreatedOn)
            .FirstOrDefaultAsync();

        return View(video);
    }

    // Helper method to calculate total storage size
    private async Task<string> CalculateTotalStorageSize()
    {
        var totalBytes = await _context.Videos.SumAsync(v => v.FileSize);

        if (totalBytes >= 1024 * 1024 * 1024) // GB
        {
            return $"{totalBytes / (1024.0 * 1024 * 1024):F2} GB";
        }
        else if (totalBytes >= 1024 * 1024) // MB
        {
            return $"{totalBytes / (1024.0 * 1024):F2} MB";
        }
        else if (totalBytes >= 1024) // KB
        {
            return $"{totalBytes / 1024.0:F2} KB";
        }
        else
        {
            return $"{totalBytes} bytes";
        }
    }

    /// Validates extension + size, saves under wwwroot/uploads/{folder},
    /// returns the web path, or null if the file was rejected.
    private async Task<string?> UploadFileAsync(
        IFormFile file, string folder, string[] allowedExtensions, long maxBytes)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext) || file.Length > maxBytes) return null;

        var name = $"{Guid.NewGuid()}{ext}";
        var dir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(dir);

        using var stream = new FileStream(Path.Combine(dir, name), FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{folder}/{name}";
    }

    private void DeleteFile(string? webPath)
    {
        if (string.IsNullOrEmpty(webPath)) return;
        var full = Path.Combine(_webHostEnvironment.WebRootPath, webPath.TrimStart('/'));
        if (System.IO.File.Exists(full)) System.IO.File.Delete(full);
    }
    // GET: Videos/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var video = await _context.Videos
            .FirstOrDefaultAsync(m => m.Id == id);
        if (video == null)
        {
            return NotFound();
        }

        return View(video);
    }

    // GET: Videos/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var video = await _context.Videos.FindAsync(id);
        if (video == null)
        {
            return NotFound();
        }
        return View(video);
    }

    // POST: Videos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Video video)
    {
        if (id != video.Id)
        {
            return NotFound();
        }

        // These are never posted by the edit form
        ModelState.Remove("VideoFile");
        ModelState.Remove("ThumbnailFile");
        ModelState.Remove("FileType");

        if (!ModelState.IsValid)
        {
            return View(video);
        }

        // Load the tracked entity and copy onto it, so fields the form doesn't
        // post (VideoPath, FileSize, CreatedOn) aren't blanked out.
        var existing = await _context.Videos.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.Section = video.Section;
        existing.Title = video.Title;
        existing.Description = video.Description;
        existing.UpdatedOn = DateTime.Now;

        // Optional replacement video
        if (video.VideoFile != null && video.VideoFile.Length > 0)
        {
            var videoPath = await UploadFileAsync(
                video.VideoFile, "videos", _allowedVideoExtensions, _maxFileSize);

            if (videoPath == null)
            {
                ModelState.AddModelError("VideoFile",
                    $"Invalid file type, or size exceeds {_maxFileSize / (1024 * 1024)}MB limit.");
                return View(video);
            }

            DeleteFile(existing.VideoPath);
            existing.VideoPath = videoPath;
            existing.FileType = Path.GetExtension(video.VideoFile.FileName)
                                    .TrimStart('.').ToUpperInvariant();
            existing.FileName = video.VideoFile.FileName;
            existing.FileSize = video.VideoFile.Length;
        }

        // Optional replacement thumbnail
        if (video.ThumbnailFile != null && video.ThumbnailFile.Length > 0)
        {
            var thumbnailPath = await UploadFileAsync(
                video.ThumbnailFile, "thumbnails", _allowedImageExtensions, _maxImageSize);

            if (thumbnailPath == null)
            {
                ModelState.AddModelError("ThumbnailFile",
                    "Thumbnail must be a JPG, PNG or WEBP under 5MB.");
                return View(video);
            }

            DeleteFile(existing.ThumbnailPath);
            existing.ThumbnailPath = thumbnailPath;
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Video updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET: Videos/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var video = await _context.Videos
            .FirstOrDefaultAsync(m => m.Id == id);
        if (video == null)
        {
            return NotFound();
        }

        return View(video);
    }

    // POST: Videos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var video = await _context.Videos.FindAsync(id);

        if (video == null)
        {
            return RedirectToAction(nameof(Index));
        }

        // Delete physical files (no-ops when the path is empty or missing)
        DeleteFile(video.VideoPath);
        DeleteFile(video.ThumbnailPath);

        _context.Videos.Remove(video);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Video deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
    // Rest of your controller methods (Edit, Delete, Details) remain the same
    // but update references from vedPath to VideoPath, and fType to FileType
}