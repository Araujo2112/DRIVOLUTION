using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Microsoft.IdentityModel.Tokens;

namespace Drivolution.Services;

// Service responsável pela autenticação, criação de utilizadores,
// alteração de passwords e geração de tokens JWT
public class AuthService : IAuthService
{
    // Lista de roles aceites pela aplicação
    private static readonly string[] ValidRoles = { "admin", "operator", "client", "manager" };

    // Sem caracteres ambíguos (0/O, 1/l/I) para facilitar a leitura/transcrição manual.
    private const string PasswordChars = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";

    // Comprimento das passwords temporárias geradas pelo sistema
    private const int TemporaryPasswordLength = 12;

    // Repository responsável pelo acesso aos utilizadores na base de dados
    private readonly IUserRepository _userRepository;

    // O ASP.NET injeta automaticamente o repository necessário
    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // Autentica um utilizador através do email e da password
    public async Task<AuthResult<LoginResponseDTO>> Login(LoginRequestDTO dto)
    {
        // Confirma que os campos obrigatórios foram preenchidos
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return AuthResult<LoginResponseDTO>.Fail(
                AuthErrorCode.InvalidInput,
                "Email e password são obrigatórios."
            );

        // Procura o utilizador pelo email
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        // Verifica se o utilizador existe e se a password introduzida
        // corresponde ao hash guardado na base de dados
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return AuthResult<LoginResponseDTO>.Fail(
                AuthErrorCode.InvalidCredentials,
                "Credenciais inválidas."
            );

        // Impede o login de contas inativas
        if (user.Status != "active")
            return AuthResult<LoginResponseDTO>.Fail(
                AuthErrorCode.InactiveAccount,
                "Conta inativa."
            );

        // Gera um token JWT para o utilizador autenticado
        var token = GenerateToken(user);

        // Devolve o token e os dados básicos do utilizador
        return AuthResult<LoginResponseDTO>.Ok(
            new LoginResponseDTO
            {
                Token = token,
                User = MapToInfo(user)
            }
        );
    }

    // Cria uma nova conta de utilizador
    public async Task<AuthResult<RegisterResponseDTO>> Register(RegisterRequestDTO dto)
    {
        // Confirma que o nome e o email foram preenchidos
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Name))
            return AuthResult<RegisterResponseDTO>.Fail(
                AuthErrorCode.InvalidInput,
                "Nome e email são obrigatórios."
            );

        // Verifica se a role recebida é permitida
        if (!ValidRoles.Contains(dto.Role))
            return AuthResult<RegisterResponseDTO>.Fail(
                AuthErrorCode.InvalidRole,
                $"Role inválido. Valores aceites: {string.Join(", ", ValidRoles)}."
            );

        // Impede a criação de contas com um email já existente
        if (await _userRepository.GetByEmailAsync(dto.Email) is not null)
            return AuthResult<RegisterResponseDTO>.Fail(
                AuthErrorCode.EmailAlreadyExists,
                "Já existe um utilizador com este email."
            );

        // A password inicial é sempre gerada pelo sistema. O admin que cria a conta
        // nunca a escolhe — apenas a vê uma vez nesta resposta para a entregar à pessoa.
        var temporaryPassword = GenerateTemporaryPassword();

        // Cria a entidade do novo utilizador
        var user = new UserModel
        {
            Name = dto.Name,

            // Guarda o email em minúsculas e sem espaços extra
            Email = dto.Email.ToLower().Trim(),

            // Guarda apenas o hash da password, nunca a password original
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword),

            Role = dto.Role,
            Status = "active",

            // Obriga o utilizador a trocar a password temporária
            MustChangePassword = true,

            CreatedAt = DateTime.UtcNow
        };

        // Guarda o utilizador na base de dados
        var created = await _userRepository.CreateAsync(user);

        // Devolve os dados do utilizador e a password temporária
        return AuthResult<RegisterResponseDTO>.Ok(
            new RegisterResponseDTO
            {
                User = MapToInfo(created),
                TemporaryPassword = temporaryPassword,
            }
        );
    }

    // Altera a password do utilizador autenticado
    public async Task<AuthResult<bool>> ChangePassword(int userId, ChangePasswordRequestDTO dto)
    {
        // Confirma que ambas as passwords foram preenchidas
        if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
            return AuthResult<bool>.Fail(
                AuthErrorCode.InvalidInput,
                "Password atual e nova password são obrigatórias."
            );

        // Impõe um tamanho mínimo para a nova password
        if (dto.NewPassword.Length < 8)
            return AuthResult<bool>.Fail(
                AuthErrorCode.InvalidInput,
                "A nova password deve ter pelo menos 8 caracteres."
            );

        // Procura o utilizador
        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
            return AuthResult<bool>.Fail(
                AuthErrorCode.UserNotFound,
                "Utilizador não encontrado."
            );

        // Confirma que a password atual está correta
        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return AuthResult<bool>.Fail(
                AuthErrorCode.InvalidCurrentPassword,
                "Password atual incorreta."
            );

        // Impede que a nova password seja igual à anterior
        if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
            return AuthResult<bool>.Fail(
                AuthErrorCode.InvalidInput,
                "A nova password tem de ser diferente da atual."
            );

        // Guarda o hash da nova password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        // Indica que o utilizador já deixou de ter uma password temporária
        user.MustChangePassword = false;

        // Guarda as alterações
        await _userRepository.UpdateAsync(user);

        return AuthResult<bool>.Ok(true);
    }

    // Devolve os dados básicos de um utilizador
    public async Task<UserInfoDTO?> GetUserInfo(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        // Se o utilizador existir, converte-o para DTO
        return user is null ? null : MapToInfo(user);
    }

    // Gera uma password temporária aleatória e segura
    private static string GenerateTemporaryPassword()
    {
        // Gera bytes aleatórios usando um gerador criptograficamente seguro
        var bytes = RandomNumberGenerator.GetBytes(TemporaryPasswordLength);

        // Array onde serão guardados os caracteres da password
        var chars = new char[TemporaryPasswordLength];

        // Converte cada byte num carácter da lista permitida
        for (var i = 0; i < TemporaryPasswordLength; i++)
            chars[i] = PasswordChars[bytes[i] % PasswordChars.Length];

        return new string(chars);
    }

    // Gera um token JWT para o utilizador autenticado
    private string GenerateToken(UserModel user)
    {
        // Obtém as configurações do JWT a partir das variáveis de ambiente
        var issuer =
            Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? throw new InvalidOperationException("JWT_ISSUER not set");

        var audience =
            Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? throw new InvalidOperationException("JWT_AUDIENCE not set");

        var secret =
            Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? throw new InvalidOperationException("JWT_SECRET not set");

        // Converte o segredo numa chave de segurança
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secret)
        );

        // Define o algoritmo usado para assinar o token
        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        // Informações guardadas dentro do token
        var claims = new[]
        {
            // ID do utilizador no claim padrão "sub"
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),

            // Email do utilizador
            new Claim(JwtRegisteredClaimNames.Email, user.Email),

            // ID usado pelo ASP.NET para identificar o utilizador
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

            // Role usada nas autorizações dos controllers
            new Claim(ClaimTypes.Role, user.Role),

            // Nome do utilizador
            new Claim("name", user.Name),
        };

        // Cria o token JWT
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,

            // O token expira ao fim de oito horas
            expires: DateTime.UtcNow.AddHours(8),

            signingCredentials: creds
        );

        // Converte o token num texto que pode ser enviado ao frontend
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Converte um UserModel num UserInfoDTO,
    // evitando devolver dados sensíveis como o PasswordHash
    private static UserInfoDTO MapToInfo(UserModel u) => new()
    {
        Id = u.Id,
        Name = u.Name,
        Email = u.Email,
        Role = u.Role,
        Status = u.Status,
        MustChangePassword = u.MustChangePassword,
        CreatedAt = u.CreatedAt
    };
}