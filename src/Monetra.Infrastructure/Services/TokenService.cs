using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Monetra.Application.Common.Interfaces;

namespace Monetra.Infrastructure.Services;

/// <summary>
/// Serviço de geração e validação de tokens JWT.
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _key;
    private readonly string _issuer;
    private readonly string _audience;

    public int AccessTokenExpirationMinutes =>
        int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "15");

    public int RefreshTokenExpirationDays =>
        int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;

        var secretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey não configurada.");

        // Validar tamanho mínimo da chave (512 bits = 64 bytes)
        if (Encoding.UTF8.GetBytes(secretKey).Length < 64)
            throw new InvalidOperationException("JWT SecretKey deve ter pelo menos 512 bits (64 caracteres).");

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        _issuer = configuration["Jwt:Issuer"] ?? "monetra-api";
        _audience = configuration["Jwt:Audience"] ?? "monetra-app";
    }

    /// <summary>
    /// Gera access token JWT.
    /// </summary>
    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = credentials,
            NotBefore = DateTime.UtcNow
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Gera refresh token aleatório seguro.
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    /// <summary>
    /// Obtém claims de um token expirado (para refresh).
    /// </summary>
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateLifetime = false, // Permite token expirado
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Token inválido.");
        }

        return principal;
    }
}
