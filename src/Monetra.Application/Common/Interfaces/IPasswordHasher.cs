namespace Monetra.Application.Common.Interfaces;

/// <summary>
/// Serviço de hash de senhas.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
