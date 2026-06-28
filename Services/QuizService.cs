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
                .Where(q => q.MaterialID == materialId && q.Status == "Active")
                .OrderBy(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<Quiz?> GetQuizByIdAsync(int quizId)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.Material)
                    .ThenInclude(m => m.Module)
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
            catch (Exception ex)
            {
                Console.WriteLine($"CreateQuizAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateQuizAsync(Quiz quiz)
        {
            try
            {
                var existing = await _context.Quizzes.FindAsync(quiz.QuizID);
                if (existing == null) return false;

                existing.Title = quiz.Title;
                existing.Subject = quiz.Subject;
                existing.Theme = quiz.Theme;
                // MaterialID and CreatedAt are intentionally NOT overwritten on edit.

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateQuizAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteQuizAsync(int quizId)
        {
            // The actual Postgres schema has Results.QuestionID -> Questions set to
            // RESTRICT (not cascade), so deleting a Quiz that has any student attempts
            // would otherwise blow up on the FK constraint. We tear the graph down
            // explicitly inside a transaction in dependency order:
            //   Results -> Sessions -> Questions -> Quiz.
            // This is the expected behaviour of "CRUD Quiz" per proposal item 12 —
            // deleting a quiz removes its attempts too.
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var quiz = await _context.Quizzes.FindAsync(quizId);
                if (quiz == null) return false;

                quiz.Status = "Removed";

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                Console.WriteLine($"DeleteQuizAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Question>> GetQuestionsByQuizIdAsync(int quizId)
        {
            return await _context.Questions
                .Where(q => q.QuizID == quizId && q.Status == "Active")
                .OrderBy(q => q.QuestionID)
                .ToListAsync();
        }

        public async Task<bool> AddQuestionAsync(Question question)
        {
            try
            {
                _context.Questions.Add(question);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddQuestionAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateQuestionAsync(Question question)
        {
            try
            {
                var existing = await _context.Questions.FindAsync(question.QuestionID);
                if (existing == null) return false;

                existing.QuestionText = question.QuestionText;
                existing.Option1 = question.Option1;
                existing.Option2 = question.Option2;
                existing.Option3 = question.Option3;
                existing.Option4 = question.Option4;
                existing.CorrectAnswer = question.CorrectAnswer;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateQuestionAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteQuestionAsync(int questionId)
        {
            try
            {
                var question = await _context.Questions.FindAsync(questionId);
                if (question == null) return false;

                question.Status = "Removed";
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteQuestionAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<int> CreateQuizWithQuestionsAsync(Quiz quiz, List<Question> questions)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Quiz.CreatedAt is mapped to Postgres "timestamp with time zone";
                // it requires DateTimeKind.Utc, which DateTime.UtcNow already provides.
                quiz.CreatedAt = DateTime.UtcNow;
                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();

                foreach (var q in questions)
                {
                    q.QuizID = quiz.QuizID;
                    _context.Questions.Add(q);
                }
                await _context.SaveChangesAsync();

                await tx.CommitAsync();
                return quiz.QuizID;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                Console.WriteLine($"CreateQuizWithQuestionsAsync error: {ex.Message}");
                return 0;
            }
        }

        public async Task<int?> GetLecturerIdForQuizAsync(int quizId)
        {
            // Quiz -> Material -> Module.LecturerID. One round-trip via joins.
            return await _context.Quizzes
                .Where(q => q.QuizID == quizId && q.Status == "Active")
                .Select(q => (int?)q.Material!.Module!.LecturerID)
                .FirstOrDefaultAsync();
        }
    }
}
