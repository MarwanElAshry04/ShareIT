using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using ShareIT.Data;
using ShareIT.Models;
using ShareIT.Models.ViewModel;
using ShareIT.Services;
using System.Text;

namespace ShareIT.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IEmailService _emailService;
        private readonly TimelineService _timelineService; // ✅ Add this

        public SearchController(
            ApplicationDbContext context,
            IWebHostEnvironment hostingEnvironment,
            IEmailService emailService,
            TimelineService timelineService) // ✅ Add this
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _emailService = emailService;
            _timelineService = timelineService; // ✅ Add this
        }

        // GET: /Search
        public IActionResult Index()
        {
            var model = new TicketSearchModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TicketSearchModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var encodedPassword = EncodePasswordToBase64(model.Password);

            var ticket = await _context.Tickets
                .Include(c => c.Reporter)
                .Include(c => c.TicketType)
                .Include(c => c.TicketChats)
                .Include(c => c.TicketAttachments)
                .FirstOrDefaultAsync(c => c.RefNo == model.TicketRef
                                       && c.password == encodedPassword);

            if (ticket != null)
            {
                // ✅ Load timeline from DB instead of generating it
                var timeline = await _timelineService.GetTimelineAsync(ticket.Id);

                // Get chat messages (public only)
                var chatMessages = await _context.Chats
                    .Where(c => c.compId == ticket.Id )
                    .OrderByDescending(c => c.CreatedOn)
                    .Take(10)
                    .ToListAsync();

                model.TicketDetails = new TicketDetailsViewModel
                {
                    Ticket = ticket,
                    Timeline = timeline,       // ✅ from DB
                    RecentMessages = chatMessages,
                    Attachments = ticket.TicketAttachments?.ToList()
                };

                model.Message = "Ticket found successfully!";
                model.MessageType = "success";
                model.ShowForgetPasswordLink = false;
                model.ShowTicketDetails = true;

                HttpContext.Session.SetInt32("ChatTicketId", ticket.Id);
            }
            else
            {
                // ... your existing not-found logic ...
            }

            model.Password = "";
            ModelState.Clear();
            return View(model);
        }
        //private List<TimelineEvent> GetTicketTimeline(Ticket ticket)
        //{
        //    var timeline = new List<TimelineEvent>();

        //    // Ticket creation
        //    timeline.Add(new TimelineEvent
        //    {
        //        Date = (DateTime)ticket.CreatedOn,
        //        Title = "Ticket Submitted",
        //        Description = "Ticket was submitted to the system",
        //        Icon = "fas fa-plus-circle",
        //        Color = "danger"
        //    });

        //    // Status updates
        //    if (!string.IsNullOrEmpty(ticket.validation))
        //    {
        //        timeline.Add(new TimelineEvent
        //        {
        //            Date = (DateTime)(ticket.UpdatedOn ?? ticket.CreatedOn),
        //            Title = "Status Updated",
        //            Description = $"Status: {ticket.validation}",
        //            Icon = "fas fa-sync-alt",
        //            Color = "warning"
        //        });
        //    }

        //    // Add more timeline events as needed
        //    // For example: investigator assignments, messages, etc.

        //    return timeline;
        //}

      
        public async Task<IActionResult> FollowTicket(int id)
        {
            try
            {
                // Check if session is available
                if (HttpContext.Session != null)
                {
                    HttpContext.Session.SetInt32("TICKET_ID", id);
                }
                else
                {
                    // Fallback to TempData
                    TempData["TICKET_ID"] = id;
                    TempData.Keep("TICKET_ID"); // Keep for next request
                }

                // Store in ViewData for current request
                ViewData["TicketId"] = id;

                return RedirectToAction("Index", "FollowTicket", new { id = id });
            }
            catch (InvalidOperationException ex)
            {
              
                return RedirectToAction("Index", "FollowTicket", new { id = id });
            }
        }
     

        private string EncodePasswordToBase64(string password)
        {
            try
            {
                byte[] encData_byte = Encoding.UTF8.GetBytes(password);
                return Convert.ToBase64String(encData_byte);
            }
            catch (Exception)
            {
                return password;
            }
        }

        private string GenerateRandomPassword(int length)
        {
            const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                chars[i] = allowedChars[random.Next(allowedChars.Length)];
            }

            return new string(chars);
        }

        private async Task SendPasswordEmail(string toEmail, string name, string newPassword)
        {
            // Implement email sending logic
            var subject = "Password Recovery - ShareIT System";
            var body = $"Dear {name},<br/><br/>" +
                      $"Your new password is: <strong>{newPassword}</strong><br/><br/>" +
                      $"Please use this password to access your ticket.<br/><br/>" +
                      $"Thank you,<br/>ShareIT Compliance Team";

            // Use your email service here
            // await _emailSender.SendEmailAsync(toEmail, subject, body);

            // For now, log to console
            Console.WriteLine($"Sending email to {toEmail} with new password: {newPassword}");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(ChatViewModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Message))
                    return Json(new { success = false, message = "Message cannot be empty" });

                var ticket = await _context.Tickets
                    .FirstOrDefaultAsync(c => c.Id == model.TicketId);

                if (ticket == null)
                    return Json(new { success = false, message = "Ticket not found" });

                var chat = new Chats
                {
                    compId = model.TicketId,
                    Comment = model.Message,
                    flag = "user",
                    CreatedOn = DateTime.Now,
                    CreatedBy = ticket.RefNo
                };

                _context.Chats.Add(chat);
                await _context.SaveChangesAsync();

                // ✅ Save to timeline so user sees it when tracking
                await _timelineService.AddEventAsync(
                    ticketId: ticket.Id,
                    title: "New Message Sent",
                    description: "Reporter sent a new message",
                    icon: "fas fa-comment",
                    color: "info",
                    createdBy: ticket.RefNo
                );

                await NotifyComplianceTeam(ticket, model.Message);

                return Json(new
                {
                    success = true,
                    message = "Message sent successfully",
                    chatId = chat.Id,
                    time = DateTime.Now.ToString("HH:mm"),
                    date = DateTime.Now.ToString("dd MMM yyyy")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error sending message" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChatMessages(int ticketId)
        {
            try
            {
                var messages = await _context.Chats
                    .Where(c => c.compId == ticketId)
                    .OrderBy(c => c.CreatedOn)
                    .Select(c => new ChatMessageViewModel
                    {
                        Id = c.Id,
                        Comment = c.Comment,
                        Sender = c.flag == "user" ? "You" : "Compliance Team",
                        SenderType = c.flag == "user" ? "user" : "compliance",
                        CreatedOn =(DateTime) c.CreatedOn
                    })
                    .ToListAsync();

                return Json(new { success = true, messages });
            }
            catch (Exception)
            {
                return Json(new { success = false, messages = new List<ChatMessageViewModel>() });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadChatAttachment(int ticketId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "No file selected" });
                }

                // Validate file size (max 10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return Json(new { success = false, message = "File size cannot exceed 10MB" });
                }

                // Validate file extensions
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    return Json(new { success = false, message = "File type not allowed" });
                }

                // Generate unique file name
                var fileName = $"{Guid.NewGuid()}{extension}";
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "chat");

                // Create directory if not exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Save to database
                var attachment = new Attachment
                {
                    compId = ticketId,
                    filePath = $"/uploads/chat/{fileName}",
                    CreatedOn = DateTime.Now,
                    CreatedBy = "User",
                    attachmentType = "chat"
                };

                _context.Attachments.Add(attachment);
                await _context.SaveChangesAsync();

                // Create chat message for the attachment
                var chat = new Chats
                {
                    compId = ticketId,
                    Comment = $"Sent file: {file.FileName}",
                    flag = "user",
                    CreatedOn = DateTime.Now,
                    CreatedBy = "User"
                };

                _context.Chats.Add(chat);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "File uploaded successfully",
                    fileName = file.FileName,
                    fileUrl = $"/uploads/chat/{fileName}"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error uploading file" });
            }
        }

     

        private async Task NotifyComplianceTeam(Ticket ticket, string message)
        {
            try
            {
                // Update ticket timestamp
                ticket.UpdatedOn = DateTime.Now;
                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();

                // Email the compliance team
                var subject = $"New Message on Ticket – Ref: {ticket.RefNo}";
                var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto;'>
                <h3 style='color: #c4252a;'>New Message Received</h3>
                <p>A reporter has sent a new message on ticket <strong>{ticket.RefNo}</strong>.</p>
                <table style='border-collapse: collapse; width: 100%; margin: 20px 0;'>
                    <tr style='background: #f8f9fa;'>
                        <td style='padding: 12px; border: 1px solid #dee2e6; font-weight: bold;'>Reference No</td>
                        <td style='padding: 12px; border: 1px solid #dee2e6;'>{ticket.RefNo}</td>
                    </tr>
                    <tr>
                        <td style='padding: 12px; border: 1px solid #dee2e6; font-weight: bold;'>Message</td>
                        <td style='padding: 12px; border: 1px solid #dee2e6;'>{message}</td>
                    </tr>
                    <tr style='background: #f8f9fa;'>
                        <td style='padding: 12px; border: 1px solid #dee2e6; font-weight: bold;'>Sent At</td>
                        <td style='padding: 12px; border: 1px solid #dee2e6;'>{DateTime.Now:dd/MM/yyyy HH:mm}</td>
                    </tr>
                </table>
                <p><a href='https://compliance.elsewedy.com/Ticket/Details/{ticket.Id}' 
                      style='color: #c4252a;'>View Ticket</a></p>
            </div>";

                if (_emailService != null)
                    await _emailService.SendEmailAsync("compliance-int@elsewedy.com", subject, body);
            }
            catch (Exception ex)
            {
                // Log but don't fail the chat message
                Console.WriteLine($"Notify team failed: {ex.Message}");
            }
        }     
        // Update your Index action to include chat messages

    }
}
