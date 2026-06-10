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
    public class QuizSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }

        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        public int ParticipantCount { get; set; }

        public int DurationMinutes { get; set; }

        [InverseProperty("QuizSession")]
        public virtual ICollection<FastAnswerEntry> FastAnswerEntries { get; set; }
            = new ObservableCollection<FastAnswerEntry>();
    }
}