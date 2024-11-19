namespace RealTimeQuizApp.Models
{
    public class Answer
    {
        public Guid AnswerId { get; set; }
        public required string Text { get; set; }
        public bool IsCorrect { get; set; }
        public Guid QuestionId { get; set; }
        public Question? Question { get; set; }
    }
}
