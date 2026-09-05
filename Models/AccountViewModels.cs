
using System.ComponentModel.DataAnnotations;

namespace My_Study_App.Models
{
    public class AccountViewModels
    {

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage = "Passwords do not match. Try again")]
        public string ConfirmPassword { get; set; }
    }

    public class loginViewModel {

        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public String Password { get; set; }

        public bool RememberMe { get; set; }
    
    }
}
