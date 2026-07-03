namespace APMoodle.Models
{
    public class GuestQuizResult
    {
        public string QuizTitle { get; set; } = string.Empty;
        public string QuizSubject { get; set; } = string.Empty;
        public int CorrectCount { get; set; }
        public int TotalQuestions { get; set; }
        public int Score { get; set; }
        public List<GuestQuestionReview> Reviews { get; set; } = new();
    }

    public class GuestQuestionReview
    {
        public string QuestionText { get; set; } = string.Empty;
        public string Option1 { get; set; } = string.Empty;
        public string Option2 { get; set; } = string.Empty;
        public string Option3 { get; set; } = string.Empty;
        public string Option4 { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public string GivenAnswer { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int TimeUsed { get; set; }
    }
}