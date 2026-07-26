using System.Security.Claims;

namespace Monetra.Application.Common.Interfaces;

/// <summary>
/// Serviço de geração e validação de tokens JWT.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Gera access token JWT.
    /// </summary>
    string GenerateAccessToken(IEnumerable<Claim> claims);

    /// <summary>
    /// Gera refresh token.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Obtém claims do token expirado.
    /// </summary>
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

    /// <summary>
    /// Tempo de expiração do access token.
    /// </summary>
    int AccessTokenExpirationMinutes { get; }

    /// <summary>
    /// Tempo de expiração do refresh token.
    /// </summary>
    int RefreshTokenExpirationDays { get; }
}
