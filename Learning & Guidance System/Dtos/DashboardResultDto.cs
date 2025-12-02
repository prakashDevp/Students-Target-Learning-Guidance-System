namespace Learning_Guidance_System.Dtos
{
    public class DashboardResultDto
    {
        public decimal TargetCgpa { get; set; }
        public decimal CurrentCgpa { get; set; }
        public int CompletedSemesters { get; set; }
        public int RemainingSemesters { get; set; }
        public decimal RequiredAverage { get; set; }
        public string Status { get; set; } = "";
        public string Suggestion { get; set; } = "";
    }
}
