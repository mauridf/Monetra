using Monetra.Core.Interfaces;

namespace Monetra.Infrastructure.Services;

/// <summary>
/// Implementação do serviço de usuário atual (scoped).
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; private set; }
    public string? Email { get; private set; }
    public string? Role { get; private set; }
    public bool IsAuthenticated => UserId.HasValue;
    public bool IsAdmin => Role == "admin";
    public bool IsPremium => Role == "premium_user";

    public void SetUser(Guid userId, string email, string role)
    {
        UserId = userId;
        Email = email;
        Role = role;
    }
}
