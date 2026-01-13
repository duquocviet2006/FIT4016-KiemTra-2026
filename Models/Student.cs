using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagement.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }
        public School? School { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; } = null!;

        [Required]
        [StringLength(20, MinimumLength = 5)]
        public string StudentId { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Phone must be 10-11 digits")]
        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
