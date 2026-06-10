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
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }

        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } // MultipleChoice, Ordering, Matching, OpenQuestion, Podium, Break

        public string? Content { get; set; }

        public string? ImagePath { get; set; }

        public int OrderIndex { get; set; }

        [InverseProperty("Question")]
        public virtual ICollection<QuestionOption> QuestionOptions { get; set; }
            = new ObservableCollection<QuestionOption>();
    }
}