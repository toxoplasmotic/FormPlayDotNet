using System.ComponentModel.DataAnnotations;

namespace FormPlay.Models.ViewModels
{
    public class TpsReportViewModel
    {
        public int Id { get; set; }
        
        public TpsReportStatus Status { get; set; }
        
        public string StatusDisplay => Status.ToString();
        
        public required string InitiatedByName { get; set; }
        
        public required string PartnerName { get; set; }
        
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime? ScheduledDate { get; set; }
        
        [Display(Name = "Start Time")]
        public string? ScheduledStartTime { get; set; }
        
        [Display(Name = "Estimated End Time")]
        public string? ScheduledEndTime { get; set; }
        
        public string? Location { get; set; }
        
        public bool CanUserEdit { get; set; }
        
        public bool IsInitiator { get; set; }
        
        public string? PdfUrl { get; set; }
        
        public DateTime CreatedDate { get; set; }
        
        public bool MattsInitialsProvided => !string.IsNullOrEmpty(MattsInitials);
        
        public bool MinasInitialsProvided => !string.IsNullOrEmpty(MinasInitials);
        
        public string? MattsInitials { get; set; }
        
        public string? MinasInitials { get; set; }
        
        public Dictionary<string, string> FormFields { get; set; } = new Dictionary<string, string>();
        
        public List<TpsReportAction> Actions { get; set; } = new List<TpsReportAction>();
    }
}
