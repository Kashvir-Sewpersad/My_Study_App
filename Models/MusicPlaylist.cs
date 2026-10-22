using System.ComponentModel.DataAnnotations;

namespace My_Study_App.Models
{
    public class MusicPlaylist
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Url { get; set; } = string.Empty;        // what the user pasted

        [Required]
        [MaxLength(20)]
        public string Provider { get; set; } = string.Empty;   // YouTube / Spotify / SoundCloud

        [Required]
        [MaxLength(600)]
        public string EmbedUrl { get; set; } = string.Empty;   // built by the server, never by the user
    }
}