using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareIT.Data;
using ShareIT.Models;
using ShareIT.Models.ViewModel;
using System.Diagnostics;

namespace ShareIT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;

        }
        // GET: Home page with documents
        public async Task<IActionResult> Index()
        {
            var documents = await _context.Documents
                .OrderBy(d => d.DisplayOrder)
                .ThenBy(d => d.Name)
                .ToListAsync();

            var model = new HomeViewModel
            {
                Documents = documents
            };

            return View(model);
        }
        public async Task<IActionResult> Dashboard()
        {
            var tickets = await _context.Tickets
                .Include(c => c.TicketType)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                TotalTickets = tickets.Count
            };

            model.SubmissionsByCompany = tickets
                .GroupBy(c => string.IsNullOrWhiteSpace(c.ConcerningCompany) ? "Unspecified" : c.ConcerningCompany)
                .Select(g => new ChartLabelValue { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToList();

            model.TypeLabels = tickets
                .Select(c => c.TicketType?.ComplainType ?? "Unspecified")
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            var statusLabels = tickets
                .Select(c => string.IsNullOrWhiteSpace(c.Status) ? "Unspecified" : c.Status)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            foreach (var status in statusLabels)
            {
                var series = new StackedSeries { StatusLabel = status };
                foreach (var type in model.TypeLabels)
                {
                    series.Values.Add(tickets.Count(c =>
                        (c.TicketType?.ComplainType ?? "Unspecified") == type &&
                        (string.IsNullOrWhiteSpace(c.Status) ? "Unspecified" : c.Status) == status));
                }
                model.SubmissionsByTypeAndStatus.Add(series);
            }

            model.TopTicketTypes = tickets
                .GroupBy(c => c.TicketType?.ComplainType ?? "Unspecified")
                .Select(g => new ChartLabelValue { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToList();

            model.StatusBreakdown = tickets
                .GroupBy(c => string.IsNullOrWhiteSpace(c.Status) ? "Unspecified" : c.Status)
                .Select(g => new ChartLabelValue { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToList();

            model.KindBreakdown = tickets
                .GroupBy(c => c.Kind.ToString())
                .Select(g => new ChartLabelValue { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToList();

            return View(model);
        }
        

        // GET: View document details
        public async Task<IActionResult> Details(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        public async Task<IActionResult> Guid() 
        {
            return View ();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
