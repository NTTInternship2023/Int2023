namespace SearchTheWebServer.Models;

public class UserDto
{
    public string Username { get; set; } = "";

    public string Password { get; set; } = "";

    public bool IsAdmin { get; set; } = false;
}