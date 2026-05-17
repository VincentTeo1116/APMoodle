using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;

        public ChatService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ChatSession?> GetOrCreateChatSessionAsync(int studentId, int lecturerId)
        {
            var chat = await _context.ChatSessions
                .FirstOrDefaultAsync(c => c.StudentID == studentId && c.LecturerID == lecturerId);

            if (chat == null)
            {
                chat = new ChatSession
                {
                    StudentID = studentId,
                    LecturerID = lecturerId,
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow
                };
                _context.ChatSessions.Add(chat);
                await _context.SaveChangesAsync();
            }

            return chat;
        }

        public async Task<List<ChatSession>> GetStudentChatSessionsAsync(int studentId)
        {
            return await _context.ChatSessions
                .Where(c => c.StudentID == studentId)
                .Include(c => c.Lecturer)
                .Include(c => c.Messages!.OrderByDescending(m => m.Timestamp).Take(1))
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();
        }

        public async Task<List<ChatSession>> GetLecturerChatSessionsAsync(int lecturerId)
        {
            return await _context.ChatSessions
                .Where(c => c.LecturerID == lecturerId)
                .Include(c => c.Student)
                .Include(c => c.Messages!.OrderByDescending(m => m.Timestamp).Take(1))
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();
        }

        public async Task<ChatSession?> GetChatSessionByIdAsync(int chatId)
        {
            return await _context.ChatSessions
                .Include(c => c.Student)
                .Include(c => c.Lecturer)
                .FirstOrDefaultAsync(c => c.ChatID == chatId);
        }

        public async Task<List<Message>> GetMessagesAsync(int chatId)
        {
            return await _context.Messages
                .Where(m => m.ChatID == chatId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task<Message> SendMessageAsync(int chatId, int senderId, string senderRole, string content)
        {
            var newMessage = new Message
            {
                ChatID = chatId,
                SenderID = senderId,
                SenderRole = senderRole,
                Content = content,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(newMessage);
            
            // Update last message time
            var chat = await _context.ChatSessions.FindAsync(chatId);
            if (chat != null)
            {
                chat.LastMessageAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return newMessage;
        }

        public async Task<int> GetUnreadCountForChatAsync(int chatId, int userId, string role)
        {
            if (role == "student")
            {
                return await _context.Messages
                    .CountAsync(m => m.ChatID == chatId && m.SenderRole == "lecturer" && !m.IsRead);
            }
            else if (role == "lecturer")
            {
                return await _context.Messages
                    .CountAsync(m => m.ChatID == chatId && m.SenderRole == "student" && !m.IsRead);
            }
            return 0;
        }

        public async Task MarkMessagesAsReadAsync(int chatId, int userId, string role)
        {
            var messages = await _context.Messages
                .Where(m => m.ChatID == chatId && !m.IsRead)
                .ToListAsync();

            foreach (var message in messages)
            {
                // Mark as read if message was sent by the other person
                if ((role == "student" && message.SenderRole == "lecturer") ||
                    (role == "lecturer" && message.SenderRole == "student"))
                {
                    message.IsRead = true;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}