using Microsoft.EntityFrameworkCore;
using ShareIT.Data;
using ShareIT.Models;

namespace ShareIT.Seeds
{
    /// <summary>
    /// Seeds demonstration data (lookups + sample tickets) so the site's flow
    /// can be shown end-to-end. Runs once: skips entirely if companies exist.
    /// </summary>
    public static class DefaultData
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Companies.AnyAsync())
                return; // already seeded

            var now = DateTime.Now;

            // ---- Companies ----
            var companies = new List<Company>
            {
                new() { Name = "Elsewedy Electric - Cairo HQ",      Region = "Egypt",        status = 1, CreatedOn = now },
                new() { Name = "Elsewedy Electric - 10th of Ramadan", Region = "Egypt",       status = 1, CreatedOn = now },
                new() { Name = "Elsewedy Electric - KSA",           Region = "Saudi Arabia", status = 1, CreatedOn = now },
                new() { Name = "Elsewedy Electric - UAE",           Region = "UAE",          status = 1, CreatedOn = now },
                new() { Name = "Elsewedy Technical Academy",        Region = "Egypt",        status = 1, CreatedOn = now },
            };
            context.Companies.AddRange(companies);

            // ---- Departments ----
            var departments = new List<Department>
            {
                new() { Name = "Human Resources",       status = 1, CreatedOn = now },
                new() { Name = "Finance",               status = 1, CreatedOn = now },
                new() { Name = "Information Technology", status = 1, CreatedOn = now },
                new() { Name = "Operations",            status = 1, CreatedOn = now },
                new() { Name = "Legal & Compliance",    status = 1, CreatedOn = now },
                new() { Name = "Procurement",           status = 1, CreatedOn = now },
                new() { Name = "Health & Safety",       status = 1, CreatedOn = now },
                new() { Name = "Sales & Marketing",     status = 1, CreatedOn = now },
            };
            context.Departments.AddRange(departments);

            // ---- Relations (reporter relationship to the company) ----
            var relations = new List<Relation>
            {
                new() { Name = "Employee",         Name_ar = "موظف" },
                new() { Name = "Supplier",         Name_ar = "مورد" },
                new() { Name = "Contractor",       Name_ar = "مقاول" },
                new() { Name = "Customer",         Name_ar = "عميل" },
                new() { Name = "Business Partner",  Name_ar = "شريك" },
                new() { Name = "Other",            Name_ar = "أخرى" },
            };
            context.Relations.AddRange(relations);

            // ---- Ticket types (the 6 kinds) ----
            var types = new List<Category>
            {
                new() { ComplainType = "Issue",            ComplainType_ar = "مشكلة",         definition = "Operational, people, or process problem.",   priority = 1, status = 1, CreatedOn = now },
                new() { ComplainType = "Idea",             ComplainType_ar = "فكرة",          definition = "Improvement or new idea.",                   priority = 3, status = 1, CreatedOn = now },
                new() { ComplainType = "Project Proposal", ComplainType_ar = "مقترح مشروع",   definition = "Project with cost, benefits, and timeline.", priority = 3, status = 1, CreatedOn = now },
                new() { ComplainType = "Safety Concern",   ComplainType_ar = "مخاوف السلامة", definition = "Safety hazard or risk.",                     priority = 1, status = 1, CreatedOn = now },
                new() { ComplainType = "Feedback",         ComplainType_ar = "ملاحظات",       definition = "Workplace feedback or a complaint.",         priority = 4, status = 1, CreatedOn = now },
                new() { ComplainType = "System Request",   ComplainType_ar = "طلب نظام",      definition = "Digital or system improvement request.",     priority = 3, status = 1, CreatedOn = now },
            };
            context.Categories.AddRange(types);

            await context.SaveChangesAsync(); // materialize Ids for FKs

            var employeeRel = relations.First(r => r.Name == "Employee");
            var pwd = EncodeBase64("123456"); // demo tracking password: 123456

            Reporter Rep(string name, string email, string mobile, string type, int? deptIndex, string? empCode) => new()
            {
                name = name,
                email1 = email,
                mobile = mobile,
                reporterType = type,
                RelationId = employeeRel.Id,
                DepratmentId = deptIndex.HasValue ? departments[deptIndex.Value].Id : null,
                empCode = empCode,
                CreatedOn = now
            };

            // ---- Sample tickets across kinds / statuses / months ----
            var tickets = new List<Ticket>
            {
                MakeTicket("SHR-2026-0001", TicketType.Issue,  types[0], "Reported inappropriate behavior from a colleague during a team meeting.",
                    companies[0].Name, departments[0].Name, "Initiate",   "Valid",   now.AddDays(-3),  pwd,
                    Rep("Mona Adel", "mona.adel@elsewedy.com", "01001234567", "Disclosed", 0, "EE1001")),

                MakeTicket("SHR-2026-0002", TicketType.Issue,  types[0], "Fire exit on the 3rd floor is blocked by stored equipment.",
                    companies[1].Name, departments[6].Name, "In-Process", "Valid",   now.AddDays(-12), pwd,
                    Rep(null, null, null, "Anonymous", null, null)),

                MakeTicket("SHR-2026-0003", TicketType.Issue,  types[0], "Suspected irregularities in a recent supplier procurement process.",
                    companies[2].Name, departments[5].Name, "In-Process", null,      now.AddDays(-20), pwd,
                    Rep("Khaled Samir", "khaled.samir@elsewedy.com", "01112223334", "Disclosed", 5, "EE1042")),

                MakeTicket("SHR-2026-0004", TicketType.Issue,  types[0], "Concerns about unequal access to training opportunities.",
                    companies[0].Name, departments[0].Name, "Closed",     "Valid",   now.AddMonths(-2), pwd,
                    Rep("Sara Nabil", "sara.nabil@elsewedy.com", "01223334445", "Disclosed", 0, "EE1077")),

                MakeTicket("SHR-2026-0005", TicketType.Idea, types[1], "Introduce a shared digital request tracker to cut email back-and-forth.",
                    null, null, "Initiate",   null, now.AddDays(-5),  pwd,
                    Rep("Omar Hassan", "omar.hassan@elsewedy.com", "01009998887", "Disclosed", 2, "EE1120")),

                MakeTicket("SHR-2026-0006", TicketType.Idea, types[1], "Add a mobile app for field engineers to log work orders offline.",
                    null, null, "In-Process", null, now.AddDays(-9),  pwd,
                    Rep("Youssef Fathy", "youssef.fathy@elsewedy.com", "01555666777", "Disclosed", 3, "EE1150")),

                MakeTicket("SHR-2026-0007", TicketType.Idea, types[1], "Offer flexible start times to ease morning traffic congestion.",
                    null, null, "Closed",     null, now.AddMonths(-1), pwd,
                    Rep("Laila Mahmoud", "laila.mahmoud@elsewedy.com", "01778889990", "Disclosed", 0, "EE1188")),

                MakeTicket("SHR-2026-0008", TicketType.Feedback,   types[4], "The new onboarding program was clear and very helpful. Thank you!",
                    null, null, "Closed",     null, now.AddDays(-2),  pwd,
                    Rep("Ahmed Zaki", "ahmed.zaki@elsewedy.com", "01004445556", "Disclosed", 2, "EE1201")),

                MakeTicket("SHR-2026-0009", TicketType.Feedback,   types[4], "Cafeteria menu variety has improved a lot this quarter.",
                    null, null, "Initiate",   null, now.AddDays(-1),  pwd,
                    Rep(null, null, null, "Anonymous", null, null)),

                MakeTicket("SHR-2026-0010", TicketType.Issue,  types[0], "Slippery floor near the warehouse loading bay after rain.",
                    companies[3].Name, departments[3].Name, "Initiate",   null, now.AddDays(-7),  pwd,
                    Rep("Hana Tarek", "hana.tarek@elsewedy.com", "01667778889", "Disclosed", 3, "EE1233")),

                MakeTicket("SHR-2026-0011", TicketType.Feedback,   types[4], "IT support response time has been excellent lately.",
                    null, null, "Closed",     null, now.AddMonths(-3), pwd,
                    Rep("Nour Ibrahim", "nour.ibrahim@elsewedy.com", "01008887776", "Disclosed", 2, "EE1260")),

                MakeTicket("SHR-2026-0012", TicketType.Issue,  types[0], "Repeated dismissive comments from a supervisor toward the team.",
                    companies[4].Name, departments[0].Name, "In-Process", "Valid", now.AddMonths(-1).AddDays(-4), pwd,
                    Rep("Tarek Wael", "tarek.wael@elsewedy.com", "01551112223", "Disclosed", 0, "EE1288")),
            };

            context.Tickets.AddRange(tickets);
            await context.SaveChangesAsync();
        }

        private static Ticket MakeTicket(
            string refNo, TicketType kind, Category type, string desc,
            string? concerningCompany, string? concerningDept,
            string status, string? validation, DateTime createdOn, string password, Reporter reporter)
        {
            return new Ticket
            {
                RefNo = refNo,
                TicketType = kind,
                Category = type,
                Desc = desc,
                ConcerningCompany = concerningCompany,
                ConcerningDepartment = concerningDept,
                Status = status,
                validation = validation,
                langFlag = "E",
                password = password,
                CreatedOn = createdOn,
                closureDate = status == "Closed" ? createdOn.AddDays(6) : null,
                Reporter = reporter
            };
        }

        private static string EncodeBase64(string value) =>
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value));
    }
}
