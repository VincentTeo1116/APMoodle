using APMoodle.Models;

namespace APMoodle.Services.Interfaces
{
    public interface IChatService
    {
        // Chat Session operations
        Task<ChatSession?> GetOrCreateChatSessionAsync(int studentId, int lecturerId);
        Task<List<ChatSession>> GetStudentChatSessionsAsync(int studentId);
        Task<List<ChatSession>> GetLecturerChatSessionsAsync(int lecturerId);
        Task<ChatSession?> GetChatSessionByIdAsync(int chatId);
        
        // Message operations
        Task<List<Message>> GetMessagesAsync(int chatId);
        Task<Message> SendMessageAsync(int chatId, int senderId, string senderRole, string content);
        
        // Read status
        Task<int> GetUnreadCountForChatAsync(int chatId, int userId, string role);
        Task MarkMessagesAsReadAsync(int chatId, int userId, string role);
    }
}