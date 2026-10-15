using System.ComponentModel.DataAnnotations;

namespace My_Study_App.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        public string? PasswordHash { get; set; }   // null for Google-only accounts
    }
}