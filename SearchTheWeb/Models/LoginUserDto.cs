using SearchTheWeb.Validators;
using System.ComponentModel.DataAnnotations;
 

namespace SearchTheWeb.Models
{
    public class LoginUserDto
    {
        [Required,MinLength(6, ErrorMessage = "Please enter a longer username, at least 6 characters")]
        public string Username { get; set; } = "";
        [Required]
        public string Password { get; set; } = "";
    }
}
