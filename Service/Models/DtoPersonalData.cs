namespace Service.Models;

public class PersonalDataResultDto
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; } = string.Empty;
    public string? Role { get; set; } = string.Empty;
    public bool? IsOnline { get; set; } = false;
    public string? Location { get; set; } = string.Empty;
    public string? Timezone { get; set; } = string.Empty;
    public string? Phone { get; set; } = string.Empty;
}

public class UpdateProfileDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Role { get; set; }
    public string? Location { get; set; }
    public string? Timezone { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
}

public class UpdateEmailDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string NewEmail { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class AvatarUpdateDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string Base64Image { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

public class AvatarDeleteDto
{
    public string AccessToken { get; set; } = string.Empty;
}