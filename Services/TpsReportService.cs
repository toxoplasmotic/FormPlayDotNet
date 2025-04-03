using FormPlay.Data;
using FormPlay.Models;
using FormPlay.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FormPlay.Services
{
    public class TpsReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly PdfService _pdfService;
        private readonly EmailService _emailService;
        private readonly CalendarService _calendarService;
        private readonly ILogger<TpsReportService> _logger;

        public TpsReportService(
            ApplicationDbContext context,
            PdfService pdfService,
            EmailService emailService,
            CalendarService calendarService,
            ILogger<TpsReportService> logger)
        {
            _context = context;
            _pdfService = pdfService;
            _emailService = emailService;
            _calendarService = calendarService;
            _logger = logger;
        }

        public async Task<List<TpsReportViewModel>> GetAllTpsReportsAsync(int currentUserId)
        {
            var reports = await _context.TpsReports
                .Include(r => r.InitiatedBy)
                .Include(r => r.PartnerUser)
                .Include(r => r.Fields)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            return reports.Select(r => MapToViewModel(r, currentUserId)).ToList();
        }

        public async Task<TpsReportViewModel> GetTpsReportAsync(int id, int currentUserId)
        {
            var report = await _context.TpsReports
                .Include(r => r.InitiatedBy)
                .Include(r => r.PartnerUser)
                .Include(r => r.Fields)
                .Include(r => r.Actions)
                .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
                return null;

            return MapToViewModel(report, currentUserId);
        }

        public async Task<TpsReport> CreateNewTpsReportAsync(int initiatorId, string templateType = null)
        {
            var initiator = await _context.Users.FindAsync(initiatorId);
            if (initiator == null)
                throw new ArgumentException("Invalid user ID");

            var partner = await _context.Users.FindAsync(initiator.PartnerId);
            if (partner == null)
                throw new ArgumentException("Partner not found");

            // If no template type is specified, use the default from configuration
            if (string.IsNullOrEmpty(templateType))
            {
                templateType = _configuration["PdfSettings:DefaultTemplate"];
            }

            var report = new TpsReport
            {
                CreatedDate = DateTime.Now,
                Status = TpsReportStatus.New,
                InitiatedById = initiatorId,
                PartnerUserId = initiator.PartnerId,
                InitiatedBy = initiator,
                PartnerUser = partner,
                TemplateType = templateType
            };

            _context.TpsReports.Add(report);
            
            var action = new TpsReportAction
            {
                TpsReport = report,
                UserId = initiatorId,
                User = initiator,
                Timestamp = DateTime.Now,
                ActionType = "Created"
            };
            
            _context.TpsReportActions.Add(action);
            
            await _context.SaveChangesAsync();
            
            return report;
        }

        public async Task<bool> UpdateTpsReportFieldsAsync(int reportId, Dictionary<string, string> formFields, int currentUserId)
        {
            var report = await _context.TpsReports
                .Include(r => r.Fields)
                .FirstOrDefaultAsync(r => r.Id == reportId);
                
            if (report == null || !report.CanUserEdit(currentUserId))
                return false;
                
            foreach (var field in formFields)
            {
                var existingField = report.Fields.FirstOrDefault(f => f.FieldName == field.Key);
                
                if (existingField != null)
                {
                    existingField.FieldValue = field.Value;
                    existingField.LastModified = DateTime.Now;
                    existingField.LastModifiedByUserId = currentUserId;
                }
                else
                {
                    report.Fields.Add(new TpsReportField
                    {
                        TpsReport = report,
                        TpsReportId = reportId,
                        FieldName = field.Key,
                        FieldValue = field.Value,
                        LastModified = DateTime.Now,
                        LastModifiedByUserId = currentUserId
                    });
                }
            }
            
            // Extract certain key fields for easier access
            ExtractKeyFields(report, formFields);
            
            report.LastModifiedDate = DateTime.Now;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SubmitForReviewAsync(int reportId, int currentUserId)
        {
            var report = await _context.TpsReports
                .Include(r => r.InitiatedBy)
                .Include(r => r.PartnerUser)
                .FirstOrDefaultAsync(r => r.Id == reportId);
                
            if (report == null || report.Status != TpsReportStatus.New || report.InitiatedById != currentUserId)
                return false;
                
            var user = await _context.Users.FindAsync(currentUserId);
            if (user == null)
                return false;

            report.Status = TpsReportStatus.Pending;
            report.LastModifiedDate = DateTime.Now;
            
            var action = new TpsReportAction
            {
                TpsReport = report,
                TpsReportId = reportId,
                User = user,
                UserId = currentUserId,
                Timestamp = DateTime.Now,
                ActionType = "Submitted"
            };
            
            _context.TpsReportActions.Add(action);
            await _context.SaveChangesAsync();
            
            // Send notification to partner
            await _emailService.SendTpsReportNotificationAsync(report, report.PartnerUser, "Created");
            
            return true;
        }

        public async Task<bool> RespondToTpsReportAsync(int reportId, bool approved, string notes, int currentUserId)
        {
            var report = await _context.TpsReports
                .Include(r => r.InitiatedBy)
                .Include(r => r.PartnerUser)
                .FirstOrDefaultAsync(r => r.Id == reportId);
                
            if (report == null || report.Status != TpsReportStatus.Pending || report.PartnerUserId != currentUserId)
                return false;
                
            var user = await _context.Users.FindAsync(currentUserId);
            if (user == null)
                return false;
                
            if (approved)
            {
                report.Status = TpsReportStatus.AwaitingFinalApproval;
                
                var action = new TpsReportAction
                {
                    TpsReport = report,
                    TpsReportId = reportId,
                    User = user,
                    UserId = currentUserId,
                    Timestamp = DateTime.Now,
                    ActionType = "Reviewed",
                    Notes = notes
                };
                
                _context.TpsReportActions.Add(action);
            }
            else
            {
                report.Status = TpsReportStatus.Aborted;
                
                var action = new TpsReportAction
                {
                    TpsReport = report,
                    TpsReportId = reportId,
                    User = user,
                    UserId = currentUserId,
                    Timestamp = DateTime.Now,
                    ActionType = "Denied",
                    Notes = notes
                };
                
                _context.TpsReportActions.Add(action);
            }
            
            report.LastModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            
            // Send notification to initiator
            string actionType = approved ? "Reviewed" : "Denied";
            await _emailService.SendTpsReportNotificationAsync(report, report.InitiatedBy, actionType);
            
            return true;
        }

        public async Task<bool> FinalizeApprovalAsync(int reportId, int currentUserId)
        {
            var report = await _context.TpsReports
                .Include(r => r.InitiatedBy)
                .Include(r => r.PartnerUser)
                .FirstOrDefaultAsync(r => r.Id == reportId);
                
            if (report == null || report.Status != TpsReportStatus.AwaitingFinalApproval || report.InitiatedById != currentUserId)
                return false;
                
            var user = await _context.Users.FindAsync(currentUserId);
            if (user == null)
                return false;

            report.Status = TpsReportStatus.Approved;
            report.LastModifiedDate = DateTime.Now;
            
            var action = new TpsReportAction
            {
                TpsReport = report,
                TpsReportId = reportId,
                User = user,
                UserId = currentUserId,
                Timestamp = DateTime.Now,
                ActionType = "Approved"
            };
            
            _context.TpsReportActions.Add(action);
            await _context.SaveChangesAsync();
            
            // Add to calendars
            await _calendarService.AddToCalendarsAsync(report);
            
            // Send notification to both users
            await _emailService.SendTpsReportNotificationAsync(report, report.InitiatedBy, "Approved");
            await _emailService.SendTpsReportNotificationAsync(report, report.PartnerUser, "Approved");
            
            return true;
        }

        private void ExtractKeyFields(TpsReport report, Dictionary<string, string> formFields)
        {
            // Extract key fields for easier access and querying
            if (formFields.TryGetValue("Date", out var date) && !string.IsNullOrEmpty(date))
            {
                if (DateTime.TryParse(date, out var parsedDate))
                {
                    report.ScheduledDate = parsedDate;
                }
            }
            
            if (formFields.TryGetValue("Time (Start)", out var startTime) && !string.IsNullOrEmpty(startTime))
            {
                if (TimeSpan.TryParse(startTime, out var parsedTime))
                {
                    report.ScheduledStartTime = parsedTime;
                }
            }
            
            if (formFields.TryGetValue("Time (Estimated End)", out var endTime) && !string.IsNullOrEmpty(endTime))
            {
                if (TimeSpan.TryParse(endTime, out var parsedTime))
                {
                    report.ScheduledEndTime = parsedTime;
                }
            }
            
            // Location might be stored in multiple fields, so check them in sequence
            string[] locationFields = { "Location", "Master bedroom", "Basement", "Other" };
            foreach (var field in locationFields)
            {
                if (formFields.TryGetValue(field, out var location) && !string.IsNullOrEmpty(location))
                {
                    if (location == "On") // Checkbox is checked
                    {
                        report.Location = field;
                        break;
                    }
                    else if (field == "Location" || field == "Other")
                    {
                        report.Location = location;
                        break;
                    }
                }
            }
            
            // Store initials if present
            if (formFields.TryGetValue("Matt", out var mattInitials) && !string.IsNullOrEmpty(mattInitials))
            {
                report.MattsInitials = mattInitials;
            }
            
            if (formFields.TryGetValue("Mina", out var minaInitials) && !string.IsNullOrEmpty(minaInitials))
            {
                report.MinasInitials = minaInitials;
            }
        }

        private TpsReportViewModel MapToViewModel(TpsReport report, int currentUserId)
        {
            var viewModel = new TpsReportViewModel
            {
                Id = report.Id,
                Status = report.Status,
                InitiatedByName = report.InitiatedBy?.Name,
                PartnerName = report.PartnerUser?.Name,
                ScheduledDate = report.ScheduledDate,
                ScheduledStartTime = report.ScheduledStartTime?.ToString(),
                ScheduledEndTime = report.ScheduledEndTime?.ToString(),
                Location = report.Location,
                CanUserEdit = report.CanUserEdit(currentUserId),
                IsInitiator = report.InitiatedById == currentUserId,
                PdfUrl = report.PdfFilePath,
                CreatedDate = report.CreatedDate,
                MattsInitials = report.MattsInitials,
                MinasInitials = report.MinasInitials,
                Actions = report.Actions?.ToList() ?? new List<TpsReportAction>()
            };
            
            // Add form fields to the view model
            if (report.Fields != null)
            {
                foreach (var field in report.Fields)
                {
                    viewModel.FormFields[field.FieldName] = field.FieldValue;
                }
            }
            
            return viewModel;
        }
    }
}
