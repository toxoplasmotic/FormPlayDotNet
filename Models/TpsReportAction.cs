using System.ComponentModel.DataAnnotations;

namespace FormPlay.Models
{
    public class TpsReportAction
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int TpsReportId { get; set; }
        
        public required TpsReport TpsReport { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        public required User User { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; }
        
        [Required]
        [MaxLength(50)]
        public required string ActionType { get; set; }  // Created, Submitted, Approved, Denied, etc.
        
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
