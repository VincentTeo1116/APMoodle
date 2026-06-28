using APMoodle.Models;

namespace APMoodle.Services.Interfaces
{
    public interface ISessionService
    {
        // Start a new quiz attempt and return its SessionID
        Task<int> StartSessionAsync(int studentId, int quizId);

        // Save the student's answers for an existing session, mark it completed, compute TotalScore
        Task<bool> SubmitSessionAsync(int sessionId, List<AnswerSubmission> answers);

        // Fetch a single session with its quiz, questions, and results (for the result page)
        Task<Session?> GetSessionWithDetailsAsync(int sessionId);

        // List all completed sessions for a student (for QuizHistory page)
        Task<List<Session>> GetSessionsByStudentAsync(int studentId);

        // Check if the session belongs to the given student (authorization guard)
        Task<bool> IsSessionOwnedByStudentAsync(int sessionId, int studentId);
        // Get sessions for a specific quiz, optionally filtered by student
        Task<List<Session>> GetSessionsByQuizAsync(int quizId, int? studentId = null);
    }

    // DTO used when the student submits their attempt
    public class AnswerSubmission
    {
        public int QuestionID { get; set; }
        public string Answer { get; set; } = string.Empty; // "A" | "B" | "C" | "D" | "" (unanswered)
        public int TimeUsed { get; set; } // seconds
    }
}
