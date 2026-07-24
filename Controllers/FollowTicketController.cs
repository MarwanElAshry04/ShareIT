using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareIT.Data;
using ShareIT.Models;
using ShareIT.Models.ViewModel;
using ShareIT.Services;
using System.Diagnostics;

namespace ShareIT.Controllers
{
    [Authorize]

    public class FollowTicketController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IEmailService _emailService;
        private readonly ILogger<FollowTicketController> _logger;
        private readonly TimelineService _timelineService;

        public FollowTicketController(
            ApplicationDbContext context,
            IWebHostEnvironment hostingEnvironment,
            IEmailService emailService,
            ILogger<FollowTicketController> logger, TimelineService timelineService)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _emailService = emailService;
            _logger = logger;
            _timelineService = timelineService;

        }

        // GET: /FollowTicket/AllTickets
        // صفحة عرض جميع الشكاوى
        public async Task<IActionResult> AllTickets(
        string searchTerm = "",
        string? status = "",        // For Status field (Initiate, In-Process, Closed)
        string? kind = "",          // For Kind field (Complaint, Suggestion, Feedback)
        string? validation = "",    // For validation field (Valid, Invalid)
        int? typeId = null,
        string dateFrom = "",
        string dateTo = "",
        string priority = "",
        string assignedTo = "",
        string reporterType = "",
        int page = 1,
        int pageSize = 10)
        {
            try
            {
                // Base query with includes
                var query = _context.Tickets
                    .Include(c => c.Reporter)
                    .Include(c => c.TicketType)
                    .Include(c => c.TicketChats)
                        .OrderByDescending(c => c.CreatedOn)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(c =>
                        c.RefNo.Contains(searchTerm) ||
                        c.Desc.Contains(searchTerm) ||
                        (c.Reporter != null && c.Reporter.name.Contains(searchTerm)) ||
                        c.ConcerningCompany.Contains(searchTerm) ||
                        c.ConcerningDepartment.Contains(searchTerm) ||
                        c.ConcerningPerson.Contains(searchTerm)
                    );
                }

                // Filter by Status (Initiate, In-Process, Closed)
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(c => c.Status == status);
                }

                // Filter by Kind (Complaint, Suggestion, Feedback)
                if (!string.IsNullOrEmpty(kind) && Enum.TryParse<TicketKind>(kind, out var kindFilter))
                {
                    query = query.Where(c => c.Kind == kindFilter);
                }

                // Filter by Validation (Valid, Invalid)
                if (!string.IsNullOrEmpty(validation))
                {
                    query = query.Where(c => c.validation == validation);
                }

                if (!string.IsNullOrEmpty(assignedTo))
                {
                    query = query.Where(c => c.forwarding == assignedTo);
                }

                if (!string.IsNullOrEmpty(reporterType))
                {
                    if (reporterType == "Anonymous")
                        query = query.Where(c => c.Reporter != null && c.Reporter.reporterType == "Anonymous");
                    else if (reporterType == "Disclosed")
                        query = query.Where(c => c.Reporter != null && c.Reporter.reporterType != "Disclosed");
                }

                if (!string.IsNullOrEmpty(dateFrom))
                {
                    if (DateTime.TryParse(dateFrom, out var fromDate))
                    {
                        query = query.Where(c => c.CreatedOn >= fromDate);
                    }
                }

                if (!string.IsNullOrEmpty(dateTo))
                {
                    if (DateTime.TryParse(dateTo, out var toDate))
                    {
                        toDate = toDate.AddDays(1);
                        query = query.Where(c => c.CreatedOn <= toDate);
                    }
                }

                // Get statistics
                var statistics = new TicketStatisticsViewModel
                {
                    TotalTickets = await _context.Tickets.CountAsync(),
                    Initiate = await _context.Tickets.Where(c => c.Status == "Initiate").CountAsync(),
                    InProcess = await _context.Tickets.Where(c => c.Status == "In-Process").CountAsync(),
                    Closed = await _context.Tickets.Where(c => c.Status == "Closed").CountAsync(),
                    ValidTickets = await _context.Tickets.Where(c => c.validation == "Valid").CountAsync(),
                    InvalidTickets = await _context.Tickets.Where(c => c.validation == "Invalid").CountAsync(),
                    ComplaintCount = await _context.Tickets.Where(c => c.Kind == TicketKind.Complaint).CountAsync(),
                    SuggestionCount = await _context.Tickets.Where(c => c.Kind == TicketKind.Suggestion).CountAsync(),
                    FeedbackCount = await _context.Tickets.Where(c => c.Kind == TicketKind.Feedback).CountAsync(),
                    AnonymousReports = await _context.Tickets
                        .Include(c => c.Reporter)
                        .Where(c => c.Reporter != null && c.Reporter.reporterType == "Anonymous")
                        .CountAsync(),
                    DisclosedReports = await _context.Tickets
                        .Include(c => c.Reporter)
                        .Where(c => c.Reporter != null && c.Reporter.reporterType != "Anonymous")
                        .CountAsync()
                };

                // Calculate average resolution time for closed cases
                var closedTickets = await _context.Tickets
                    .Where(c => c.Status == "Closed" && c.closureDate.HasValue && c.CreatedOn.HasValue)
                    .ToListAsync();

                if (closedTickets.Any())
                {
                    statistics.AvgResolutionDays = closedTickets
                        .Average(c => (c.closureDate.Value - c.CreatedOn.Value).TotalDays);
                }

                // Get monthly trends for the last 6 months
                statistics.MonthlyTrends = await GetMonthlyTrends();
                statistics.TypeDistribution = await GetTypeDistribution();

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var tickets = await query
                    .OrderByDescending(c => c.CreatedOn)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Get assigned departments for filter
                var assignedDepartments = await _context.Tickets
                    .Where(c => !string.IsNullOrEmpty(c.forwarding))
                    .Select(c => c.forwarding)
                    .Distinct()
                    .ToListAsync();

                // Prepare ViewModel
                var model = new TicketListViewModel
                {
                    Tickets = tickets,
                    Statistics = statistics,
                    SearchTerm = searchTerm,
                    Status = status,
                    Kind = kind,
                    Validation = validation,  // Add this to your ViewModel
                    TypeId = typeId,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Priority = priority,
                    AssignedTo = assignedTo,
                    ReporterType = reporterType,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),

                    // Get lookup data for filters
                    TicketTypes = await _context.TicketTypes.ToListAsync(),
                    AssignedDepartments = assignedDepartments ?? new List<string>(),
                    Priorities = new List<string> { "High", "Medium", "Low" },
                    ReporterTypes = new List<string> { "All", "Disclosed", "Anonymous" }
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tickets list");
                TempData["ErrorMessage"] = "An error occurred while loading tickets.";
                return View(new TicketListViewModel());
            }
        }

        // GET: /FollowTicket/MyAssignments
        // عرض الشكاوى المسندة إلى قسم المشرف الحالي
        public async Task<IActionResult> MyAssignments()
        {
            try
            {
                var currentUser = User.Identity.Name;

                // Get user's department - you need to implement this based on your user management
                var userDepartment = await GetUserDepartment(currentUser);

                var tickets = await _context.Tickets
                    .Include(c => c.Reporter)
                    .Include(c => c.TicketType)
                    .Where(c => c.forwarding == userDepartment || c.UpdatedBy == currentUser)
                    .OrderByDescending(c => c.CreatedOn)
                    .ToListAsync();

                return View(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading assigned tickets");
                TempData["ErrorMessage"] = "An error occurred while loading your assignments.";
                return RedirectToAction(nameof(AllTickets));
            }
        }

        // GET: /FollowTicket/PendingApproval
        // عرض الشكاوى المعلقة للموافقة
        public async Task<IActionResult> PendingApproval()
        {
            try
            {
                var tickets = await _context.Tickets
                    .Include(c => c.Reporter)
                    .Include(c => c.TicketType)
                    .Where(c => c.Status == "Initiate") // Open status
                    .OrderByDescending(c => c.CreatedOn)
                    .ToListAsync();

                return View(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading pending tickets");
                TempData["ErrorMessage"] = "An error occurred while loading pending tickets.";
                return RedirectToAction(nameof(AllTickets));
            }
        }

        // GET: /FollowTicket/HighPriority
        // عرض الشكاوى عالية الأولوية
        public async Task<IActionResult> HighPriority()
        {
            try
            {
                var tickets = await _context.Tickets
                    .Include(c => c.Reporter)
                    .Include(c => c.TicketType)
                    .Where(c =>c.Status != "Closed")
                    .OrderByDescending(c => c.CreatedOn)
                    .ToListAsync();

                return View(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading high priority tickets");
                TempData["ErrorMessage"] = "An error occurred while loading high priority tickets.";
                return RedirectToAction(nameof(AllTickets));
            }
        }

        // GET: /FollowTicket/ClosedToday
        // عرض الشكاوى المغلقة اليوم
        public async Task<IActionResult> ClosedToday()
        {
            try
            {
                var today = DateTime.Today;
                var tickets = await _context.Tickets
                    .Include(c => c.Reporter)
                    .Include(c => c.TicketType)
                    .Where(c => c.Status == "Closed" && c.closureDate.HasValue && c.closureDate.Value.Date == today)
                    .OrderByDescending(c => c.closureDate)
                    .ToListAsync();

                return View(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading closed tickets");
                TempData["ErrorMessage"] = "An error occurred while loading closed tickets.";
                return RedirectToAction(nameof(AllTickets));
            }
        }

        // GET: /FollowTicket/ExportTickets
        public async Task<IActionResult> ExportTickets(string searchTerm = "", int? statusId = null)
        {
            try
            {
                var query = _context.Tickets
                    .Include(c => c.Reporter)
                    .Include(c => c.TicketType)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(c => c.RefNo.Contains(searchTerm) || c.Desc.Contains(searchTerm));
                }

                //if (statusId.HasValue && statusId.Value > 0)
                //{
                //    query = query.Where(c => c.ComStatusId == statusId.Value);
                //}

                var tickets = await query
                    .OrderByDescending(c => c.CreatedOn)
                    .ToListAsync();

                // Generate CSV
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Ref No,Date,Type,Status,Priority,Reporter,Concerning Company,Concerning Department,Assigned To,Description");

                foreach (var c in tickets)
                {
                    var reporterName = c.Reporter?.reporterType == "Anonymous" ? "Anonymous" : c.Reporter?.name ?? "N/A";
                    sb.AppendLine($"\"{c.RefNo}\",\"{c.CreatedOn:dd/MM/yyyy}\",\"{c.TicketType?.ComplainType}\",\"{c.Status}\",\"{reporterName}\",\"{c.ConcerningCompany}\",\"{c.ConcerningDepartment}\",\"{c.forwarding}\",\"{c.Desc?.Replace("\"", "\"\"")}\"");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                return File(bytes, "text/csv", $"tickets_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting tickets");
                TempData["ErrorMessage"] = "An error occurred while exporting tickets.";
                return RedirectToAction(nameof(AllTickets));
            }
        }

        // GET: /FollowTicket/GetStatistics
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var statistics = new
                {
                    total = await _context.Tickets.CountAsync(),
                    Initiate = await _context.Tickets.Where(c => c.Status == "Initiate").CountAsync(),
                    InProcess = await _context.Tickets.Where(c => c.Status == "In-Process").CountAsync(),
                    closed = await _context.Tickets.Where(c => c.Status == "Closed").CountAsync(),
                    ValidTickets = _context.Tickets.Where(c => c.validation == "Valid").Count(),
                    InvalidTickets = _context.Tickets.Where(c => c.validation == "Invalid").Count(),
                    today = await _context.Tickets
                        .Where(c => c.CreatedOn == DateTime.Today)
                        .CountAsync()
                };

                return Json(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics");
                return Json(new { error = "Failed to load statistics" });
            }
        }

        #region Private Methods

        private async Task<string> GetUserDepartment(string username)
        {
            // Implement this based on your user management system
            // This is a placeholder - you need to get the actual department from your user database
            return await Task.FromResult("Compliance Department");
        }

        private async Task<List<ChartData>> GetMonthlyTrends()
        {
            var trends = new List<ChartData>();
            var today = DateTime.Today;

            for (int i = 5; i >= 0; i--)
            {
                var month = today.AddMonths(-i);
                var count = await _context.Tickets
                    .Where(c => c.CreatedOn.HasValue
                             && c.CreatedOn.Value.Month == month.Month
                             && c.CreatedOn.Value.Year == month.Year)
                    .CountAsync();


                trends.Add(new ChartData
                {
                    Label = month.ToString("MMM yyyy"),
                    Value = count
                });
            }

            return trends;
        }

        private async Task<List<ChartData>> GetTypeDistribution()
        {
            var distribution = await _context.Tickets
                .Include(c => c.TicketType)
                .GroupBy(c => c.TicketType!.ComplainType)
                .Select(g => new ChartData
                {
                    Label = g.Key ?? "Unknown",
                    Value = g.Count()
                })
                .OrderByDescending(d => d.Value)
                .Take(5)
                .ToListAsync();

            return distribution;
        }

        private async Task SendCommentNotificationEmail(Ticket ticket, string comment)
        {
            if (_emailService == null) return;

            if (ticket.Reporter == null || string.IsNullOrEmpty(ticket.Reporter.email1))
                return;

            var subject = ticket.langFlag == "A"
                ? $"تعليق جديد على الشكوى رقم {ticket.RefNo}"
                : $"New Comment on Ticket #{ticket.RefNo}";

            var body = ticket.langFlag == "A"
                ? $@"
                    <div dir='rtl'>
                        <h3>عزيزي/عزيزتي،</h3>
                        <p>يرجى العلم بأنك تلقيت تعليقاً جديداً من إدارة الالتزام بخصوص شكواك رقم <strong>{ticket.RefNo}</strong></p>
                        <p>للاطلاع على التفاصيل، يرجى زيارة نظام الشكاوى.</p>
                        <hr />
                        <p><small>هذه رسالة آلية، يرجى عدم الرد عليها.</small></p>
                    </div>"
                : $@"
                    <div>
                        <h3>Dear User,</h3>
                        <p>You have received a new comment from Compliance Admin regarding your ticket <strong>{ticket.RefNo}</strong></p>
                        <p>Please check the Compliance System for details.</p>
                        <hr />
                        <p><small>This is an automated message, please do not reply.</small></p>
                    </div>";

            await _emailService.SendEmailAsync(ticket.Reporter.email1, subject, body);
        }

        private async Task NotifyAdminsNewAttachment(string refNo, string fileName)
        {
            if (_emailService == null) return;

            var adminEmails = new List<string>
            {
                "mennatullah.magdy@elsewedy.com",
                "Ahmed.Abdelmegeed@elsewedy.com"
            };

            var subject = $"New Attachment Uploaded - Ticket {refNo}";
            var body = $@"
                <div>
                    <h3>New Attachment Uploaded</h3>
                    <p>A new attachment has been uploaded by Admin for ticket <strong>{refNo}</strong></p>
                    <p><strong>File:</strong> {fileName}</p>
                    <p>Please check the admin panel for details.</p>
                    <hr />
                    <p><small>This is an automated message.</small></p>
                </div>";

            foreach (var email in adminEmails)
            {
                await _emailService.SendEmailAsync(email, subject, body);
            }
        }

        private async Task SendForwardingEmails(Ticket ticket, string departmentName, string toEmail, string ccEmail, string message)
        {
            if (_emailService == null) return;

            var toEmails = toEmail.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var ccEmails = ccEmail?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

            var subject = $"Ticket Forwarded - {ticket.RefNo}";

            // Email to department
            var departmentBody = $@"
                <div>
                    <h3>Ticket Forwarded for Investigation</h3>
                    <p>A ticket has been forwarded to <strong>{departmentName}</strong> for investigation.</p>
                    <p><strong>Reference No:</strong> {ticket.RefNo}</p>
                    <p><strong>Message:</strong> {message}</p>
                    <p>Please review the ticket in the admin panel.</p>
                    <hr />
                    <p><small>This is an automated message.</small></p>
                </div>";

            foreach (var email in toEmails)
            {
                if (!string.IsNullOrWhiteSpace(email))
                    await _emailService.SendEmailAsync(email.Trim(), subject, departmentBody);
            }

            foreach (var email in ccEmails)
            {
                if (!string.IsNullOrWhiteSpace(email))
                    await _emailService.SendEmailAsync(email.Trim(), subject, departmentBody);
            }

            // Email to reporter (if not anonymous)
            if (ticket.Reporter?.reporterType != "Anonymous" && !string.IsNullOrEmpty(ticket.Reporter?.email1))
            {
                var reporterSubject = ticket.langFlag == "A"
                    ? $"تحديث على الشكوى رقم {ticket.RefNo}"
                    : $"Update on Ticket #{ticket.RefNo}";

                var reporterBody = ticket.langFlag == "A"
                    ? $@"
                        <div dir='rtl'>
                            <h3>عزيزي/عزيزتي،</h3>
                            <p>نود إعلامك بأن شكواك رقم <strong>{ticket.RefNo}</strong> قيد التحقيق من قبل <strong>{departmentName}</strong>.</p>
                            <p>لمتابعة حالة الشكوى، يرجى زيارة نظام الشكاوى.</p>
                            <hr />
                            <p><small>هذه رسالة آلية، يرجى عدم الرد عليها.</small></p>
                        </div>"
                    : $@"
                        <div>
                            <h3>Dear User,</h3>
                            <p>Your ticket <strong>{ticket.RefNo}</strong> is now under investigation by <strong>{departmentName}</strong>.</p>
                            <p>Please check the Compliance System for updates.</p>
                            <hr />
                            <p><small>This is an automated message.</small></p>
                        </div>";

                await _emailService.SendEmailAsync(ticket.Reporter.email1, reporterSubject, reporterBody);
            }
        }

        #endregion

        // باقي الـ Actions الموجودة (Index, AddComment, AddInternalComment, UpdateStatus, UploadAttachment, etc.)
        // GET: /Admin/FollowTicket/Index/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTicketData(
        int ticketId,
        int? ticketTypeId,
        string? concerningCompany,
        string? concerningDepartment,
        string? concerningPerson,
        string? concerningOther)
        {
            try
            {
                var ticket = await _context.Tickets
                    .FirstOrDefaultAsync(c => c.Id == ticketId);

                if (ticket == null)
                {
                    TempData["ErrorMessage"] = "Ticket not found.";
                    return RedirectToAction("Index", "Dashboard");
                }

                // Build description of what changed
                var changes = new List<string>();
                if (ticket.TicketTypeId != ticketTypeId) changes.Add("Ticket Type");
                if (ticket.ConcerningCompany != concerningCompany) changes.Add("Concerning Company");
                if (ticket.ConcerningDepartment != concerningDepartment) changes.Add("Concerning Department");
                if (ticket.ConcerningPerson != concerningPerson) changes.Add("Concerning Person");
                if (ticket.ConcerningOther != concerningOther) changes.Add("Concerning Other");

                ticket.TicketTypeId = ticketTypeId;
                ticket.ConcerningCompany = concerningCompany;
                ticket.ConcerningDepartment = concerningDepartment;
                ticket.ConcerningPerson = concerningPerson;
                ticket.ConcerningOther = concerningOther;
                ticket.UpdatedBy = User.Identity?.Name ?? "Admin";
                ticket.UpdatedOn = DateTime.Now;

                await _context.SaveChangesAsync();

                // ✅ Timeline
                await _timelineService.AddEventAsync(
                    ticketId: ticketId,
                    title: "Ticket Data Updated",
                    description: changes.Any()
                        ? $"Updated fields: {string.Join(", ", changes)} — by {User.Identity?.Name}"
                        : $"Ticket details reviewed by {User.Identity?.Name}",
                    icon: "fas fa-edit",
                    color: "info",
                    createdBy: User.Identity?.Name ?? "Admin"
                );

                TempData["SuccessMessage"] = "Ticket data updated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket data");
                TempData["ErrorMessage"] = "An error occurred while updating.";
            }

            return RedirectToAction("Index", new { id = ticketId, activeTab = "main" });
        }
        public async Task<IActionResult> Index(int id, string activeTab = "main")
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(c => c.Reporter)
                    .Include(c => c.TicketType)
                    .Include(c => c.TicketChats)
                    .Include(c => c.TicketAttachments)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (ticket == null)
                {
                    TempData["ErrorMessage"] = "Ticket not found.";
                    return RedirectToAction("Index", "Dashboard");
                }

                // ✅ Add this line
                ViewBag.TicketTypes = await _context.TicketTypes.ToListAsync();
                ViewBag.Company = await _context.Companies.ToListAsync();
                ViewBag.Department = await _context.Departments.ToListAsync();

                var model = await BuildViewModel(ticket, activeTab);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ticket follow-up");
                TempData["ErrorMessage"] = "An error occurred while loading the ticket.";
                return RedirectToAction("Index", "Dashboard");
            }
        }
        // POST: /Admin/FollowTicket/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(TicketSearchModel model, int ticketId, string comment)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(comment))
                {
                    TempData["ErrorMessage"] = "Comment cannot be empty.";
                    return RedirectToAction("Index", new { id = ticketId, activeTab = "chats" });
                }

                var ticket = await _context.Tickets
                    .Include(c => c.Reporter)
                    .FirstOrDefaultAsync(c => c.Id == ticketId);

                if (ticket == null)
                    return NotFound();

                var chat = new Chats
                {
                    compId = ticketId,
                    Comment = comment,
                    CreatedBy = User.Identity.Name,
                    CreatedOn = DateTime.Now,
                    flag = ticket.langFlag == "A" ? "A" : "E"
                };

                _context.Chats.Add(chat);
                await _context.SaveChangesAsync();

                // ✅ Timeline
                await _timelineService.AddEventAsync(
                    ticketId: ticketId,
                    title: "Public Comment Added",
                    description: $"{User.Identity?.Name} posted a comment to the reporter",
                    icon: "fas fa-comment",
                    color: "primary",
                    createdBy: User.Identity?.Name ?? "Admin"
                );

                if (ticket.Reporter?.reporterType != "Anonymous" && !string.IsNullOrEmpty(ticket.Reporter?.email1))
                    await SendCommentNotificationEmail(ticket, comment);

                TempData["SuccessMessage"] = "Comment added successfully.";
                return RedirectToAction("Index", new { id = ticketId, activeTab = "chats" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment");
                TempData["ErrorMessage"] = "Error adding comment.";
                return RedirectToAction("Index", new { id = ticketId });
            }
        }
        // POST: /Admin/FollowTicket/AddInternalComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddInternalComment(int ticketId, string internalComment)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(internalComment))
                {
                    TempData["ErrorMessage"] = "Comment cannot be empty.";
                    return RedirectToAction("Index", new { id = ticketId, activeTab = "internal" });
                }

                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket != null)
                {
                    ticket.Status = "In-Process";
                    ticket.UpdatedOn = DateTime.Now;
                    ticket.UpdatedBy = User.Identity.Name;
                    _context.Tickets.Update(ticket);
                }

                var chat = new Chats
                {
                    compId = ticketId,
                    Comment = internalComment,
                    CreatedBy = User.Identity.Name,
                    CreatedOn = DateTime.Now,
                    flag = "I"
                };

                _context.Chats.Add(chat);
                await _context.SaveChangesAsync();

                // ✅ Timeline
                await _timelineService.AddEventAsync(
                    ticketId: ticketId,
                    title: "Internal Note Added",
                    description: $"Internal comment added by {User.Identity?.Name} — Status set to In-Process",
                    icon: "fas fa-lock",
                    color: "warning",
                    createdBy: User.Identity?.Name ?? "Admin"
                );

                TempData["SuccessMessage"] = "Internal comment added successfully.";
                return RedirectToAction("Index", new { id = ticketId, activeTab = "internal" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding internal comment");
                TempData["ErrorMessage"] = "Error adding internal comment.";
                return RedirectToAction("Index", new { id = ticketId });
            }
        }
        // POST: /Admin/FollowTicket/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int ticketId, string statusId, string validation, string finalRes)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                    return NotFound();

                var previousStatus = ticket.Status;

                ticket.finalRes = finalRes;
                ticket.UpdatedBy = User.Identity.Name;
                ticket.UpdatedOn = DateTime.Now;
                ticket.Status = statusId;

                if (statusId == "Closed")
                {
                    ticket.validation = validation;
                    ticket.closureDate = DateTime.Now;
                }
                else
                {
                    ticket.validation = null;
                    ticket.closureDate = null;
                }

                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();

                // ✅ Timeline — Status Changed
                await _timelineService.AddEventAsync(
                    ticketId: ticketId,
                    title: "Status Updated",
                    description: $"Status changed from '{previousStatus}' to '{statusId}' by {User.Identity?.Name}",
                    icon: "fas fa-sync-alt",
                    color: "warning",
                    createdBy: User.Identity?.Name ?? "Admin"
                );

                // ✅ Timeline — Extra event if Closed
                if (statusId == "Closed")
                {
                    await _timelineService.AddEventAsync(
                        ticketId: ticketId,
                        title: "Case Closed",
                        description: $"Closed by {User.Identity?.Name} — Validation: {validation}",
                        icon: "fas fa-check-circle",
                        color: "success",
                        createdBy: User.Identity?.Name ?? "Admin"
                    );
                }

                TempData["SuccessMessage"] = "Status updated successfully.";
                return RedirectToAction("Index", new { id = ticketId, activeTab = "final" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status");
                TempData["ErrorMessage"] = "Error updating status.";
                return RedirectToAction("Index", new { id = ticketId });
            }
        }
        // POST: /Admin/FollowTicket/UploadAttachment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAttachment(int ticketId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    TempData["ErrorMessage"] = "Please select a file to upload.";
                    return RedirectToAction("Index", new { id = ticketId, activeTab = "attachment" });
                }

                var allowedExtensions = new[] { ".docx", ".pdf", ".doc", ".wav", ".mp3", ".jpg", ".png", ".msg" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    TempData["ErrorMessage"] = "File type not allowed.";
                    return RedirectToAction("Index", new { id = ticketId, activeTab = "attachment" });
                }

                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                    return NotFound();

                var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, "CompFiles", ticket.RefNo);
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                var attachment = new Attachment
                {
                    compId = ticketId,
                    filePath = $"/CompFiles/{ticket.RefNo}/{fileName}",
                    CreatedOn = DateTime.Now,
                    CreatedBy = User.Identity.Name
                };

                _context.Attachments.Add(attachment);
                await _context.SaveChangesAsync();

                // ✅ Timeline
                await _timelineService.AddEventAsync(
                    ticketId: ticketId,
                    title: "Attachment Uploaded",
                    description: $"File '{fileName}' uploaded by {User.Identity?.Name}",
                    icon: "fas fa-paperclip",
                    color: "info",
                    createdBy: User.Identity?.Name ?? "Admin"
                );

                TempData["SuccessMessage"] = "File uploaded successfully.";
                return RedirectToAction("Index", new { id = ticketId, activeTab = "attachment" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                TempData["ErrorMessage"] = "Error uploading file.";
                return RedirectToAction("Index", new { id = ticketId });
            }
        }
        // POST: /Admin/FollowTicket/UploadInternalAttachment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadInternalAttachment(int ticketId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    TempData["ErrorMessage"] = "Please select a file to upload.";
                    return RedirectToAction("Index", new { id = ticketId, activeTab = "internal" });
                }

                // Validate file extension
                var allowedExtensions = new[] { ".docx", ".pdf", ".doc", ".wav", ".mp3", ".jpg", ".png", ".msg", ".xlsx" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    TempData["ErrorMessage"] = "File type not allowed. Allowed types: DOCX, PDF, DOC, WAV, MP3, JPG, PNG, MSG, XLSX";
                    return RedirectToAction("Index", new { id = ticketId, activeTab = "internal" });
                }

                // Get ticket reference
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                    return NotFound();

                // Create directory if not exists
                var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, "CompFiles", ticket.RefNo, "Admin");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                // Save file
                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Save to database
                var attachment = new Attachment
                {
                    compId = ticketId,
                    filePath = $"/CompFiles/{ticket.RefNo}/Admin/{fileName}",
                    CreatedOn = DateTime.Now,
                    CreatedBy = User.Identity.Name,
                    attachmentType = "Internal"
                };

                _context.Attachments.Add(attachment);
                    // ✅ Timeline — add after _context.SaveChangesAsync()
await _timelineService.AddEventAsync(
    ticketId: ticketId,
    title: "Internal File Uploaded",
    description: $"Internal file '{fileName}' uploaded by {User.Identity?.Name}",
    icon: "fas fa-file-upload",
    color: "secondary",
    createdBy: User.Identity?.Name ?? "Admin"
); ;
                await _context.SaveChangesAsync();

                // Notify other admins
                await NotifyAdminsNewAttachment(ticket.RefNo, fileName);

                TempData["SuccessMessage"] = "Internal file uploaded successfully.";
                return RedirectToAction("Index", new { id = ticketId, activeTab = "internal" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading internal file");
                TempData["ErrorMessage"] = "Error uploading file.";
                return RedirectToAction("Index", new { id = ticketId });
            }
        }

        // POST: /Admin/FollowTicket/DeleteAttachment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttachment(string filePath, int ticketId, bool isInternal = false)
        {
            try
            {
                var fullPath = Path.Combine(_hostingEnvironment.WebRootPath, filePath.TrimStart('/'));

                // Move to Deleted folder
                var deletedPath = Path.Combine(
                    _hostingEnvironment.WebRootPath,
                    "CompFiles",
                    "Deleted",
                    Path.GetFileName(filePath));

                Directory.CreateDirectory(Path.GetDirectoryName(deletedPath));

                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Move(fullPath, deletedPath);

                // Remove from database
                var attachment = await _context.Attachments
                    .FirstOrDefaultAsync(a => a.filePath == filePath);

                if (attachment != null)
                {
                    _context.Attachments.Remove(attachment);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file");
                return Json(new { success = false, error = ex.Message });
            }
        }

        // POST: /Admin/FollowTicket/ForwardTicket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForwardTicket(int ticketId, int departmentId, string toEmail, string ccEmail, string message)
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(c => c.Reporter)
                    .FirstOrDefaultAsync(c => c.Id == ticketId);

                if (ticket == null)
                    return NotFound();

                var department = await _context.Departments.FindAsync(departmentId);

                // Update forwarding info
                ticket.forwarding = department?.Name;
                ticket.toMails = toEmail;
                ticket.ccMails = ccEmail;
                ticket.messageMail = message;
                ticket.UpdatedBy = User.Identity.Name;
                ticket.UpdatedOn = DateTime.Now;

                _context.Tickets.Update(ticket);

                // Add to chat
                var chat = new Chats
                {
                    compId = ticketId,
                    Comment = $"Assigned to {department?.Name}",
                    CreatedBy = "Admin",
                    CreatedOn = DateTime.Now,
                    flag = ticket.langFlag == "A" ? "A" : "E"
                };

                _context.Chats.Add(chat);
                // ✅ Timeline — add after _context.SaveChangesAsync()
                await _timelineService.AddEventAsync(
                    ticketId: ticketId,
                    title: "Ticket Assigned",
                    description: $"Assigned to '{department?.Name}' by {User.Identity?.Name}",
                    icon: "fas fa-share",
                    color: "primary",
                    createdBy: User.Identity?.Name ?? "Admin"
                );
                await _context.SaveChangesAsync();

                // Send emails
                await SendForwardingEmails(ticket, department?.Name, toEmail, ccEmail, message);

                TempData["SuccessMessage"] = "Ticket forwarded successfully.";
                return RedirectToAction("Index", new { id = ticketId, activeTab = "forward" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding ticket");
                TempData["ErrorMessage"] = "Error forwarding ticket.";
                return RedirectToAction("Index", new { id = ticketId });
            }
        }

        // GET: /Admin/FollowTicket/DownloadFile
        public async Task<IActionResult> DownloadFile(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_hostingEnvironment.WebRootPath, filePath.TrimStart('/'));

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
                _logger.LogError(ex, "Error downloading file");
                return NotFound();
            }
        }

        // GET: /Admin/FollowTicket/GetUnreadCount
        public async Task<IActionResult> GetUnreadCount(int ticketId)
        {
            try
            {
                var count = await _context.Chats
                    .Where(c => c.compId == ticketId && c.flag != "I" && c.CreatedOn > DateTime.Now.AddHours(-24))
                    .CountAsync();

                return Json(count);
            }
            catch
            {
                return Json(0);
            }
        }

        #region Private Methods

        private async Task<TicketDetailsViewModel> BuildViewModel(Ticket ticket, string activeTab)
        {
            // Get timeline
            var timeline = await _timelineService.GetTimelineAsync(ticket.Id);

            // Get recent messages
            var recentMessages = await _context.Chats
                .Where(c => c.compId == ticket.Id && (c.flag == "E" || c.flag == "A"))
                .OrderByDescending(c => c.CreatedOn)
                .Take(10)
                .ToListAsync();

            // Get attachments
            var attachments = await _context.Attachments
                .Where(a => a.compId == ticket.Id)
                .ToListAsync();

            var model = new TicketDetailsViewModel
            {
                Ticket = ticket,
                Timeline = timeline,
                RecentMessages = recentMessages,
                Attachments = attachments
            };

            // Store active tab in ViewBag
            ViewBag.ActiveTab = activeTab;
            ViewBag.TicketId = ticket.Id;
            ViewBag.RefNo = ticket.RefNo;

            // Get departments for forwarding
            ViewBag.Departments = await _context.Departments
                .OrderBy(d => d.Name)
                .ToListAsync();

            // Get statuses
      

            // Get internal comments
            ViewBag.InternalComments = await _context.Chats
                .Where(c => c.compId == ticket.Id && c.flag == "I")
                .OrderByDescending(c => c.CreatedOn)
                .ToListAsync();

            // Get internal attachments
            ViewBag.InternalAttachments = await _context.Attachments
                .Where(a => a.compId == ticket.Id && a.attachmentType == "Internal")
                .ToListAsync();

            return new TicketDetailsViewModel
            {
                Ticket = ticket,
                Timeline = timeline,
                RecentMessages = ticket.TicketChats?
                                       .OrderByDescending(c => c.CreatedOn)
                                       .ToList() ?? new(),
                Attachments = ticket.TicketAttachments?
                                       .ToList() ?? new()
            };
        }        

        private List<TimelineEvent> GetTicketTimeline(Ticket ticket)
        {
            var timeline = new List<TimelineEvent>();

            // Ticket creation
            timeline.Add(new TimelineEvent
            {
                Date = (DateTime)ticket.CreatedOn,
                Title = "Ticket Submitted",
                Description = "Ticket was submitted to the system",
                Icon = "fas fa-plus-circle",
                Color = "danger"
            });

            // Status updates
            if (!string.IsNullOrEmpty(ticket.validation))
            {
                timeline.Add(new TimelineEvent
                {
                    Date = (DateTime)(ticket.UpdatedOn ?? ticket.CreatedOn),
                    Title = "Status Updated",
                    Description = $"Status: {ticket.Status}",
                    Icon = "fas fa-sync-alt",
                    Color = "warning"
                });
            }

            // If closed
            if (ticket.Status == "Closed")
            {
                timeline.Add(new TimelineEvent
                {
                    Date = (DateTime)(ticket.closureDate ?? ticket.UpdatedOn ?? ticket.CreatedOn),
                    Title = "Case Closed",
                    Description = $"Resolution: {ticket.validation}",
                    Icon = "fas fa-check-circle",
                    Color = "success"
                });
            }

            return timeline;
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

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

  

        #endregion
    }

}
