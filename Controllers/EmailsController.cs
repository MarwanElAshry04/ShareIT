using Microsoft.AspNetCore.Mvc;
using ShareIT.Data;
using ShareIT.Models;
using ShareIT.Models.ViewModel;
using System.Text.Json;

namespace ShareIT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<EmailsController> _logger;

        public EmailsController(
            ApplicationDbContext context,
            IWebHostEnvironment env,
            ILogger<EmailsController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }
        [HttpPost("CreateTicketFromEmail")]
        public async Task<ActionResult> CreateTicketFromEmail([FromBody] EmailDto emailDto)
        {
            try
            {
                // 1. حفظ البيانات مؤقتاً
                var tempEmail = new Models.TempEmailModel
                {
                    OutlookItemId = emailDto.OutlookItemId,
                    Subject = emailDto.Subject,
                    SenderEmail = emailDto.Sender?.Email,
                    SenderName = emailDto.Sender?.Name,
                    ToRecipients = JsonSerializer.Serialize(emailDto.ToRecipients),
                    CcRecipients = JsonSerializer.Serialize(emailDto.CcRecipients),
                    BodyPreview = emailDto.BodyPreview,
                    FullBody = emailDto.FullBody,
                    ConversationId = emailDto.ConversationId,
                    DateTimeCreated = emailDto.DateTimeCreated,
                    ReceivedAt = DateTime.Now,
                    Attachments = JsonSerializer.Serialize(emailDto.Attachments),
                    IsProcessed = false
                };

                _context.TempEmails.Add(tempEmail);
                await _context.SaveChangesAsync();

                // 2. إنشاء URL للتوجيه إلى صفحة إنشاء الشكوى
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var redirectUrl = $"{baseUrl}/Ticket/CreateFromEmail/{tempEmail.Id}";

                // 3. إرجاع رابط التوجيه
                return Ok(new
                {
                    success = true,
                    tempEmailId = tempEmail.Id,
                    redirectUrl = redirectUrl,
                    message = "تم استقبال البريد بنجاح، يرجى إكمال بيانات الشكوى"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket from email");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // إضافة API للتحقق من حالة البريد
        [HttpGet("CheckEmailStatus/{id}")]
        public async Task<ActionResult> CheckEmailStatus(int id)
        {
            try
            {
                var tempEmail = await _context.TempEmails.FindAsync(id);
                if (tempEmail == null)
                {
                    return NotFound(new { success = false, message = "Email not found" });
                }

                return Ok(new
                {
                    success = true,
                    isProcessed = tempEmail.IsProcessed,
                    processedAt = tempEmail.IsProcessed ? tempEmail.ReceivedAt : (DateTime?)null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email status");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}