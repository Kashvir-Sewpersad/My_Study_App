using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace My_Study_App.Models
{
    // One row every time a user logs in (password or Google)
    public class LoginRecord
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime LoggedInAtUtc { get; set; }
    }

    // A study plan the user schedules on a specific day
    public class CalendarEvent
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "date")]
        public DateTime EventDate { get; set; }

        [MaxLength(5)]
        public string? StartTime { get; set; }   // "HH:mm", optional

        public string? Notes { get; set; }

        public int? StudyPlanId { get; set; }    // optional link to a study plan

        public bool IsCompleted { get; set; }
    }

    public class CalendarEventRequest
    {
        public string? Title { get; set; }
        public string? Date { get; set; }        // "yyyy-MM-dd"
        public string? StartTime { get; set; }
        public string? Notes { get; set; }
        public int? StudyPlanId { get; set; }
    }
}