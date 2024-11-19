using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealTimeQuizApp.Data;
using RealTimeQuizApp.Hubs;
namespace RealTimeQuizApp.Services
{
    public class CountdownService
    {
        private readonly IHubContext<CountdownHub> _hubContext;
        private readonly ApplicationDbContext _context;
        public CountdownService(IHubContext<CountdownHub> hubContext, ApplicationDbContext context)
        {
            _hubContext = hubContext;
            _context = context;
        }

        public async Task StartCountDown(Guid QuizId)
        {
            var quiz = _context.Quizzes
                .Where(q => q.QuizId == QuizId)
                .Select(q => new
                {
                    q.QuizId,
                    QuestionsCount = q.Questions.Count
                })
                .FirstOrDefault();

            if (quiz == null)
                throw new ArgumentException("Invalid QuizId");

            int durationInMinutes = quiz.QuestionsCount;
            int durationInSeconds = durationInMinutes * 60;

            for(int i=durationInSeconds; i>=0; i--)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveCountdown", i);
                await Task.Delay(1000);
            }
            await _hubContext.Clients.All.SendAsync("Time limit reached!", quiz.QuizId);
        }
    }
}
