using SearchTheWebServer.Validators;
using System.ComponentModel.DataAnnotations;

namespace SearchTheWebServer.Models;

public class RegisterUserDto
{
    
    [Required,MinLength(6,ErrorMessage ="Please enter a longer username, at least 6 characters")]
    public string Username { get; set; } = "";
    [Required]   
    public string Email { get; set; } = "";
    [Required]
    public string Password { get; set; } = "";

    
} 