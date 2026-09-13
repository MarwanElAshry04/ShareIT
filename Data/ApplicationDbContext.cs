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
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // keeps ASP.NET Identity tables working

            // Each detail shares its Ticket's Id (a 1-to-1 link).
            // Delete a ticket -> its detail is deleted too.
            builder.Entity<IssueDetail>().HasOne(d => d.Ticket).WithOne(t => t.IssueDetail).HasForeignKey<IssueDetail>(d => d.TicketId);
            builder.Entity<IdeaDetail>().HasOne(d => d.Ticket).WithOne(t => t.IdeaDetail).HasForeignKey<IdeaDetail>(d => d.TicketId);
            builder.Entity<ProjectProposalDetail>().HasOne(d => d.Ticket).WithOne(t => t.ProjectProposalDetail).HasForeignKey<ProjectProposalDetail>(d => d.TicketId);
            builder.Entity<SafetyDetail>().HasOne(d => d.Ticket).WithOne(t => t.SafetyDetail).HasForeignKey<SafetyDetail>(d => d.TicketId);
            builder.Entity<FeedbackDetail>().HasOne(d => d.Ticket).WithOne(t => t.FeedbackDetail).HasForeignKey<FeedbackDetail>(d => d.TicketId);
        }
        public DbSet<Models.Attachment> Attachments { get; set; }
        public DbSet<Chats> Chats { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Recording> Recordings { get; set; }
        public DbSet<Reporter> Reporters { get; set; }
        public DbSet<Models.Document> Documents { get; set; }
        public DbSet<TempEmailModel> TempEmails { get; set; }
        public DbSet<TicketTimeline> TicketTimelines { get; set; }
        public DbSet<IssueDetail> IssueDetails { get; set; }
        public DbSet<IdeaDetail> IdeaDetails { get; set; }
        public DbSet<ProjectProposalDetail> ProjectProposalDetails { get; set; }
        public DbSet<SafetyDetail> SafetyDetails { get; set; }
        public DbSet<FeedbackDetail> FeedbackDetails { get; set; }

        public DbSet<Users> users { get; set; }
        public DbSet<Video> Videos  { get; set; }
        public DbSet<Relation> Relations { get; set; }

    }
}
