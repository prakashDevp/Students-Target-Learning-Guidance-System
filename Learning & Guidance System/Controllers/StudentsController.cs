using Learning_Guidance_System.Data;
using Learning_Guidance_System.Dtos;
using Learning_Guidance_System.Models;
using Learning_Guidance_System.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Learning_Guidance_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly DashboardService _dashboardService;

        public StudentsController(AppDbContext context, DashboardService dashboardService)
        {
            _context = context;
            _dashboardService = dashboardService;
        }

        // POST: api/students
        [HttpPost]
        public async Task<ActionResult<Student>> CreateStudent([FromBody] StudentCreateDto dto)
        {
            var student = new Student
            {
                Name = dto.Name,
                Email = dto.Email,
                Department = dto.Department,
                TargetCgpa = dto.TargetCgpa,
                TotalSemesters = dto.TotalSemesters
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
        }

        // GET: api/students/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.SemesterResults)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                return NotFound();

            return student;
        }

        // POST: api/students/{id}/semester-results
        [HttpPost("{id}/semester-results")]
        public async Task<ActionResult> AddOrUpdateSemesterResult(int id, [FromBody] SemesterResultDto dto)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound("Student not found");

            var existing = await _context.SemesterResults
                .FirstOrDefaultAsync(r => r.StudentId == id && r.SemesterNumber == dto.SemesterNumber);

            if (existing == null)
            {
                var result = new SemesterResult
                {
                    StudentId = id,
                    SemesterNumber = dto.SemesterNumber,
                    SemesterGpa = dto.SemesterGpa
                };
                _context.SemesterResults.Add(result);
            }
            else
            {
                existing.SemesterGpa = dto.SemesterGpa;
                _context.SemesterResults.Update(existing);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // GET: api/students/{id}/semester-results
        [HttpGet("{id}/semester-results")]
        public async Task<ActionResult<IEnumerable<SemesterResultDto>>> GetSemesterResults(int id)
        {
            var exists = await _context.Students.AnyAsync(s => s.Id == id);
            if (!exists)
                return NotFound("Student not found");

            var results = await _context.SemesterResults
                .Where(r => r.StudentId == id)
                .OrderBy(r => r.SemesterNumber)
                .Select(r => new SemesterResultDto
                {
                    SemesterNumber = r.SemesterNumber,
                    SemesterGpa = r.SemesterGpa
                })
                .ToListAsync();

            return results;
        }

        // GET: api/students/{id}/dashboard
        [HttpGet("{id}/dashboard")]
        public async Task<ActionResult<DashboardResultDto>> GetDashboard(int id)
        {
            try
            {
                var dashboard = await _dashboardService.GetDashboardAsync(id);
                return dashboard;
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
