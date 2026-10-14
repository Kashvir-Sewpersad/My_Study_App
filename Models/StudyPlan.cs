using System.ComponentModel.DataAnnotations;

namespace My_Study_App.Models
{
    public class StudyPlan
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Topic { get; set; }

        public string? Notes { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Flashcard> Flashcards { get; set; } = new();
    }
}