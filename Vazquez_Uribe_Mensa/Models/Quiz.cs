using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Vazquez_Uribe_Mensa.Models
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [InverseProperty("Quiz")]
        public virtual ICollection<Question> Questions { get; set; }
            = new ObservableCollection<Question>();

        [InverseProperty("Quiz")]
        public virtual ICollection<QuizSession> QuizSessions { get; set; }
            = new ObservableCollection<QuizSession>();
    }
}