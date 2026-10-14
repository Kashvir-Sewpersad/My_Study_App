using System.ComponentModel.DataAnnotations;

namespace My_Study_App.Models
{
    public class Flashcard
    {
        public int Id { get; set; }

        [Required]
        public string Question { get; set; }

        [Required]
        public string Answer { get; set; }

        public int StudyPlanId { get; set; }

        public StudyPlan? StudyPlan { get; set; }
    }
}