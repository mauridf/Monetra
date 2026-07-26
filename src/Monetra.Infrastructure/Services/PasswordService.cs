using Monetra.Application.Common.Interfaces;

namespace Monetra.Infrastructure.Services;

/// <summary>
/// Serviço de hash e verificação de senhas usando BCrypt.
/// </summary>
public class PasswordService : IPasswordHasher
{
    private const int WorkFactor = 12; // Salt factor >= 12 conforme requisito de segurança

    /// <summary>
    /// Gera hash BCrypt da senha.
    /// </summary>
    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Senha não pode ser vazia.", nameof(password));

        // Validar força da senha
        if (password.Length < 8)
            throw new ArgumentException("Senha deve ter no mínimo 8 caracteres.");

        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
            throw new ArgumentException("Senha deve conter letras e números.");

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    /// <summary>
    /// Verifica se a senha corresponde ao hash.
    /// </summary>
    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
