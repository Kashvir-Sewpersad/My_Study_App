using System.ComponentModel.DataAnnotations;

namespace My_Study_App.Models
{
    public class StudyPlanEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Topic { get; set; }

        public string? Notes { get; set; }

        public List<Flashcard> Flashcards { get; set; } = new();
    }

    public class FlashcardCreateViewModel
    {
        public int StudyPlanId { get; set; }

        [Required]
        public string Question { get; set; }

        [Required]
        public string Answer { get; set; }
    }
}