using System.ComponentModel.DataAnnotations;

namespace FormPlay.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public required string Name { get; set; }
        
        [Required]
        [MaxLength(100)]
        public required string Email { get; set; }
        
        // Partner relationship is implicit since there are only two users
        public int PartnerId => Id == 1 ? 2 : 1;
    }
}
