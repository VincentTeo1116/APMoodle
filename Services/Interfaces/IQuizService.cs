using APMoodle.Models;

namespace APMoodle.Services.Interfaces
{
    public interface IQuizService
    {
        Task<List<Quiz>> GetQuizzesByMaterialIdAsync(int materialId);
        Task<Quiz?> GetQuizByIdAsync(int quizId);
        Task<bool> CreateQuizAsync(Quiz quiz);
        Task<bool> UpdateQuizAsync(Quiz quiz);
        Task<bool> DeleteQuizAsync(int quizId);
        Task<List<Question>> GetQuestionsByQuizIdAsync(int quizId);
    }
}