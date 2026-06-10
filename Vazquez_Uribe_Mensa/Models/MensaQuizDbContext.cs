using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Vazquez_Uribe_Mensa.Models
{
    public partial class MensaQuizDbContext : DbContext
    {
        // MODELOS
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Quiz> Quizzes { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<QuestionOption> QuestionOptions { get; set; }
        public virtual DbSet<QuizSession> QuizSessions { get; set; }
        public virtual DbSet<FastAnswerEntry> FastAnswerEntries { get; set; }

        public MensaQuizDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = config.GetConnectionString("MensaQuiz");

            optionsBuilder.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            );

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Password = "$2a$11$nyWPQ2m/9dMvEBeeqF4xkecit2gGNsAb01XMeFwRhASTQ9Z86YkTK",
                    IsActive = true,
                    Role = "Admin",
                    CreatedAt = new DateTime(2026, 1, 1)
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}