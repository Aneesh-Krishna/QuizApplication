using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using RealTimeQuizApp.Data;
using RealTimeQuizApp.Models;
using RealTimeQuizApp.Services;

namespace RealTimeQuizApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly CountdownService _countdownService;
        public QuizController(ApplicationDbContext context, CountdownService countdownService)
        {
            _context = context;
            _countdownService = countdownService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllQuizzes()
        {
            var quizzes = await _context.Quizzes.Select(q => new
            {
                q.QuizId,
                q.Title,
                q.StartTime,
                q.EndTime
            })
                .ToListAsync();

            return Ok(quizzes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuizById(Guid id)
        {
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == id);
            if (quiz == null)
                return NotFound();

            return Ok(quiz);
        }

        [HttpGet("byGroup/{groupId}")]
        public async Task<IActionResult> GetQuizzesByGroup(Guid groupId)
        {
            var quizzes = await _context.Quizzes
                .Where(q => q.GroupId == groupId)
                .Select(q => new
                {
                    q.GroupId,
                    q.QuizId,
                    q.Title,
                    q.StartTime,
                    q.EndTime
                })
                .ToListAsync();
            if (quizzes == null)
                return NotFound("No quizzes in the group!");

            return Ok(quizzes);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuiz(Guid id, [FromBody] Quiz quiz)
        {
            var existingQuiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == id);
            if(existingQuiz == null)
                return NotFound();

            existingQuiz.Title = quiz.Title;
            existingQuiz.EndTime = quiz.EndTime;

            await _context.SaveChangesAsync();
            return Ok(existingQuiz);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(Guid id)
        {
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == id);
            if (quiz == null) return NotFound();

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
            return Ok("Quiz deleted successfully!");
        }

        [HttpPost("StartCountdown")]
        public async Task<IActionResult> StartCountdown(Guid quizId)
        {
            try
            {
                await _countdownService.StartCountDown(quizId);
                return Ok();
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
