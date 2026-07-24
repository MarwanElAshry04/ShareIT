using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShareIT.Models;
using System.Net.Mail;

namespace ShareIT.Data
{
    public class ApplicationDbContext : IdentityDbContext<Users>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Models.Attachment> Attachments { get; set; }
        public DbSet<Chats> Chats { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Recording> Recordings { get; set; }
        public DbSet<Reporter> Reporters { get; set; }
        public DbSet<Models.Document> Documents { get; set; }
        public DbSet<TempEmailModel> TempEmails { get; set; }
        public DbSet<TicketTimeline> TicketTimelines { get; set; }

        public DbSet<Users> users { get; set; }
        public DbSet<Video> Videos  { get; set; }
        public DbSet<Relation> Relations { get; set; }

    }
}
