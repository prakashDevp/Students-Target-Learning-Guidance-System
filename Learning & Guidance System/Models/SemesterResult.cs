using System.ComponentModel.DataAnnotations;

namespace Learning_Guidance_System.Models
{
    public class SemesterResult
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        [Range(1, 12)]
        public int SemesterNumber { get; set; } // 1..8 (or more)

        [Range(0, 10)]
        public decimal SemesterGpa { get; set; } // e.g. 8.75

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
