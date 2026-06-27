using Drivolution.DTO;

namespace Drivolution.Services.Interface;

public enum AuthErrorCode
{
    None,
    InvalidInput,
    InvalidCredentials,
    InactiveAccount,
    InvalidRole,
    EmailAlreadyExists,
    InvalidCurrentPassword,
    UserNotFound,
}

public class AuthResult<T>
{
    public bool Success { get; init; }
    public T? Value { get; init; }
    public AuthErrorCode ErrorCode { get; init; } = AuthErrorCode.None;
    public string? ErrorMessage { get; init; }

    public static AuthResult<T> Ok(T value) => new() { Success = true, Value = value };

    public static AuthResult<T> Fail(AuthErrorCode code, string message) =>
        new() { Success = false, ErrorCode = code, ErrorMessage = message };
}

public interface IAuthService
{
    Task<AuthResult<LoginResponseDTO>> Login(LoginRequestDTO dto);
    Task<AuthResult<RegisterResponseDTO>> Register(RegisterRequestDTO dto);
    Task<AuthResult<bool>> ChangePassword(int userId, ChangePasswordRequestDTO dto);
    Task<UserInfoDTO?> GetUserInfo(int userId);
}