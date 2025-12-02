namespace Learning_Guidance_System.Dtos
{
    public class StudentCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string? Department { get; set; }

        public decimal TargetCgpa { get; set; }
        public int TotalSemesters { get; set; } = 8;
    }
}
