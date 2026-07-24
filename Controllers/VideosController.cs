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
    private readonly long _maxFileSize = 500 * 1024 * 1024; // 500MB max file size

    public VideosController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: Videos
    public async Task<IActionResult> Index(string searchString, string sectionFilter)
    {
        var videos = from v in _context.Videos select v;

        // Search by section or filename
        if (!String.IsNullOrEmpty(searchString))
        {
            videos = videos.Where(s => s.Section.Contains(searchString) ||
                                       s.FileName.Contains(searchString));
        }

        // Filter by section
        if (!String.IsNullOrEmpty(sectionFilter))
        {
            videos = videos.Where(s => s.Section == sectionFilter);
        }

        // Get distinct sections for filter dropdown
        ViewBag.Sections = await _context.Videos
            .Select(v => v.Section)
            .Distinct()
            .ToListAsync();

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

        if (ModelState.IsValid)
        {
            if (video.VideoFile != null && video.VideoFile.Length > 0)
            {
                // Validate file extension
                var fileExtension = Path.GetExtension(video.VideoFile.FileName).ToLowerInvariant();
                if (!_allowedVideoExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("VideoFile", "Invalid file type. Please upload a video file.");
                    return View(video);
                }

                // Validate file size
                if (video.VideoFile.Length > _maxFileSize)
                {
                    ModelState.AddModelError("VideoFile", $"File size exceeds {_maxFileSize / (1024 * 1024)}MB limit.");
                    return View(video);
                }

                // Create unique filename
                string fileName = $"{Guid.NewGuid()}{fileExtension}";
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "videos");

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await video.VideoFile.CopyToAsync(fileStream);
                }

                // Set video properties
                video.VideoPath = $"/uploads/videos/{fileName}";
                video.FileType = fileExtension.Replace(".", "").ToUpper();
                video.FileName = video.VideoFile.FileName;
                video.FileSize = video.VideoFile.Length;
                video.CreatedOn = DateTime.Now;

                // You can add video duration here using a library like FFmpeg
                // video.Duration = await GetVideoDuration(filePath);

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

        if (video != null && !string.IsNullOrEmpty(video.VideoPath))
        {
            // Delete physical file
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, video.VideoPath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.Videos.Remove(video);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Video deleted successfully!";
        }

        return RedirectToAction(nameof(Index));
    }
    // Rest of your controller methods (Edit, Delete, Details) remain the same
    // but update references from vedPath to VideoPath, and fType to FileType
}