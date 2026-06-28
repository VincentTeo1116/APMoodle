using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Services
{
    public class SessionService : ISessionService
    {
        private readonly ApplicationDbContext _context;

        public SessionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> StartSessionAsync(int studentId, int quizId)
        {
            var session = new Session
            {
                StudentID = studentId,
                QuizID = quizId,
                IsCompleted = false,
                TotalScore = null
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();
            return session.SessionID;
        }

        public async Task<bool> SubmitSessionAsync(int sessionId, List<AnswerSubmission> answers)
        {
            try
            {
                var session = await _context.Sessions
                    .Include(s => s.Quiz)
                        .ThenInclude(q => q!.Questions)
                    .FirstOrDefaultAsync(s => s.SessionID == sessionId);

                if (session == null || session.IsCompleted)
                    return false;

                // Build a lookup of correct answers from the quiz the session belongs to
                var questions = session.Quiz?.Questions ?? new List<Question>();
                var correctByQuestionId = questions.ToDictionary(q => q.QuestionID, q => q.CorrectAnswer);

                int correctCount = 0;

                foreach (var submission in answers)
                {
                    // Only accept answers that belong to this quiz's questions
                    if (!correctByQuestionId.TryGetValue(submission.QuestionID, out var correctAnswer))
                        continue;

                    // Normalize to a single uppercase character BEFORE comparing,
                    // so the stored Answer and IsCorrect can never disagree.
                    var raw = (submission.Answer ?? string.Empty).Trim().ToUpper();
                    var givenAnswer = string.IsNullOrEmpty(raw) ? string.Empty : raw.Substring(0, 1);
                    var isCorrect = !string.IsNullOrEmpty(givenAnswer)
                                    && string.Equals(givenAnswer, correctAnswer, StringComparison.OrdinalIgnoreCase);

                    if (isCorrect) correctCount++;

                    _context.Results.Add(new Result
                    {
                        SessionID = sessionId,
                        QuestionID = submission.QuestionID,
                        Answer = string.IsNullOrEmpty(givenAnswer) ? "-" : givenAnswer,
                        TimeUsed = submission.TimeUsed < 0 ? 0 : submission.TimeUsed,
                        IsCorrect = isCorrect
                    });
                }

                // Score = percentage out of 100, rounded
                int totalQuestions = questions.Count;
                session.TotalScore = totalQuestions == 0
                    ? 0
                    : (int)Math.Round((double)correctCount / totalQuestions * 100.0);
                session.IsCompleted = true;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SubmitSessionAsync error: {ex}");
                if (ex.InnerException != null)
                    Console.WriteLine($"  Inner: {ex.InnerException.Message}");
                return false;
            }
        }

        public async Task<Session?> GetSessionWithDetailsAsync(int sessionId)
        {
            return await _context.Sessions
                .Include(s => s.Quiz)
                    .ThenInclude(q => q!.Questions)
                .Include(s => s.Results!)
                    .ThenInclude(r => r.Question)
                .FirstOrDefaultAsync(s => s.SessionID == sessionId);
        }

        public async Task<List<Session>> GetSessionsByStudentAsync(int studentId)
        {
            return await _context.Sessions
                .Where(s => s.StudentID == studentId && s.IsCompleted)
                .Include(s => s.Quiz)
                .OrderByDescending(s => s.Timestamp)
                .ToListAsync();
        }

        public async Task<bool> IsSessionOwnedByStudentAsync(int sessionId, int studentId)
        {
            return await _context.Sessions
                .AnyAsync(s => s.SessionID == sessionId && s.StudentID == studentId);
        }

        public async Task<List<Session>> GetSessionsByQuizAsync(int quizId, int? studentId = null)
        {
            var query = _context.Sessions
                .Where(s => s.QuizID == quizId && s.IsCompleted)
                .Include(s => s.Student)
                .Include(s => s.Quiz)
                .AsQueryable();

            if (studentId.HasValue)
                query = query.Where(s => s.StudentID == studentId.Value);

            return await query
                .OrderByDescending(s => s.Timestamp)
                .ToListAsync();
        }
    }
}
