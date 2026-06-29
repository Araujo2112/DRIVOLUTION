namespace Drivolution.DTO;

public class LoginRequestDTO
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDTO
{
    public string Token { get; set; } = string.Empty;
    public UserInfoDTO User { get; set; } = null!;
}

public class RegisterRequestDTO
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "operator";
}

public class RegisterResponseDTO
{
    public UserInfoDTO User { get; set; } = null!;

    // Mostrada apenas uma vez, na resposta de criação. Nunca é persistida em claro
    // nem voltará a ser devolvida pela API depois deste momento.
    public string TemporaryPassword { get; set; } = string.Empty;
}

public class ChangePasswordRequestDTO
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class UserInfoDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool MustChangePassword { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateUserRequestDTO
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
 
public class ResetPasswordResponseDTO
{
    public string TemporaryPassword { get; set; } = string.Empty;
}

// Usado para popular dropdowns de seleção de cliente (ex: criar encomenda)
public record ClientOptionDTO(int Id, string Name);