namespace Monetra.Core.Interfaces;

/// <summary>
/// Serviço para acessar informações do usuário atual.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// ID do usuário autenticado.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Email do usuário autenticado.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Role do usuário autenticado.
    /// </summary>
    string? Role { get; }

    /// <summary>
    /// Verifica se usuário está autenticado.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Verifica se usuário é admin.
    /// </summary>
    bool IsAdmin { get; }

    /// <summary>
    /// Verifica se usuário é premium.
    /// </summary>
    bool IsPremium { get; }

    /// <summary>
    /// Define informações do usuário atual.
    /// </summary>
    void SetUser(Guid userId, string email, string role);
}
