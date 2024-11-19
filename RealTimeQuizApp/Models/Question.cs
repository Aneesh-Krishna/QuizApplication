namespace RealTimeQuizApp.Models
{
    public class Question
    {
        public Guid QuestionId { get; set; }
        public required string Text { get; set; }
        public ICollection<Answer>? Answers { get; set; } = new List<Answer>();

        public Guid QuizId { get; set; }
        public Quiz? Quiz { get; set; }

        public int MinOptions { get; set; } = 4;
        public int MaxOptions { get; set; } = 5;
    }
}
