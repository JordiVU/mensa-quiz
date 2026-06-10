using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vazquez_Uribe_Mensa.Models
{
    public class QuestionOption
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }

        [Required]
        [MaxLength(500)]
        public string Text { get; set; }

        public bool IsCorrect { get; set; }

        public int OrderIndex { get; set; }

        public string? MatchPair { get; set; }

        public string? ImagePath { get; set; }
    }
}