using APMoodle.Models;

namespace APMoodle.Services.Interfaces
{
    public interface IQuizService
    {
        // Quiz-level CRUD
        Task<List<Quiz>> GetQuizzesByMaterialIdAsync(int materialId);
        Task<Quiz?> GetQuizByIdAsync(int quizId);
        Task<bool> CreateQuizAsync(Quiz quiz);
        Task<bool> UpdateQuizAsync(Quiz quiz);
        Task<bool> DeleteQuizAsync(int quizId);

        // Question-level CRUD (used by EditQuiz)
        Task<List<Question>> GetQuestionsByQuizIdAsync(int quizId);
        Task<bool> AddQuestionAsync(Question question);
        Task<bool> UpdateQuestionAsync(Question question);
        Task<bool> DeleteQuestionAsync(int questionId);

        // Atomic Quiz + Questions create (transactional). Returns new QuizID, or 0 on failure.
        Task<int> CreateQuizWithQuestionsAsync(Quiz quiz, List<Question> questions);

        // Ownership lookup for authorization in lecturer pages. Returns the LecturerID
        // who owns the module-of-material-of-quiz, or null if the quiz / chain doesn't exist.
        Task<int?> GetLecturerIdForQuizAsync(int quizId);
    }
}