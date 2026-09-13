using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ShareIT;
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
        private readonly IStringLocalizer<SharedResource> _localizer;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context,
            IStringLocalizer<SharedResource> localizer)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
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
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Dashboard()
        {
            var tickets = await _context.Tickets
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
                .Select(c => c.TicketType.ToString())
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
                        c.TicketType.ToString() == type &&
                        (string.IsNullOrWhiteSpace(c.Status) ? "Unspecified" : c.Status) == status));
                }
                model.SubmissionsByTypeAndStatus.Add(series);
            }

            model.TopCategories = tickets
                .GroupBy(c => c.TicketType.ToString())
                .Select(g => new ChartLabelValue { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToList();

            model.StatusBreakdown = tickets
                .GroupBy(c => string.IsNullOrWhiteSpace(c.Status) ? "Unspecified" : c.Status)
                .Select(g => new ChartLabelValue { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToList();

            model.TypeBreakdown = tickets
                .GroupBy(c => c.TicketType.ToString())
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

        // GET: Public User Guide — tutorial videos grouped by section
        public async Task<IActionResult> Guid()
        {
            var videos = await _context.Videos
                .OrderBy(v => v.CreatedOn)
                .ToListAsync();

            var icons = new Dictionary<VideoSection, string>
            {
                [VideoSection.NewCase] = "fas fa-plus-circle",
                [VideoSection.TrackCase] = "fas fa-search",
                [VideoSection.ShareITVideos] = "fas fa-video"
            };

            // Built from the enum, not from the data, so the three headings always
            // render in a fixed order even when a section has no videos.
            var model = new UserGuideViewModel
            {
                Sections = Enum.GetValues<VideoSection>()
                    .Select(s => new GuideSectionViewModel
                    {
                        Section = s,
                        // Localized: the User Guide is employee-facing and in the ar scope.
                        Heading = _localizer[s.GetDisplayName()],
                        IconClass = icons[s],
                        Videos = videos.Where(v => v.Section == s).ToList()
                    })
                    .ToList()
            };

            return View(model);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
