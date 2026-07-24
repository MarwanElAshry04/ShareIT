using Microsoft.EntityFrameworkCore;
using ShareIT.Data;
using ShareIT.Models;
using ShareIT.Models.ViewModel;
using System;

namespace ShareIT.Services
{
    public class TimelineService
    {
        private readonly ApplicationDbContext _context;

        public TimelineService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Save a new timeline event to DB
        public async Task AddEventAsync(int ticketId, string title,
            string description, string icon, string color,
            string createdBy = "System", DateTime? eventDate = null)
        {
            var entry = new TicketTimeline
            {
                TicketId = ticketId,
                Title = title,
                Description = description,
                Icon = icon,
                Color = color,
                EventDate = eventDate ?? DateTime.Now,
                CreatedBy = createdBy,
                CreatedOn = DateTime.Now
            };

            _context.TicketTimelines.Add(entry);
            await _context.SaveChangesAsync();
        }

        // Load timeline from DB
        public async Task<List<TimelineEvent>> GetTimelineAsync(int ticketId)
        {
            return await _context.TicketTimelines
                .Where(t => t.TicketId == ticketId)
                .OrderByDescending(t => t.EventDate)
                .Select(t => new TimelineEvent
                {
                    Id = t.Id,
                    Date = t.EventDate,
                    Title = t.Title,
                    Description = t.Description,
                    Icon = t.Icon,
                    Color = t.Color
                })
                .ToListAsync();
        }
    }
}