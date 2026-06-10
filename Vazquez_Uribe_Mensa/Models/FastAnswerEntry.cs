using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vazquez_Uribe_Mensa.Models
{
    public class FastAnswerEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuizSessionId { get; set; }

        [ForeignKey("QuizSessionId")]
        public virtual QuizSession QuizSession { get; set; }

        [Required]
        [MaxLength(150)]
        public string GroupName { get; set; }

        public string Answer { get; set; }

        public int ResponseTimeSeconds { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
