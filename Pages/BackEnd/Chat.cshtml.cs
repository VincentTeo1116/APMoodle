using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Services.Interfaces;
using APMoodle.Models;
using Microsoft.AspNetCore.Antiforgery;

namespace APMoodle.Pages.BackEnd
{
    [IgnoreAntiforgeryToken]
    public class ChatModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly IStudentService _studentService;
        private readonly ILecturerService _lecturerService;

        public ChatModel(IChatService chatService, IStudentService studentService, ILecturerService lecturerService)
        {
            _chatService = chatService;
            _studentService = studentService;
            _lecturerService = lecturerService;
        }

        public string UserRole { get; set; } = string.Empty;
        public int UserId { get; set; }
        public List<ChatSession> ChatSessions { get; set; } = new();
        public int? CurrentChatId { get; set; }
        public Lecturer? CurrentLecturer { get; set; }
        public Student? CurrentStudent { get; set; }
        public List<Message> Messages { get; set; } = new();
        public Dictionary<int, int> UnreadCounts { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? chatId, int? lecturerId, int? studentId)
        {
            var sessionUserId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(sessionUserId))
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            UserRole = userRole ?? "Guest";
            UserId = int.Parse(sessionUserId);

            if (UserRole == "student")
            {
                if (lecturerId.HasValue && lecturerId.Value > 0)
                {
                    var chat = await _chatService.GetOrCreateChatSessionAsync(UserId, lecturerId.Value);
                    CurrentChatId = chat.ChatID;
                    CurrentLecturer = await _lecturerService.GetLecturerByIdAsync(lecturerId.Value);
                    Messages = await _chatService.GetMessagesAsync(chat.ChatID);
                }
                else if (chatId.HasValue && chatId.Value > 0)
                {
                    CurrentChatId = chatId;
                    Messages = await _chatService.GetMessagesAsync(chatId.Value);
                    await _chatService.MarkMessagesAsReadAsync(chatId.Value, UserId, "student");
                }

                ChatSessions = await _chatService.GetStudentChatSessionsAsync(UserId);

                foreach (var chat in ChatSessions)
                {
                    var unread = await _chatService.GetUnreadCountForChatAsync(chat.ChatID, UserId, "student");
                    UnreadCounts[chat.ChatID] = unread;
                }

                if (CurrentChatId.HasValue)
                {
                    var currentChat = ChatSessions.FirstOrDefault(c => c.ChatID == CurrentChatId);
                    if (currentChat != null && CurrentLecturer == null)
                    {
                        CurrentLecturer = await _lecturerService.GetLecturerByIdAsync(currentChat.LecturerID);
                    }
                }
            }
            else if (UserRole == "lecturer")
            {
                if (studentId.HasValue && studentId.Value > 0)
                {
                    // Lecturer starting chat with a student
                    var chat = await _chatService.GetOrCreateChatSessionAsync(studentId.Value, UserId);
                    CurrentChatId = chat.ChatID;
                    CurrentStudent = await _studentService.GetStudentByIdAsync(studentId.Value);
                    Messages = await _chatService.GetMessagesAsync(chat.ChatID);
                }
                else if (chatId.HasValue && chatId.Value > 0)
                {
                    CurrentChatId = chatId;
                    Messages = await _chatService.GetMessagesAsync(chatId.Value);
                    await _chatService.MarkMessagesAsReadAsync(chatId.Value, UserId, "lecturer");
                }

                ChatSessions = await _chatService.GetLecturerChatSessionsAsync(UserId);

                foreach (var chat in ChatSessions)
                {
                    var unread = await _chatService.GetUnreadCountForChatAsync(chat.ChatID, UserId, "lecturer");
                    UnreadCounts[chat.ChatID] = unread;
                }

                if (CurrentChatId.HasValue)
                {
                    var currentChat = ChatSessions.FirstOrDefault(c => c.ChatID == CurrentChatId);
                    if (currentChat != null && CurrentStudent == null)
                    {
                        CurrentStudent = await _studentService.GetStudentByIdAsync(currentChat.StudentID);
                    }
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSendMessageAsync([FromBody] SendMessageRequest request)
        {
            var sessionUserId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(sessionUserId))
            {
                return Unauthorized();
            }

            var userId = int.Parse(sessionUserId);
            var chat = await _chatService.GetChatSessionByIdAsync(request.ChatId);

            if (chat == null)
            {
                return BadRequest();
            }

            if ((userRole == "student" && chat.StudentID != userId) ||
                (userRole == "lecturer" && chat.LecturerID != userId))
            {
                return BadRequest();
            }

            var message = await _chatService.SendMessageAsync(request.ChatId, userId, userRole ?? "unknown", request.Content);
            return new JsonResult(new
            {
                success = true,
                messageId = message.MessageID,
                message = message.Content,
                timestamp = message.Timestamp.ToString("o")
            });
        }

        public async Task<IActionResult> OnGetGetMessagesAsync(int chatId)
        {
            var sessionUserId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(sessionUserId))
            {
                return Unauthorized();
            }

            var messages = await _chatService.GetMessagesAsync(chatId);
            return new JsonResult(messages.Select(m => new
            {
                m.MessageID,
                m.Content,
                m.SenderRole,
                Timestamp = m.Timestamp.ToString("o")
            }));
        }
        public async Task<IActionResult> OnGetSearchUsersAsync(string searchTerm, string role)
        {
            if (string.IsNullOrEmpty(searchTerm) || searchTerm.Length < 2)
            {
                return new JsonResult(new { users = new List<object>() });
            }

            var users = new List<object>();

            if (role == "student")
            {
                // Student searches for lecturers
                var lecturers = await _lecturerService.SearchLecturersAsync(searchTerm);
                users = lecturers.Select(l => new
                {
                    id = l.LecturerID,
                    name = l.Name,
                    code = l.LecturerCode,
                    profilePic = l.ProfilePic,
                    avatar = GetAvatarLetter(l.Name)
                }).ToList<object>();
            }
            else if (role == "lecturer")
            {
                // Lecturer searches for students
                var students = await _studentService.SearchStudentsAsync(searchTerm);
                users = students.Select(s => new
                {
                    id = s.StudentID,
                    name = s.Name,
                    code = s.StudentCode,
                    profilePic = s.ProfilePic,
                    avatar = GetAvatarLetter(s.Name)
                }).ToList<object>();
            }

            return new JsonResult(new { users = users });
        }

        private string GetAvatarLetter(string name)
        {
            if (string.IsNullOrEmpty(name)) return "?";
            return name.Trim()[0].ToString().ToUpper();
        }
    }

    public class SendMessageRequest
    {
        public int ChatId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

}