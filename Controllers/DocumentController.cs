// Controllers/DocumentController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareIT.Data;
using ShareIT.Models;
using ShareIT.Models.ViewModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ShareIT.Controllers
{
    [Authorize]

    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public DocumentController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: Display all documents
        public async Task<IActionResult> Index()
        {
            var documents = await _context.Documents
                .OrderBy(d => d.DisplayOrder)
                .ThenBy(d => d.Name)
                .ToListAsync();

            return View(documents);
        }

        // GET: Create new document
        public IActionResult Create()
        {
            return View(new DocumentViewModel());
        }

        // POST: Create new document
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Handle PDF file upload
                    string pdfFilePath = await UploadFileAsync(model.PdfFile, "documents");
                    if (string.IsNullOrEmpty(pdfFilePath))
                    {
                        ModelState.AddModelError("PdfFile", "Invalid PDF file. Please upload a valid PDF.");
                        return View(model);
                    }

                    // Handle logo file upload (optional)
                    string logoPath = null;
                    if (model.LogoFile != null && model.LogoFile.Length > 0)
                    {
                        logoPath = await UploadFileAsync(model.LogoFile, "logos");
                    }

                    // Create document
                    var document = new Document
                    {
                        Name = model.Name,
                        Description = model.Description,
                        FilePath = pdfFilePath,
                        Logo = logoPath,
                        DisplayOrder = model.DisplayOrder ?? 0
                    };

                    _context.Documents.Add(document);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Document '{model.Name}' created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating document: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Edit document
        public async Task<IActionResult> Edit(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            var model = new DocumentViewModel
            {
                Id = document.Id,
                Name = document.Name,
                Description = document.Description,
                DisplayOrder = document.DisplayOrder,
                ExistingFilePath = document.FilePath,
                ExistingLogoPath = document.Logo
            };

            return View(model);
        }

        // POST: Edit document
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DocumentViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var document = await _context.Documents.FindAsync(id);
                    if (document == null)
                    {
                        return NotFound();
                    }

                    // Update PDF file if provided
                    if (model.PdfFile != null && model.PdfFile.Length > 0)
                    {
                        // Delete old file
                        DeleteFile(document.FilePath);

                        // Upload new file
                        document.FilePath = await UploadFileAsync(model.PdfFile, "documents");
                    }

                    // Update logo if provided
                    if (model.LogoFile != null && model.LogoFile.Length > 0)
                    {
                        // Delete old logo
                        if (!string.IsNullOrEmpty(document.Logo))
                        {
                            DeleteFile(document.Logo);
                        }

                        // Upload new logo
                        document.Logo = await UploadFileAsync(model.LogoFile, "logos");
                    }

                    // Update other fields
                    document.Name = model.Name;
                    document.Description = model.Description;
                    document.DisplayOrder = model.DisplayOrder ?? 0;

                    _context.Documents.Update(document);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Document '{model.Name}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating document: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Delete document
        public async Task<IActionResult> Delete(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // POST: Delete document
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id);
                if (document != null)
                {
                    // Delete files from server
                    DeleteFile(document.FilePath);
                    if (!string.IsNullOrEmpty(document.Logo))
                    {
                        DeleteFile(document.Logo);
                    }

                    _context.Documents.Remove(document);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Document '{document.Name}' deleted successfully!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting document: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Download document
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id);
                if (document == null)
                {
                    return NotFound();
                }

                var filePath = Path.Combine(_hostingEnvironment.WebRootPath, document.FilePath.TrimStart('/'));
                if (!System.IO.File.Exists(filePath))
                {
                    TempData["ErrorMessage"] = "File not found on server.";
                    return RedirectToAction(nameof(Index));
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                var fileName = Path.GetFileName(document.FilePath);
                return File(memory, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error downloading file: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // View document in browser
        public async Task<IActionResult> View(int id)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id);
                if (document == null)
                {
                    return NotFound();
                }

                var filePath = Path.Combine(_hostingEnvironment.WebRootPath, document.FilePath.TrimStart('/'));
                if (!System.IO.File.Exists(filePath))
                {
                    TempData["ErrorMessage"] = "File not found on server.";
                    return RedirectToAction(nameof(Index));
                }

                return PhysicalFile(filePath, "application/pdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error viewing file: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper method to upload files
        private async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return null;

            // Validate file type
            var allowedExtensions = folderName == "documents"
                ? new[] { ".pdf" }
                : new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".ico" };

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return null;

            // Validate file size (max 50MB for PDF, 5MB for images)
            var maxSize = folderName == "documents" ? 50 * 1024 * 1024 : 5 * 1024 * 1024;
            if (file.Length > maxSize)
                return null;

            // Create upload directory
            var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", folderName);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{folderName}/{fileName}";
        }

        // Helper method to delete files
        private void DeleteFile(string filePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    var fullPath = Path.Combine(_hostingEnvironment.WebRootPath, filePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw
                Console.WriteLine($"Error deleting file {filePath}: {ex.Message}");
            }
        }
    }
}