using Learning_Guidance_System.Models;
using System.ComponentModel.DataAnnotations;

namespace Learning_Guidance_System.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public string? Email { get; set; }
        public string DegreeType { get; set; } = "Engineering";
        public string? Department { get; set; }

        // e.g. 9.50
        [Range(0, 10)]
        public decimal TargetCgpa { get; set; }

        // Usually 8 for engineering
        [Range(1, 12)]
        public int TotalSemesters { get; set; } = 8;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SemesterResult> SemesterResults { get; set; } = new List<SemesterResult>();
    }
}
