namespace My_Study_App.Models
{
    public class StudySession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public long LoginTicks { get; set; }      // identifies which login this session belongs to
        public DateTime StartedAt { get; set; }
        public DateTime LastUpdated { get; set; }
        public int ActiveSeconds { get; set; }
    }

    public class SaveTimeRequest
    {
        public int ActiveSeconds { get; set; }
    }
}