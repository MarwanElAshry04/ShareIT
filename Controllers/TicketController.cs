using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShareIT.Data;
using ShareIT.Data;
using ShareIT.Models;
using ShareIT.Models;
using ShareIT.Models.ViewModel;
using ShareIT.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace ShareIT.Controllers
{
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailService _emailService;
        private readonly TimelineService _timelineService;


        private readonly IConfiguration _configuration;
            private const int MAX_FILE_SIZE = 10485760; // 10MB

            public TicketController(ApplicationDbContext context, IWebHostEnvironment environment, IConfiguration configuration, IEmailService emailService, TimelineService timelineService)
            {
                _context = context;
                _environment = environment;
                _configuration = configuration;
                _emailService = emailService;
                _timelineService = timelineService;
        }

        // GET: /Ticket/New
        public async Task<IActionResult> New()
        {
            var model = new NewTicketViewModel
            {
                RefNo = GenerateRefNo(),
                Companies = await _context.Companies.ToListAsync(),
                Departments = await _context.Departments.ToListAsync(),
                TicketTypes = await _context.TicketTypes.ToListAsync(),
                Relations = await GetRelationsAsync(), // Add this line
                CurrentStep = 1
            };

            return View(model);
        }
        // Add this method to get relations
        private async Task<List<LookupItem>> GetRelationsAsync()
        {
            // Assuming you have a Relations table or this comes from another source
            // For now, return sample data
            return new List<LookupItem>
    {
        new LookupItem { Id = 1, Name = "Supplier" },
        new LookupItem { Id = 2, Name = "Customer" },
        new LookupItem { Id = 3, Name = "Partner" },
        new LookupItem { Id = 4, Name = "Contractor" },
        new LookupItem { Id = 5, Name = "Other" }
    };
        }

        // Also update the POST method to load relations
        private async Task LoadLookupData(NewTicketViewModel model)
        {
            model.Companies = await _context.Companies.ToListAsync();
            model.Departments = await _context.Departments.ToListAsync();
            model.TicketTypes = await _context.TicketTypes.ToListAsync();
            model.Relations = await GetRelationsAsync(); // Add this line
        }
        [HttpPost]
        public async Task<IActionResult> New(NewTicketViewModel model)
        {
            if (model.CurrentStep == 1)
            {
                // Step 1: Reporter Data Validation
                if (model.ReporterType == "Disclose")
                {
                    if (model.IsEmployee == "Yes")
                    {
                        
                        if (string.IsNullOrEmpty(model.EmpNum))
                            ModelState.AddModelError("EmpNum", "Universal Code is required");
                        if (!model.EmpCompanyId.HasValue || model.EmpCompanyId == 0)
                            ModelState.AddModelError("EmpCompanyId", "Company is required");
                        if (!model.EmpDepartmentId.HasValue || model.EmpDepartmentId == 0)
                            ModelState.AddModelError("EmpDepartmentId", "Department is required");
                        if (string.IsNullOrEmpty(model.EmpName))
                            ModelState.AddModelError("EmpName", "Name is required");
                        if (string.IsNullOrEmpty(model.EmpMobile))
                            ModelState.AddModelError("EmpMobile", "Mobile is required");
                        if (string.IsNullOrEmpty(model.EmpEmail1))
                            ModelState.AddModelError("EmpEmail1", "Email is required");
                    }
                    else if (model.IsEmployee == "No")
                    {
                        if (!model.NonEmpRelationId.HasValue || model.NonEmpRelationId == 0)
                            ModelState.AddModelError("NonEmpRelationId", "Relationship is required");
                        if (string.IsNullOrEmpty(model.Organization))
                            ModelState.AddModelError("Organization", "Organization is required");
                        if (string.IsNullOrEmpty(model.NonEmpName))
                            ModelState.AddModelError("NonEmpName", "Name is required");
                        if (string.IsNullOrEmpty(model.NonEmpMobile))
                            ModelState.AddModelError("NonEmpMobile", "Mobile is required");
                        if (string.IsNullOrEmpty(model.NonEmpEmail1))
                            ModelState.AddModelError("NonEmpEmail1", "Email is required");
                    }
                }

                if (ModelState.IsValid)
                {
                    model.CurrentStep = 2;
                }
            }
            else if (model.CurrentStep == 2)
            {
                // Step 2: Ticket Data Validation
                //if (model.TicketTypeId == 0)
                //    ModelState.AddModelError("TicketTypeId", "Ticket type is required");
                if (string.IsNullOrEmpty(model.Description))
                    ModelState.AddModelError("Description", "Description is required");
                // "Concerning" party is only required for Complaints; suggestions
                // and feedback are not filed against a company or department.
                if (model.Kind == TicketKind.Complaint)
                {
                    if (model.ConcerningCompanyId == 0)
                        ModelState.AddModelError("ConcerningCompanyId", "Company is required");
                    if (model.ConcerningDepartmentId == 0)
                        ModelState.AddModelError("ConcerningDepartmentId", "Department is required");
                }

                if (ModelState.IsValid)
                {
                    model.CurrentStep = 3;
                }
            }
            else if (model.CurrentStep == 3)
            {
                var form = await Request.ReadFormAsync();
                var action = form["action"].ToString();

                if (action == "continue")
                {
                    var savedPaths = new List<string>();

                    if (model.Files != null && model.Files.Count > 0)
                    {
                        foreach (var file in model.Files)
                        {
                            if (file.Length > MAX_FILE_SIZE)
                            {
                                ModelState.AddModelError("Files", $"File {file.FileName} exceeds 10MB limit");
                                continue;
                            }

                            var allowedExtensions = new[] { ".docx", ".pdf", ".doc", ".wav", ".mp3", ".webm", ".jpg", ".jpeg", ".png", ".msg" };
                            var extension = Path.GetExtension(file.FileName).ToLower();
                            if (!allowedExtensions.Contains(extension))
                            {
                                ModelState.AddModelError("Files", $"File {file.FileName} has invalid type");
                                continue;
                            }

                            // Save to temp folder immediately
                            var tempFolder = Path.Combine(_environment.WebRootPath, "TempUploads", model.RefNo);
                            if (!Directory.Exists(tempFolder))
                                Directory.CreateDirectory(tempFolder);

                            var uniqueName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            var tempPath = Path.Combine(tempFolder, uniqueName);

                            using (var stream = new FileStream(tempPath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            savedPaths.Add($"TempUploads/{model.RefNo}/{uniqueName}");
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        // Store saved paths — join with pipe separator
                        model.UploadedFilePaths = string.Join("|", savedPaths);
                        model.CurrentStep = 4;
                    }
                }
                else if (action == "back")
                {
                    model.CurrentStep = 2;
                }
            }
            else if (model.CurrentStep == 4)
            {
                bool hasErrors = false;

                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError("Password", "Password is required");
                    hasErrors = true;
                }
                else if (model.Password.Length < 8)
                {
                    ModelState.AddModelError("Password", "Password must be at least 8 characters");
                    hasErrors = true;
                }

                if (string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    ModelState.AddModelError("ConfirmPassword", "Please confirm your password");
                    hasErrors = true;
                }
                else if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Passwords do not match");
                    hasErrors = true;
                }

                if (hasErrors)
                {
                    await LoadLookupData(model);
                    return View(model);
                }

                try
                {
                    // ✅ await so ticket is Ticket not Task<Ticket>
                    var ticket = await SaveTicket(model);

                    // Move temp files
                    if (!string.IsNullOrEmpty(model.UploadedFilePaths))
                    {
                        var paths = model.UploadedFilePaths.Split('|', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var relativePath in paths)
                        {
                            var tempFull = Path.Combine(_environment.WebRootPath,
                                relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

                            if (System.IO.File.Exists(tempFull))
                            {
                                var finalFolder = Path.Combine(_environment.WebRootPath, "CompFiles", model.RefNo);
                                if (!Directory.Exists(finalFolder))
                                    Directory.CreateDirectory(finalFolder);

                                var fileName = Path.GetFileName(tempFull);
                                var finalPath = Path.Combine(finalFolder, fileName);
                                System.IO.File.Move(tempFull, finalPath, overwrite: true);
                            }
                        }
                    }

                    await SendEmailNotification(model);

                    // ✅ ticket is now Ticket — .Id and .CreatedOn are accessible
                    await _timelineService.AddEventAsync(
                        ticketId: ticket.Id,
                        title: "Ticket Submitted",
                        description: "Ticket was submitted to the system",
                        icon: "fas fa-plus-circle",
                        color: "danger",
                        createdBy: "System",
                        eventDate: ticket.CreatedOn ?? DateTime.Now
                    );

                    return RedirectToAction("Confirmation", new { refNo = model.RefNo });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                    await LoadLookupData(model);
                    return View(model);
                }
            }
            await LoadLookupData(model);
            return View(model);
        }
        // GET: /Ticket/Confirmation
        public IActionResult Confirmation(string refNo)
            {
                ViewBag.RefNo = refNo;
                return View();
            }

            // GET: /Ticket/GetSubTypes
            [HttpGet]
            public async Task<JsonResult> GetSubTypes(int typeId)
            {
                // This would query your SubTicketTypes table
                var subTypes = new List<LookupItem>
            {
                new LookupItem { Id = 1, Name = "Sub Type 1" },
                new LookupItem { Id = 2, Name = "Sub Type 2" }
            };
                return Json(subTypes);
            }

            // GET: /Ticket/UploadAudio
            [HttpPost]
            public async Task<IActionResult> UploadAudio(IFormFile audioFile, string refNo)
            {
                if (audioFile == null || audioFile.Length == 0)
                    return BadRequest("No file uploaded");

                if (audioFile.Length > MAX_FILE_SIZE)
                    return BadRequest("File size exceeds 10MB limit");

                var allowedExtensions = new[] { ".wav", ".mp3" };
                var extension = Path.GetExtension(audioFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest("Invalid file type. Only WAV and MP3 allowed.");

                var filePath = await SaveUploadedFile(refNo, audioFile);

                return Ok(new { fileName = audioFile.FileName, filePath });
            }

            // GET: /Ticket/DeleteFile
            [HttpPost]
            public IActionResult DeleteFile(string filePath)
            {
                try
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                        return Ok();
                    }
                    return NotFound();
                }
                catch (Exception)
                {
                    return StatusCode(500);
                }
            }

            #region Private Methods

          

            private string GenerateRefNo()
            {
                var random = new Random();
                int num = random.Next(1000, 5000);
                return DateTime.Now.ToString("ddMMyyhhmm") + num.ToString();
            }

            private async Task<string> SaveUploadedFile(string refNo, IFormFile file)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "CompFiles", refNo);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // If WAV file, process pitch shifting
                if (Path.GetExtension(file.FileName).ToLower() == ".wav")
                {
                    ProcessWavFile(filePath);
                }

                return filePath;
            }

            private void ProcessWavFile(string filePath)
            {
                // Implement pitch shifting logic here
                // This is simplified - you'll need to port your existing WAV processing logic
                try
                {
                    // Read WAV file, apply pitch shift, save back
                    // You'll need to implement this based on your existing code
                }
                catch (Exception)
                {
                    // Log error but don't fail the upload
                }
            }

        private async Task<Ticket> SaveTicket(NewTicketViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Save Reporter
                    var reporter = new Reporter
                    {
                        reporterType = model.ReporterType == "Disclose" ? "Disclose" : "Anonymous",
                        name = model.ReporterType == "Disclose" ?
                              (model.IsEmployee == "Yes" ? model.EmpName : model.NonEmpName) : "Anonymous",
                        email1 = model.ReporterType == "Disclose" ?
                               (model.IsEmployee == "Yes" ? model.EmpEmail1 : model.NonEmpEmail1) : "",
                        email2 = model.ReporterType == "Disclose" ?
                               (model.IsEmployee == "Yes" ? model.EmpEmail2 : model.NonEmpEmail2) : "",
                        mobile = model.ReporterType == "Disclose" ?
                                (model.IsEmployee == "Yes" ? model.EmpMobile : model.NonEmpMobile) : "",
                        empCode = model.ReporterType == "Disclose" && model.IsEmployee == "Yes" ? model.EmpNum : "0",
                        CreatedBy = model.ReporterType == "Disclose" ?
                                   (model.IsEmployee == "Yes" ? model.EmpName : model.NonEmpName) : "Anonymous",
                        CreatedOn = DateTime.Now
                    };

                    _context.Reporters.Add(reporter);
                    await _context.SaveChangesAsync();

                    // Save Ticket
                    var ticket = new Ticket
                    {
                        RefNo = model.RefNo,
                        Kind = model.Kind,
                        TicketTypeId = model.TicketTypeId,
                        ReporterId = reporter.Id,
                        Status = "Initiate",
                        Desc = model.Description,
                        CreatedOn = DateTime.Now,
                        password = EncodePasswordToBase64(model.Password),
                        ConcerningCompany = (await _context.Companies.FindAsync(model.ConcerningCompanyId))?.Name,
                        ConcerningDepartment = (await _context.Departments.FindAsync(model.ConcerningDepartmentId))?.Name,
                        ConcerningPerson = model.ConcerningPerson,
                        ConcerningOther = model.ConcerningOtherSuppliers,
                        langFlag = "E"
                    };

                    _context.Tickets.Add(ticket);
                    await _context.SaveChangesAsync();

                    // Update Reporter with Ticket Id
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                return ticket; // ✅ always returns

            }
            catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            private string EncodePasswordToBase64(string password)
            {
                var plainTextBytes = Encoding.UTF8.GetBytes(password);
                return Convert.ToBase64String(plainTextBytes);
            }

            private async Task SendEmailNotification(NewTicketViewModel model)
            {
                // Implement email sending logic
                // This would use your existing Mail class or IEmailSender service
                try
                {
                    var subject = $"New Ticket with ref. {model.RefNo}";
                    var body = $"Dear,<br/>Kindly be informed that a new ticket has been inserted.<br/>" +
                              $"Ticket concerning: {model.ConcerningCompanyId} in the category of {model.TicketTypeId}<br/>" +
                              $"{(model.ReporterType == "Anonymous" ? "Note: User is Anonymous<br/>" : "")}" +
                              $"Thanks";

                    // Get admin emails from configuration or database
                    var adminEmails = _configuration["AdminEmails"]?.Split(',') ?? new[] { "admin@example.com" };

                    foreach (var email in adminEmails)
                    {
                        // Send email using your preferred method
                        // await _emailSender.SendEmailAsync(email, subject, body);
                    }

                    // Send confirmation to reporter if not anonymous
                    if (model.ReporterType == "Disclose")
                    {
                        var reporterEmail = model.IsEmployee == "Yes" ? model.EmpEmail1 : model.NonEmpEmail1;
                        // await _emailSender.SendEmailAsync(reporterEmail, "Ticket Submitted", "Your ticket has been submitted successfully.");
                    }
                }
                catch (Exception)
                {
                    // Log email sending error but don't fail the ticket submission
                }
            }

        #endregion
        
        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _context.Tickets
                .Include(c => c.TicketType)
                .Include(c => c.Reporter)
                .Include(c => c.TicketChats)
                .Include(c => c.TicketAttachments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // Controllers/TicketController.cs - إضافة هذه الدوال

        // GET: Ticket/CreateFromEmail/5
        public async Task<IActionResult> CreateFromEmail(int id)
        {
            try
            {
                // جلب البيانات المؤقتة
                var tempEmail = await _context.TempEmails.FindAsync(id);
                if (tempEmail == null)
                {
                    TempData["Error"] = "البريد غير موجود";
                    return RedirectToAction("Index", "Home");
                }

                if (tempEmail.IsProcessed)
                {
                    TempData["Error"] = "تمت معالجة هذا البريد مسبقاً";
                    return RedirectToAction("Index", "Home");
                }

                // تجهيز الـ ViewModel
                var model = new EmailTicketViewModel
                {
                    TempEmailId = tempEmail.Id,
                    EmailSubject = tempEmail.Subject,
                    EmailBody = tempEmail.FullBody ?? tempEmail.BodyPreview,
                    EmailSender = tempEmail.SenderEmail,
                    EmailDate = tempEmail.DateTimeCreated,
                    ReporterName = tempEmail.SenderName,
                    ReporterEmail = tempEmail.SenderEmail,
                    Desc = tempEmail.FullBody ?? tempEmail.BodyPreview,
                    LangFlag = "E" // English default
                };

                // فك تشفير المرفقات لو موجودة
                if (!string.IsNullOrEmpty(tempEmail.Attachments))
                {
                    try
                    {
                        model.EmailAttachments = JsonSerializer.Deserialize<List<EmailAttachmentViewModel>>(tempEmail.Attachments)
                            ?? new List<EmailAttachmentViewModel>();
                    }
                    catch
                    {
                        // Ignore deserialization errors
                    }
                }

                // تجهيز بيانات الـ dropdowns
                ViewBag.TicketTypes = await _context.TicketTypes
                    .ToListAsync();
                ViewBag.Companies = await _context.Companies
                    .ToListAsync();
                ViewBag.Departments = await _context.Departments
                    .ToListAsync();

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء تحميل الصفحة";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Ticket/CreateFromEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromEmail(EmailTicketViewModel model)
        {
            try
            {
                // التحقق من صحة النموذج
                if (!ModelState.IsValid)
                {
                    ViewBag.TicketTypes = await _context.TicketTypes
                       
                        .ToListAsync();
                    ViewBag.Companies = await _context.Companies
                        .ToListAsync();
                    ViewBag.Departments = await _context.Departments
                        .ToListAsync();
                    return View(model);
                }

                // جلب البيانات المؤقتة
                var tempEmail = await _context.TempEmails.FindAsync(model.TempEmailId);
                if (tempEmail == null)
                {
                    ModelState.AddModelError("", "البيانات المؤقتة غير موجودة");
                    return View(model);
                }

                if (tempEmail.IsProcessed)
                {
                    ModelState.AddModelError("", "تمت معالجة هذا البريد مسبقاً");
                    return View(model);
                }

                // بدء ترانزاكشن
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 1. إنشاء Reporter (المُبلغ)
                    var reporter = new Reporter
                    {
                        reporterType = "Disclose", // Disclosed identity (since we have email)
                        name = model.ReporterName ?? tempEmail.SenderName ?? "Unknown",
                        email1 = model.ReporterEmail ?? tempEmail.SenderEmail,
                        email2 = "",
                        mobile = model.ReporterMobile ?? "",
                        empCode = model.EmpCode ?? "0",
                        CreatedBy = "Outlook Add-in",
                        CreatedOn = DateTime.Now
                    };
                    // 1. Insert Reporter WITHOUT CompId first
                    _context.Reporters.Add(reporter);
                    await _context.SaveChangesAsync(); // reporter.Id is now generated

                    // 2. Generate RefNo
                    var refNo = GenerateRefNo();

                    // 3. Insert Ticket using reporter.Id
                    var ticket = new Ticket
                    {
                        RefNo = refNo,
                        TicketTypeId = model.TicketTypeId,
                        ReporterId = reporter.Id,       // ✅ reporter.Id now exists
                        Status = "Initiate",
                        Desc = model.Desc ?? model.EmailBody,
                        CreatedOn = DateTime.Now,
                        password = EncodePasswordToBase64(model.Password),
                        ConcerningCompany = model.ConcerningCompany,
                        ConcerningDepartment = model.ConcerningDepartment,
                        ConcerningPerson = model.ConcerningPerson,
                        ConcerningOther = model.ConcerningOther,
                        langFlag = model.LangFlag ?? "E",
                        toMails = tempEmail.ToRecipients,
                        ccMails = tempEmail.CcRecipients,
                        messageMail = tempEmail.FullBody,
                        CreatedBy = "Outlook Add-in"
                    };

                    _context.Tickets.Add(ticket);
                    await _context.SaveChangesAsync(); // ticket.Id is now generated

                    // 4. ✅ NOW update reporter.CompId back-reference

                    // 4. إنشاء Chat (المحادثة) من البريد
                    var chat = new Chats
                    {
                        compId = ticket.Id,
                        Comment = $"تم استقبال الشكوى عبر البريد الإلكتروني.\n\n" +
                                 $"المرسل: {tempEmail.SenderName} ({tempEmail.SenderEmail})\n" +
                                 $"الموضوع: {tempEmail.Subject}\n" +
                                 $"التاريخ: {tempEmail.DateTimeCreated:dd/MM/yyyy HH:mm}\n\n" +
                                 $"نص البريد:\n{tempEmail.FullBody ?? tempEmail.BodyPreview}",
                        hasNewMessage = true,
                        flag = "incoming",
                        CreatedOn = DateTime.Now,
                        CreatedBy = "System"
                    };

                    _context.Chats.Add(chat);
                    await _context.SaveChangesAsync();

                    // 5. معالجة المرفقات
                    if (model.EmailAttachments != null && model.EmailAttachments.Any())
                    {
                        foreach (var att in model.EmailAttachments)
                        {
                            string filePath = null;

                            // إذا كان المرفق محفوظ مؤقتاً
                            if (!string.IsNullOrEmpty(att.TempPath) && System.IO.File.Exists(att.TempPath))
                            {
                                // نقل الملف إلى مجلد الشكوى
                                var uploadsFolder = Path.Combine(_environment.WebRootPath, "CompFiles", refNo);
                                if (!Directory.Exists(uploadsFolder))
                                    Directory.CreateDirectory(uploadsFolder);

                                var fileName = $"{Guid.NewGuid()}_{att.Name}";
                                var newPath = Path.Combine(uploadsFolder, fileName);

                                System.IO.File.Move(att.TempPath, newPath);
                                filePath = $"/CompFiles/{refNo}/{fileName}";
                            }
                            // إذا كان المرفق مشفر بـ Base64
                            else if (!string.IsNullOrEmpty(att.ContentBytes))
                            {
                                var uploadsFolder = Path.Combine(_environment.WebRootPath, "CompFiles", refNo);
                                if (!Directory.Exists(uploadsFolder))
                                    Directory.CreateDirectory(uploadsFolder);

                                var fileName = $"{Guid.NewGuid()}_{att.Name}";
                                var newPath = Path.Combine(uploadsFolder, fileName);

                                var bytes = Convert.FromBase64String(att.ContentBytes);
                                await System.IO.File.WriteAllBytesAsync(newPath, bytes);

                                filePath = $"/CompFiles/{refNo}/{fileName}";
                            }

                            if (!string.IsNullOrEmpty(filePath))
                            {
                                var attachment = new Attachment
                                {
                                    compId = ticket.Id,
                                    filePath = filePath,
                                    attachmentType = "email_attachment",
                                    CreatedOn = DateTime.Now,
                                    CreatedBy = "Outlook Add-in"
                                };
                                _context.Attachments.Add(attachment);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    // 6. تحديث حالة البيانات المؤقتة
                    tempEmail.IsProcessed = true;
                    _context.TempEmails.Update(tempEmail);
                    await _context.SaveChangesAsync();

                    // 7. إرسال إشعار للإدارة
                    await SendNewTicketNotification(ticket, reporter);
                    await SendReporterConfirmationEmail(ticket, reporter,model.Password);
                    // إنهاء الترانزاكشن
                    await transaction.CommitAsync();

                    TempData["Success"] = $"تم إنشاء الشكوى بنجاح. رقم المرجع: {refNo}";
                    return RedirectToAction("Details", new { id = ticket.Id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", $"حدث خطأ أثناء حفظ الشكوى: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"حدث خطأ: {ex.Message}");
            }

            // في حالة الخطأ، أعد تحميل الصفحة
            ViewBag.TicketTypes = await _context.TicketTypes
                .ToListAsync();
            ViewBag.Companies = await _context.Companies
                .ToListAsync();
            ViewBag.Departments = await _context.Departments
                .ToListAsync();

            return View(model);
        }

        // GET: Ticket/DownloadEmailAttachment
        [HttpGet]
        public async Task<IActionResult> DownloadEmailAttachment(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                if (!System.IO.File.Exists(fullPath))
                    return NotFound();

                var memory = new MemoryStream();
                using (var stream = new FileStream(fullPath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                var fileName = Path.GetFileName(fullPath);
                var contentType = GetContentType(fileName);

                return File(memory, contentType, fileName);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        // GET: Ticket/CheckEmailStatus/5
        [HttpGet]
        public async Task<IActionResult> CheckEmailStatus(int id)
        {
            var tempEmail = await _context.TempEmails.FindAsync(id);
            if (tempEmail == null)
                return NotFound();

            return Json(new
            {
                processed = tempEmail.IsProcessed,
                processedAt = tempEmail.IsProcessed ? DateTime.Now : (DateTime?)null
            });
        }

        // دالة مساعدة لإرسال إشعار الإدارة
        private async Task SendNewTicketNotification(Ticket ticket, Reporter reporter)
        {
            try
            {
                // Get admin emails from configuration
                var adminEmails = _configuration["AdminEmails"]?.Split(',')
                    ?? new[] { "compliance-int@elsewedy.com" };

                var subject = $"New Ticket Created from Email - Ref: {ticket.RefNo}";
                var body = $@"
            <h3>New Ticket Created from Email</h3>
            <p><strong>Reference No:</strong> {ticket.RefNo}</p>
            <p><strong>Sender:</strong> {reporter.name} ({reporter.email1})</p>
            <p><strong>Date:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
            <p><strong>Type:</strong> {ticket.TicketType?.ComplainType}</p>
            <p><strong>Description:</strong> {ticket.Desc}</p>
            <p><a href='https://compliance.elsewedy.com/FollowTicket/Index/{ticket.Id}'>View Ticket</a></p>
        ";

                // Send email using your email service
                if (_emailService != null)
                {
                    foreach (var email in adminEmails)
                    {
                        if (!string.IsNullOrWhiteSpace(email))
                            await _emailService.SendEmailAsync(email.Trim(), subject, body);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        private async Task SendReporterConfirmationEmail(Ticket ticket, Reporter reporter, string plainPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reporter.email1))
                    return;

                var subject = $"Your Ticket Has Been Received – Ref: {ticket.RefNo}";
                var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto;'>
                <h3 style='color: #c4252a;'>Your Ticket Has Been Received</h3>
                <p>Dear {reporter.name},</p>
                <p>Thank you for reaching out. Your ticket has been successfully registered in our system.</p>
                <p>Please keep the following details to track your case:</p>
                <table style='border-collapse: collapse; width: 100%; margin: 20px 0;'>
                    <tr style='background: #f8f9fa;'>
                        <td style='padding: 12px; border: 1px solid #dee2e6; font-weight: bold;'>Reference No</td>
                        <td style='padding: 12px; border: 1px solid #dee2e6; color: #c4252a; font-weight: bold;'>{ticket.RefNo}</td>
                    </tr>
                    <tr>
                        <td style='padding: 12px; border: 1px solid #dee2e6; font-weight: bold;'>Password</td>
                        <td style='padding: 12px; border: 1px solid #dee2e6;'>{plainPassword}</td>
                    </tr>
                    <tr style='background: #f8f9fa;'>
                        <td style='padding: 12px; border: 1px solid #dee2e6; font-weight: bold;'>Submitted On</td>
                        <td style='padding: 12px; border: 1px solid #dee2e6;'>{DateTime.Now:dd/MM/yyyy HH:mm}</td>
                    </tr>
                </table>
                <p>You can use your <strong>Reference No</strong> and <strong>Password</strong> to track the status of your ticket at any time.</p>
                <p style='color: #6c757d; font-size: 13px;'>Please do not share your password with anyone.</p>
                <hr style='border: none; border-top: 1px solid #dee2e6; margin: 20px 0;'/>
                <p style='color: #6c757d; font-size: 12px;'>This is an automated message. Please do not reply to this email.</p>
            </div>";

                await _emailService.SendEmailAsync(reporter.email1.Trim(), subject, body);
            }
            catch (Exception ex)
            {
                // Log but don't fail — ticket is already saved
            }
        }

        // دالة مساعدة لتحديد نوع الملف
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".wav" => "audio/wav",
                ".mp3" => "audio/mpeg",
                ".msg" => "application/vnd.ms-outlook",
                _ => "application/octet-stream"
            };
        }
    }
    }