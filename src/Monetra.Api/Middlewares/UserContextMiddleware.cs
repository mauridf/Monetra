using System.Security.Claims;
using Monetra.Core.Interfaces;

namespace Monetra.Api.Middlewares;

/// <summary>
/// Middleware que extrai informações do usuário autenticado
/// e as disponibiliza via ICurrentUserService.
/// </summary>
public class UserContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserContextMiddleware> _logger;

    public UserContextMiddleware(RequestDelegate next, ILogger<UserContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUserService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var emailClaim = context.User.FindFirst(ClaimTypes.Email)?.Value;
            var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                currentUserService.SetUser(userId, emailClaim ?? "unknown", roleClaim ?? "user");

                _logger.LogDebug(
                    "Usuário autenticado: {UserId} ({Email}) - Role: {Role}",
                    userId, emailClaim, roleClaim);
            }
        }

        await _next(context);
    }
}
