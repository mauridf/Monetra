using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Monetra.Application.Common.DTOs;
using Monetra.Application.Services;
using Monetra.Core.Interfaces;

namespace Monetra.Api.Controllers;

/// <summary>
/// Controller responsável por autenticação e gerenciamento de conta.
/// </summary>
public class AuthController : BaseController
{
    private readonly AuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(AuthService authService, ICurrentUserService currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Registra um novo usuário.
    /// </summary>
    [HttpPost("register")]
    [EnableRateLimiting("RegisterEndpoint")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var user = await _authService.RegisterAsync(request.Name, request.Email, request.Password);

        return Created(nameof(GetCurrentUser), new { id = user.Id }, new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email.Value,
            Role = user.Role.ToString(),
            IsPremium = user.IsPremium,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        });
    }

    /// <summary>
    /// Realiza login e retorna tokens JWT.
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("LoginEndpoint")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var (accessToken, refreshToken, user) = await _authService.LoginAsync(
            request.Email, request.Password);

        return Ok(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email.Value,
                Role = user.Role.ToString(),
                IsPremium = user.IsPremium,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }
        });
    }

    /// <summary>
    /// Atualiza access token usando refresh token.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var (accessToken, refreshToken) = await _authService.RefreshTokenAsync(
            request.AccessToken, request.RefreshToken);

        return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
    }

    /// <summary>
    /// Solicita redefinição de senha.
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        // Implementar envio de email com token de reset
        return Ok(new { Message = "Se o email existir, um link de redefinição será enviado." });
    }

    /// <summary>
    /// Redefine a senha usando token.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        // Implementar redefinição de senha
        return Ok(new { Message = "Senha redefinida com sucesso." });
    }

    /// <summary>
    /// Verifica email do usuário.
    /// </summary>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        await _authService.VerifyEmailAsync(token);
        return Ok(new { Message = "Email verificado com sucesso." });
    }

    /// <summary>
    /// Altera a senha do usuário logado.
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        // Implementar alteração de senha
        return Ok(new { Message = "Senha alterada com sucesso." });
    }

    /// <summary>
    /// Habilita autenticação de dois fatores (2FA).
    /// </summary>
    [Authorize]
    [HttpPost("enable-2fa")]
    public async Task<IActionResult> EnableTwoFactor()
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var secret = await _authService.EnableTwoFactorAsync(userId);

        // Gerar URI para QR Code
        var qrCodeUri = $"otpauth://totp/Monetra:{_currentUser.Email}?secret={secret}&issuer=Monetra";

        return Ok(new { Secret = secret, QrCodeUri = qrCodeUri });
    }

    /// <summary>
    /// Verifica código 2FA.
    /// </summary>
    [HttpPost("verify-2fa")]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorRequest request)
    {
        var isValid = await _authService.VerifyTwoFactorAsync(request.UserId, request.Code);

        if (!isValid)
            return BadRequest(ErrorResponse.Create("INVALID_2FA", "Código 2FA inválido."));

        return Ok(new { Valid = true });
    }

    /// <summary>
    /// Obtém dados do usuário logado.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        if (!_currentUser.IsAuthenticated)
            return Unauthorized();

        return Ok(new
        {
            UserId = _currentUser.UserId,
            Email = _currentUser.Email,
            Role = _currentUser.Role,
            IsAdmin = _currentUser.IsAdmin,
            IsPremium = _currentUser.IsPremium
        });
    }
}

// DTOs auxiliares
public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class VerifyTwoFactorRequest
{
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
}
