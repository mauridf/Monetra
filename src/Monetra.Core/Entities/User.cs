using Monetra.Core.Enums;
using Monetra.Core.Events;
using Monetra.Core.Exceptions;
using Monetra.Core.ValueObjects;

namespace Monetra.Core.Entities;

public class User : AggregateRoot<Guid>
{
    // Propriedades básicas
    public string Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;

    // Autenticação
    public DateTime? EmailVerifiedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? LastPasswordChangeAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public string? TwoFactorSecret { get; private set; }

    // Role e Status
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsPremium { get; private set; }
    public DateTime? PremiumUntil { get; private set; }

    // Preferências
    public string Currency { get; private set; } = null!;
    public int FiscalYearStart { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Relacionamentos
    public Person? Person { get; private set; }
    private readonly List<BankAccount> _bankAccounts = new();
    public IReadOnlyCollection<BankAccount> BankAccounts => _bankAccounts.AsReadOnly();

    private readonly List<Wallet> _wallets = new();
    public IReadOnlyCollection<Wallet> Wallets => _wallets.AsReadOnly();

    private readonly List<CreditCard> _creditCards = new();
    public IReadOnlyCollection<CreditCard> CreditCards => _creditCards.AsReadOnly();

    private readonly List<TransactionCategory> _categories = new();
    public IReadOnlyCollection<TransactionCategory> Categories => _categories.AsReadOnly();

    // Construtor privado (EF Core)
    private User() { }

    private User(string name, Email email, string passwordHash)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Role = UserRole.User;
        IsActive = true;
        IsPremium = false;
        Currency = "BRL";
        FiscalYearStart = 1;
        FailedLoginAttempts = 0;
        TwoFactorEnabled = false;
    }

    /// <summary>
    /// Factory method para criar novo usuário.
    /// </summary>
    public static User Create(string name, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome é obrigatório.");

        var emailObj = Email.Create(email);

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Hash da senha é obrigatório.");

        var user = new User(name.Trim(), emailObj, passwordHash);

        user.AddDomainEvent(new UserRegisteredEvent(user.Id, user.Name, user.Email.Value, DateTime.UtcNow));

        return user;
    }

    /// <summary>
    /// Marca email como verificado.
    /// </summary>
    public void VerifyEmail()
    {
        if (EmailVerifiedAt.HasValue)
            throw new DomainException("Email já foi verificado.");

        EmailVerifiedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>
    /// Registra tentativa de login bem-sucedida.
    /// </summary>
    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockedUntil = null;
        SetUpdatedAt();
    }

    /// <summary>
    /// Registra tentativa de login falha.
    /// </summary>
    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= 5)
        {
            LockedUntil = DateTime.UtcNow.AddMinutes(15);
            throw new DomainException("Conta bloqueada por 15 minutos devido a múltiplas tentativas de login falhas.");
        }

        SetUpdatedAt();
    }

    /// <summary>
    /// Verifica se a conta está bloqueada.
    /// </summary>
    public bool IsLocked()
    {
        return LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
    }

    /// <summary>
    /// Habilita autenticação de dois fatores.
    /// </summary>
    public void EnableTwoFactor(string secret)
    {
        if (TwoFactorEnabled)
            throw new DomainException("2FA já está habilitado.");

        TwoFactorEnabled = true;
        TwoFactorSecret = secret;
        SetUpdatedAt();
    }

    /// <summary>
    /// Desabilita autenticação de dois fatores.
    /// </summary>
    public void DisableTwoFactor()
    {
        TwoFactorEnabled = false;
        TwoFactorSecret = null;
        SetUpdatedAt();
    }

    /// <summary>
    /// Altera a senha do usuário.
    /// </summary>
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        LastPasswordChangeAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>
    /// Ativa ou desativa o usuário (admin).
    /// </summary>
    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        SetUpdatedAt();
    }

    /// <summary>
    /// Define status premium do usuário.
    /// </summary>
    public void SetPremium(bool isPremium, DateTime? premiumUntil)
    {
        IsPremium = isPremium;
        PremiumUntil = premiumUntil;
        Role = isPremium ? UserRole.PremiumUser : UserRole.User;
        SetUpdatedAt();
    }

    /// <summary>
    /// Soft delete do usuário (LGPD - direito ao esquecimento).
    /// </summary>
    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        IsActive = false;
        Name = "[Usuário Removido]";
        Email = Email.Create($"deleted_{Id}@removed.com");
        SetUpdatedAt();
    }
}
