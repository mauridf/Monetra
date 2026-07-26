using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Monetra.Application.Common.Interfaces;
using Monetra.Core.Entities;
using Monetra.Core.Enums;
using Monetra.Core.Exceptions;
using Monetra.Core.Interfaces;

namespace Monetra.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ITwoFactorService twoFactorService,
        IUnitOfWork unitOfWork,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _twoFactorService = twoFactorService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Registra um novo usuário.
    /// </summary>
    public async Task<User> RegisterAsync(string name, string email, string password, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Registrando novo usuário: {Email}", email);

        // Verificar se email já existe
        if (await _userRepository.EmailExistsAsync(email, cancellationToken))
            throw new ConflictException("Email já está em uso.");

        // Criar hash da senha
        var passwordHash = _passwordHasher.Hash(password);

        // Criar usuário
        var user = User.Create(name, email, passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Usuário registrado com sucesso: {UserId}", user.Id);

        return user;
    }

    /// <summary>
    /// Realiza login do usuário.
    /// </summary>
    public async Task<(string AccessToken, string RefreshToken, User User)> LoginAsync(
        string email, string password, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Tentativa de login: {Email}", email);

        // Buscar usuário
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken)
            ?? throw new UnauthorizedException("Email ou senha inválidos.");

        // Verificar se conta está ativa
        if (!user.IsActive)
            throw new UnauthorizedException("Conta desativada. Entre em contato com o suporte.");

        // Verificar bloqueio
        if (user.IsLocked())
            throw new UnauthorizedException("Conta bloqueada temporariamente. Tente novamente em alguns minutos.");

        // Verificar senha
        if (!_passwordHasher.Verify(password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("Senha inválida para usuário: {Email}", email);
            throw new UnauthorizedException("Email ou senha inválidos.");
        }

        // Login bem-sucedido
        user.RecordSuccessfulLogin();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Gerar tokens
        var claims = GenerateClaims(user);
        var accessToken = _tokenService.GenerateAccessToken(claims);
        var refreshToken = _tokenService.GenerateRefreshToken();

        _logger.LogInformation("Login bem-sucedido: {UserId}", user.Id);

        return (accessToken, refreshToken, user);
    }

    /// <summary>
    /// Atualiza access token usando refresh token.
    /// </summary>
    public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(
        string accessToken, string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedException("Token inválido.");

            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId), cancellationToken)
                ?? throw new UnauthorizedException("Usuário não encontrado.");

            if (!user.IsActive)
                throw new UnauthorizedException("Conta desativada.");

            // Gerar novos tokens
            var claims = GenerateClaims(user);
            var newAccessToken = _tokenService.GenerateAccessToken(claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            _logger.LogInformation("Token atualizado para usuário: {UserId}", user.Id);

            return (newAccessToken, newRefreshToken);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Tentativa de refresh token inválida");
            throw new UnauthorizedException("Token inválido ou expirado.");
        }
    }

    /// <summary>
    /// Verifica email do usuário.
    /// </summary>
    public async Task VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        // Token de verificação contém o ID do usuário
        var principal = _tokenService.GetPrincipalFromExpiredToken(token);
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedException("Token de verificação inválido.");

        var user = await _userRepository.GetByIdAsync(Guid.Parse(userId), cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");

        user.VerifyEmail();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email verificado para usuário: {UserId}", user.Id);
    }

    /// <summary>
    /// Habilita autenticação de dois fatores (2FA).
    /// </summary>
    public async Task<string> EnableTwoFactorAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");

        // Gerar segredo TOTP
        var base32Secret = _twoFactorService.GenerateSecretKey();

        user.EnableTwoFactor(base32Secret);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("2FA habilitado para usuário: {UserId}", userId);

        return base32Secret;
    }

    /// <summary>
    /// Verifica código 2FA.
    /// </summary>
    public async Task<bool> VerifyTwoFactorAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");

        if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
            throw new DomainException("2FA não está habilitado.");

        return _twoFactorService.VerifyCode(user.TwoFactorSecret, code);
    }

    /// <summary>
    /// Gera claims JWT para o usuário.
    /// </summary>
    private static List<Claim> GenerateClaims(User user)
    {
        return new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email.Value),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("isPremium", user.IsPremium.ToString().ToLower()),
            new("emailVerified", (user.EmailVerifiedAt.HasValue).ToString().ToLower()),
            new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new("jti", Guid.NewGuid().ToString())
        };
    }
}
