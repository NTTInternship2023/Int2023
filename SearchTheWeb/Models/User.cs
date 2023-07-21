using System.ComponentModel.DataAnnotations;

namespace SearchTheWeb.Models;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = "";

    public string Email { get; set; } = "";

    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

    public bool IsAdmin { get; set; } = false;
}