using FormPlay.Models;
using FormPlay.Services;
using Microsoft.AspNetCore.Mvc;

namespace FormPlay.Controllers
{
    public class TpsReportController : Controller
    {
        private readonly TpsReportService _tpsReportService;
        private readonly PdfService _pdfService;
        private readonly UserService _userService;
        private readonly ILogger<TpsReportController> _logger;

        public TpsReportController(
            TpsReportService tpsReportService,
            PdfService pdfService,
            UserService userService,
            ILogger<TpsReportController> logger)
        {
            _tpsReportService = tpsReportService;
            _pdfService = pdfService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // For demo purposes, default to user 1
            int currentUserId = 1;
            var reports = await _tpsReportService.GetAllTpsReportsAsync(currentUserId);
            return View("List", reports);
        }

        public async Task<IActionResult> List(int userId = 1)
        {
            var reports = await _tpsReportService.GetAllTpsReportsAsync(userId);
            ViewBag.CurrentUserId = userId;
            return View(reports);
        }

        public async Task<IActionResult> Details(int id, int userId = 1)
        {
            var report = await _tpsReportService.GetTpsReportAsync(id, userId);
            if (report == null)
                return NotFound();
                
            ViewBag.CurrentUserId = userId;
            return View(report);
        }

        public async Task<IActionResult> Create(int userId = 1, string templateType = null)
        {
            var report = await _tpsReportService.CreateNewTpsReportAsync(userId, templateType);
            
            // For now, redirect to the report details page
            return RedirectToAction("Details", new { id = report.Id, userId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFields(int id, int userId, Dictionary<string, string> formFields)
        {
            var success = await _tpsReportService.UpdateTpsReportFieldsAsync(id, formFields, userId);
            if (!success)
                return BadRequest("Failed to update fields");
                
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitForReview(int id, int userId)
        {
            var success = await _tpsReportService.SubmitForReviewAsync(id, userId);
            if (!success)
                return BadRequest("Failed to submit for review");
                
            return RedirectToAction("Details", new { id, userId });
        }

        [HttpPost]
        public async Task<IActionResult> RespondToReport(int id, int userId, bool approved, string notes)
        {
            var success = await _tpsReportService.RespondToTpsReportAsync(id, approved, notes, userId);
            if (!success)
                return BadRequest("Failed to respond to report");
                
            return RedirectToAction("Details", new { id, userId });
        }

        [HttpPost]
        public async Task<IActionResult> FinalizeApproval(int id, int userId)
        {
            var success = await _tpsReportService.FinalizeApprovalAsync(id, userId);
            if (!success)
                return BadRequest("Failed to finalize approval");
                
            return RedirectToAction("Details", new { id, userId });
        }

        [HttpPost]
        public async Task<IActionResult> UploadPdf(int id, int userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");
                
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var formFields = await _pdfService.ExtractPdfFormFieldsAsync(stream);
                    await _tpsReportService.UpdateTpsReportFieldsAsync(id, formFields, userId);
                }
                
                return RedirectToAction("Details", new { id, userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PDF upload");
                return StatusCode(500, "Error processing PDF");
            }
        }
    }
}
