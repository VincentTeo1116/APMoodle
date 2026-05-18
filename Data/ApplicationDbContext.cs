using Microsoft.EntityFrameworkCore;
using APMoodle.Models;

namespace APMoodle.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for all tables
        public DbSet<Student> Students { get; set; } = null!;

        public DbSet<Lecturer> Lecturers { get; set; } = null!;

        public DbSet<Admin> Admins { get; set; } = null!;

        public DbSet<Module> Modules { get; set; } = null!;

        public DbSet<Material> Materials { get; set; } = null!;

        public DbSet<Quiz> Quizzes { get; set; } = null!;

        public DbSet<Question> Questions { get; set; } = null!;

        public DbSet<Session> Sessions { get; set; } = null!;

        public DbSet<Result> Results { get; set; } = null!;

        public DbSet<Announcement> Announcements { get; set; } = null!;

        public DbSet<Enrollment>? Enrollments { get; set; }

        public DbSet<ChatSession> ChatSessions { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Student Configuration
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(s => s.StudentID);
                entity.HasIndex(s => s.Email).IsUnique();
                entity.Property(s => s.Name).HasMaxLength(100).IsRequired();
                entity.Property(s => s.Email).HasMaxLength(100).IsRequired();
                entity.Property(s => s.PhoneNumber).HasMaxLength(20).IsRequired();
                entity.Property(s => s.Gender).HasMaxLength(10).IsRequired();
                entity.Property(s => s.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Active");
                entity.Property(s => s.ProfilePic).HasMaxLength(500);
                entity.HasIndex(s => s.StudentCode).IsUnique();

                // Relationships
                entity.HasMany(s => s.Sessions)
                    .WithOne(ses => ses.Student)
                    .HasForeignKey(ses => ses.StudentID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Lecturer Configuration 
            modelBuilder.Entity<Lecturer>(entity =>
            {
                entity.HasKey(l => l.LecturerID);
                entity.HasIndex(l => l.Email).IsUnique();
                entity.Property(l => l.Name).HasMaxLength(100).IsRequired();
                entity.Property(l => l.Email).HasMaxLength(100).IsRequired();
                entity.Property(l => l.PhoneNumber).HasMaxLength(20).IsRequired();
                entity.Property(l => l.Department).HasMaxLength(50).IsRequired();
                entity.Property(l => l.Gender).HasMaxLength(10).IsRequired();
                entity.Property(l => l.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Active");
                entity.Property(l => l.ProfilePic).HasMaxLength(500);
                entity.HasIndex(l => l.LecturerCode).IsUnique();

                // Relationships
                entity.HasMany(l => l.Modules)
                    .WithOne(m => m.Lecturer)
                    .HasForeignKey(m => m.LecturerID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Admin Configuration 
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasKey(a => a.AdminID);
                entity.HasIndex(a => a.Email).IsUnique();
                entity.Property(a => a.Name).HasMaxLength(100).IsRequired();
                entity.Property(a => a.Email).HasMaxLength(100).IsRequired();
                entity.Property(a => a.PhoneNumber).HasMaxLength(20).IsRequired();
                entity.Property(a => a.Gender).HasMaxLength(10).IsRequired();
                entity.Property(a => a.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Active");
                entity.Property(a => a.ProfilePic).HasMaxLength(500);
                entity.HasIndex(a => a.AdminCode).IsUnique();

                // Relationships
                entity.HasMany(a => a.Announcements)
                    .WithOne(ann => ann.Creator)
                    .HasForeignKey(ann => ann.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Module Configuration
            modelBuilder.Entity<Module>(entity =>
            {
                entity.HasKey(m => m.ModuleID);
                entity.HasIndex(m => m.ModuleCode).IsUnique();
                entity.Property(m => m.Name).HasMaxLength(100).IsRequired();
                entity.Property(m => m.ModuleCode).HasMaxLength(20).IsRequired();
                entity.Property(m => m.Description).HasMaxLength(1000);
                entity.Property(m => m.InvitationCode).HasMaxLength(20);
                entity.Property(m => m.ModulePic).HasMaxLength(500);
                entity.Property(m => m.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Active");

                // Relationships
                entity.HasOne(m => m.Lecturer)
                    .WithMany(l => l.Modules)
                    .HasForeignKey(m => m.LecturerID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(m => m.Materials)
                    .WithOne(mat => mat.Module)
                    .HasForeignKey(mat => mat.ModuleID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Material Configuration
            modelBuilder.Entity<Material>(entity =>
            {
                entity.HasKey(m => m.MaterialID);
                entity.Property(m => m.Title).HasMaxLength(200).IsRequired();
                entity.Property(m => m.Description).HasMaxLength(1000);
                entity.Property(m => m.FileURL).HasMaxLength(500);
                entity.Property(m => m.ContentType).HasMaxLength(20).HasDefaultValue("file");
                entity.Property(m => m.ContentUrl).HasMaxLength(500);
                entity.Property(m => m.EmbedHtml).HasMaxLength(2000);
                entity.Property(m => m.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(m => m.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Active");

                // Relationships
                entity.HasOne(m => m.Module)
                    .WithMany(mod => mod.Materials)
                    .HasForeignKey(m => m.ModuleID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Quiz Configuration
            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.HasKey(q => q.QuizID);
                entity.Property(q => q.Title).HasMaxLength(200).IsRequired();
                entity.Property(q => q.Subject).HasMaxLength(50).IsRequired();
                entity.Property(q => q.Theme).HasMaxLength(50).IsRequired();
                entity.Property(q => q.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Active");

                // Relationships
                entity.HasOne(q => q.Material)
                    .WithMany(m => m.Quizzes)
                    .HasForeignKey(q => q.MaterialID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(q => q.Questions)
                    .WithOne(qst => qst.Quiz)
                    .HasForeignKey(qst => qst.QuizID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(q => q.Sessions)
                    .WithOne(s => s.Quiz)
                    .HasForeignKey(s => s.QuizID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Question Configuration
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(q => q.QuestionID);
                entity.Property(q => q.QuestionText).HasMaxLength(500).IsRequired();
                entity.Property(q => q.Option1).HasMaxLength(200).IsRequired();
                entity.Property(q => q.Option2).HasMaxLength(200).IsRequired();
                entity.Property(q => q.Option3).HasMaxLength(200).IsRequired();
                entity.Property(q => q.Option4).HasMaxLength(200).IsRequired();
                entity.Property(q => q.CorrectAnswer).HasMaxLength(1).IsRequired();
                entity.Property(q => q.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Active");

                // Relationships
                entity.HasOne(q => q.Quiz)
                    .WithMany(qz => qz.Questions)
                    .HasForeignKey(q => q.QuizID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(q => q.Results)
                    .WithOne(r => r.Question)
                    .HasForeignKey(r => r.QuestionID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Session Configuration
            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasKey(s => s.SessionID);
                entity.Property(s => s.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(s => s.IsCompleted).HasDefaultValue(false);

                // Relationships
                entity.HasOne(s => s.Student)
                    .WithMany(st => st.Sessions)
                    .HasForeignKey(s => s.StudentID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Quiz)
                    .WithMany(q => q.Sessions)
                    .HasForeignKey(s => s.QuizID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(s => s.Results)
                    .WithOne(r => r.Session)
                    .HasForeignKey(r => r.SessionID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Result Configuration
            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasKey(r => r.ResultID);
                entity.Property(r => r.Answer).HasMaxLength(1).IsRequired();
                entity.Property(r => r.IsCorrect).HasDefaultValue(false);
                entity.Property(r => r.TimeUsed).HasDefaultValue(0);

                // Relationships
                entity.HasOne(r => r.Session)
                    .WithMany(s => s.Results)
                    .HasForeignKey(r => r.SessionID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Question)
                    .WithMany(q => q.Results)
                    .HasForeignKey(r => r.QuestionID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Announcement Configuration
            modelBuilder.Entity<Announcement>(entity =>
            {
                entity.HasKey(a => a.AnnouncementID);
                entity.Property(a => a.Title).HasMaxLength(200).IsRequired();
                entity.Property(a => a.Message).HasMaxLength(2000).IsRequired();
                entity.Property(a => a.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(a => a.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Active");

                // Relationships
                entity.HasOne(a => a.Creator)
                    .WithMany(ad => ad.Announcements)
                    .HasForeignKey(a => a.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.EnrollmentID);
                entity.HasIndex(e => new { e.StudentID, e.ModuleID }).IsUnique();

                entity.HasOne(e => e.Student)
                    .WithMany(s => s.Enrollments)
                    .HasForeignKey(e => e.StudentID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Module)
                    .WithMany(m => m.Enrollments)
                    .HasForeignKey(e => e.ModuleID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ChatSession Configuration
            modelBuilder.Entity<ChatSession>(entity =>
            {
                entity.HasKey(c => c.ChatID);
                entity.HasIndex(c => new { c.StudentID, c.LecturerID }).IsUnique();

                entity.HasOne(c => c.Student)
                    .WithMany()
                    .HasForeignKey(c => c.StudentID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Lecturer)
                    .WithMany()
                    .HasForeignKey(c => c.LecturerID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.Messages)
                    .WithOne(m => m.ChatSession)
                    .HasForeignKey(m => m.ChatID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Message Configuration
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(m => m.MessageID);
                entity.HasIndex(m => m.ChatID);
                entity.HasIndex(m => m.Timestamp);
                entity.Property(m => m.SenderRole).HasMaxLength(10);
                entity.Property(m => m.Content).IsRequired();
            });
        }
    }
}