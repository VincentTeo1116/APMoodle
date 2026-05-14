using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Services
{
    public class QuizService : IQuizService
    {
        private readonly ApplicationDbContext _context;

        public QuizService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Quiz>> GetQuizzesByMaterialIdAsync(int materialId)
        {
            return await _context.Quizzes
                .Where(q => q.MaterialID == materialId)
                .OrderBy(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<Quiz?> GetQuizByIdAsync(int quizId)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.QuizID == quizId);
        }

        public async Task<bool> CreateQuizAsync(Quiz quiz)
        {
            try
            {
                quiz.CreatedAt = DateTime.UtcNow;
                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateQuizAsync(Quiz quiz)
        {
            try
            {
                _context.Quizzes.Update(quiz);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteQuizAsync(int quizId)
        {
            try
            {
                var quiz = await _context.Quizzes.FindAsync(quizId);
                if (quiz == null) return false;
                
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Question>> GetQuestionsByQuizIdAsync(int quizId)
        {
            return await _context.Questions
                .Where(q => q.QuizID == quizId)
                .ToListAsync();
        }
    }
}