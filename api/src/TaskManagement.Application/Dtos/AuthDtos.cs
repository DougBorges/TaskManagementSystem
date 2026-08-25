namespace TaskManagement.Application.Dtos;

public class UserRegisterDto
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class UserLoginDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class AuthResponseDto
{
    public required string AccessToken { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
}
