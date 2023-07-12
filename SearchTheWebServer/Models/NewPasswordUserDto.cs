using SearchTheWebServer.Validators;
using System.ComponentModel.DataAnnotations;

namespace SearchTheWebServer.Models
{
    public class NewPasswordUserDto
    {
        [Required,MinLength(6, ErrorMessage = "Please enter a longer username, at least 6 characters")]
        public string Username { get; set; } = "";
        [Required]
        public string Password { get; set; } = "";
        [Required]
        
        public string NewPassword { get; set; } = "";
    }
}
