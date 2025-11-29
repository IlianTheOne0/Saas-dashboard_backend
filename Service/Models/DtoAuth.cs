namespace Service.Models;

public class AuthDto
{
    public string? name { get; set; } = String.Empty;
    public string? password { get; set; } = String.Empty;
    public string email { get; set; } = String.Empty;
}

public class AuthNewPasswordDto
{
    public string password { get; set; } = String.Empty;
    public string accessToken { get; set; } = String.Empty;
    public string refreshToken { get; set; } = String.Empty;
}