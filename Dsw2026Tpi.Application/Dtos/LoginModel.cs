namespace Dsw2026Tpi.Application.Dtos;

public record LoginAdminModel
{
    public record Request(string Email, string Password);
    public record Response(string? Token, string? Role);
}

public record LoginPatientModel
{
    public class Request
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; 
    }
    public record Response(string? Token, string? Role);
}
