using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealTimeQuizApp.Data;
using RealTimeQuizApp.Models;
using System.Security.Claims;

namespace RealTimeQuizApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public QuestionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllQuestions()
        {
            var questions = _context.Questions.Select(q => new
            {
                q.QuizId,
                q.QuestionId,
                q.Text
            });

            return Ok(questions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuestionById(Guid id)
        {
            var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == id);
            if (question == null)
                return BadRequest("Question not found");

            return Ok(question);
        }

        [HttpGet("byQuiz/{quizId}")]
        public async Task<IActionResult> GetQuestionsByQuiz(Guid quizId)
        {
            var questions = await _context.Questions
                .Where(q => q.QuizId == quizId)
                .Select(q => new
                {
                    q.QuestionId,
                    q.Text
                })
                .ToListAsync();

            return Ok(questions);
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuestion([FromBody] Question question)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var group = await _context.Groups.FirstOrDefaultAsync(q => q.GroupId == question.Quiz.GroupId);
            if (group == null)
                return BadRequest();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return BadRequest();

            if (group.AdminId != userId)
                return Unauthorized("You're not the group admin!");

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return Ok("Question created!");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] Question question)
        {
            var existingQuestion = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == id);
            if(existingQuestion == null)
                return NotFound();


            var group = await _context.Groups.FirstOrDefaultAsync(q => q.GroupId == existingQuestion.Quiz.GroupId);
            if (group == null)
                return BadRequest();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return BadRequest();

            if (group.AdminId != userId)
                return Unauthorized("You're not the group admin!");

            existingQuestion.Text = question.Text;
            await _context.SaveChangesAsync();
            
            return Ok(existingQuestion);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(Guid id)
        {
            var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == id);
            if (question == null)
                return NotFound();

            var group = await _context.Groups.FirstOrDefaultAsync(q => q.GroupId == question.Quiz.GroupId);
            if (group == null)
                return BadRequest();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return BadRequest();

            if (group.AdminId != userId)
                return Unauthorized("You're not the group admin!");

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return Ok("Question deleted!");
        }
    }
}
