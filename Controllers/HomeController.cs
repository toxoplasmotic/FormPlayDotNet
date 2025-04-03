using FormPlay.Models;
using FormPlay.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FormPlay.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserService _userService;
        private readonly TpsReportService _tpsReportService;

        public HomeController(
            ILogger<HomeController> logger,
            UserService userService,
            TpsReportService tpsReportService)
        {
            _logger = logger;
            _userService = userService;
            _tpsReportService = tpsReportService;
        }

        public async Task<IActionResult> Index(int userId = 1)
        {
            var users = await _userService.GetAllUsersAsync();
            ViewBag.Users = users;
            ViewBag.CurrentUserId = userId;
            
            var reports = await _tpsReportService.GetAllTpsReportsAsync(userId);
            return View(reports);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Model for the Error view
        public class ErrorViewModel
        {
            public string RequestId { get; set; }
            public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        }
    }
}
