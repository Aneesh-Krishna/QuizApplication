using Microsoft.AspNetCore.Mvc;
using RealTimeQuizApp.Models;
using RealTimeQuizApp.Data;
using Microsoft.EntityFrameworkCore;

namespace RealTimeQuizApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnswerController : Controller
    {
        private ApplicationDbContext _context;
        public AnswerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllAnswers()
        {
            var answers = _context.Answers.Select(a => new
            {
                a.AnswerId,
                a.Text,
                a.QuestionId,
                a.IsCorrect
            });

            return Ok(answers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnswerById(Guid id)
        {
            var answer = await _context.Answers.FirstOrDefaultAsync(a => a.AnswerId == id);
            if (answer == null)
                return NotFound();

            return Ok(answer);
        }

        [HttpGet("byquestion/{questionId}")]
        public IActionResult GetAnswerByQuestionId(Guid questionId)
        {
            var answer = _context.Answers
                .Include(a => a.Question)
                .Where(a => a.QuestionId == questionId)
                .Select(a => new
                {
                    a.AnswerId,
                    a.Text,
                    a.IsCorrect
                });

            if (answer == null)
                return NotFound();

            return Ok(answer);
        }

        [HttpPost("CreateAnswer")]
        public async Task<IActionResult> CreateAnswer([FromBody] Answer answer)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();

            return Ok("Answer created successfully!");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnswer(Guid id, [FromBody] Answer answer)
        {
            var existinganswer = await _context.Answers.FirstOrDefaultAsync(a => a.AnswerId == id);
            if (existinganswer == null)
                return BadRequest();

            existinganswer.Text = answer.Text;
            existinganswer.IsCorrect = answer.IsCorrect;

            _context.SaveChanges();
            return Ok("Answer Updated successfully!");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnswer(Guid id)
        {
            var answer = await _context.Answers.FirstOrDefaultAsync(a => a.AnswerId == id);
            if (answer == null) return NotFound();

            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();

            return Ok("Answer deleted successfully!");
        }
    }
}
