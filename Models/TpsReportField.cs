using System.ComponentModel.DataAnnotations;

namespace FormPlay.Models
{
    public class TpsReportField
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int TpsReportId { get; set; }
        
        public required TpsReport TpsReport { get; set; }
        
        [Required]
        [MaxLength(100)]
        public required string FieldName { get; set; }
        
        [MaxLength(500)]
        public string? FieldValue { get; set; }
        
        public DateTime LastModified { get; set; }
        
        public int LastModifiedByUserId { get; set; }
    }
}
