using Learning_Guidance_System.Data;
using Learning_Guidance_System.Dtos;

using Microsoft.EntityFrameworkCore;

namespace Learning_Guidance_System.Services
{
    public class DashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardResultDto> GetDashboardAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.SemesterResults)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                throw new Exception("Student not found");

            var results = student.SemesterResults
                .OrderBy(r => r.SemesterNumber)
                .ToList();

            int completed = results.Count;
            int total = student.TotalSemesters;
            int remaining = total - completed;

            decimal currentCgpa = completed == 0
                ? 0
                : results.Average(r => r.SemesterGpa);

            decimal requiredAverage;
            if (remaining == 0)
            {
                requiredAverage = currentCgpa; // no more semesters
            }
            else
            {
                decimal sumDone = results.Sum(r => r.SemesterGpa);
                requiredAverage = (student.TargetCgpa * total - sumDone) / remaining;
            }

            string status;
            string suggestion;

            if (remaining == 0)
            {
                if (currentCgpa >= student.TargetCgpa)
                {
                    status = "OnTrack";
                    suggestion = "You have achieved your target CGPA. Great job! 🎉";
                }
                else
                {
                    status = "VeryHard";
                    suggestion = "No remaining semesters. You did well, but the original target was not fully reached.";
                }
            }
            else
            {
                if (requiredAverage <= student.TargetCgpa + 0.2m)
                {
                    status = "OnTrack";
                    suggestion = $"Keep going! You need around {requiredAverage:F2} CGPA in each of the next {remaining} semesters.";
                }
                else if (requiredAverage <= 10)
                {
                    status = "NeedsImprovement";
                    suggestion = $"You must push harder. Aim for at least {requiredAverage:F2} CGPA in the next {remaining} semesters.";
                }
                else
                {
                    status = "VeryHard";
                    suggestion = "Mathematically very difficult to reach the target. Consider adjusting the target or focusing on other strengths.";
                }
            }

            return new DashboardResultDto
            {
                TargetCgpa = student.TargetCgpa,
                CurrentCgpa = currentCgpa,
                CompletedSemesters = completed,
                RemainingSemesters = remaining,
                RequiredAverage = requiredAverage,
                Status = status,
                Suggestion = suggestion
            };
        }
    }
}
