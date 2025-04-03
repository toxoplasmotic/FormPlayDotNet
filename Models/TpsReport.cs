using System.ComponentModel.DataAnnotations;

namespace FormPlay.Models
{
    public class TpsReport
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public DateTime CreatedDate { get; set; }
        
        public DateTime? LastModifiedDate { get; set; }
        
        [Required]
        public TpsReportStatus Status { get; set; }
        
        [Required]
        public int InitiatedById { get; set; }
        public required User InitiatedBy { get; set; }
        
        [Required]
        public int PartnerUserId { get; set; }
        public required User PartnerUser { get; set; }
        
        public string? PdfFilePath { get; set; }
        
        // The type of template that was used for this report (e.g., 'vanilla', 'custom1', etc.)
        public string? TemplateType { get; set; }
        
        public DateTime? ScheduledDate { get; set; }
        
        public TimeSpan? ScheduledStartTime { get; set; }
        
        public TimeSpan? ScheduledEndTime { get; set; }
        
        public string? Location { get; set; }
        
        public bool AddedToCalendar { get; set; }
        
        public string? MattsInitials { get; set; }
        
        public string? MinasInitials { get; set; }
        
        // Navigation property for fields
        public ICollection<TpsReportField> Fields { get; set; } = new List<TpsReportField>();
        
        // Navigation property for actions
        public ICollection<TpsReportAction> Actions { get; set; } = new List<TpsReportAction>();
        
        // Helper method to determine if edit is allowed
        public bool CanUserEdit(int userId)
        {
            // Initial stage: only the initiator can edit
            if (Status == TpsReportStatus.New && userId == InitiatedById)
                return true;
                
            // Review stage: only the partner can edit
            if (Status == TpsReportStatus.Pending && userId == PartnerUserId)
                return true;
                
            // Final approval stage: only initiator can add initials
            if (Status == TpsReportStatus.AwaitingFinalApproval && userId == InitiatedById)
                return true;
                
            return false;
        }
    }
}
