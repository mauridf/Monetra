using FluentAssertions;
using Monetra.Core.Entities;
using Monetra.Core.Enums;
using Monetra.Core.Exceptions;

namespace Monetra.Tests.Unit.Core.Entities;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var user = User.Create("João Silva", "joao@email.com", "hash123");

        user.Should().NotBeNull();
        user.Name.Should().Be("João Silva");
        user.Email.Value.Should().Be("joao@email.com");
        user.Role.Should().Be(UserRole.User);
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldThrow()
    {
        var act = () => User.Create("João", "email-invalido", "hash123");
        act.Should().Throw<DomainException>().WithMessage("*Email*");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrow()
    {
        var act = () => User.Create("", "joao@email.com", "hash123");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void VerifyEmail_ShouldSetVerifiedAt()
    {
        var user = CreateValidUser();
        user.VerifyEmail();

        user.EmailVerifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void RecordFailedLogin_ShouldIncrementAttempts()
    {
        var user = CreateValidUser();
        user.RecordFailedLogin();

        user.FailedLoginAttempts.Should().Be(1);
    }

    [Fact]
    public void RecordFailedLogin_After5Attempts_ShouldLockAccount()
    {
        var user = CreateValidUser();
        for (int i = 0; i < 4; i++)
            user.RecordFailedLogin();

        var act = () => user.RecordFailedLogin();
        act.Should().Throw<DomainException>().WithMessage("*bloqueada*");

        user.IsLocked().Should().BeTrue();
    }

    [Fact]
    public void RecordSuccessfulLogin_ShouldResetAttempts()
    {
        var user = CreateValidUser();
        for (int i = 0; i < 3; i++)
            user.RecordFailedLogin();

        user.RecordSuccessfulLogin();

        user.FailedLoginAttempts.Should().Be(0);
        user.LastLoginAt.Should().NotBeNull();
    }

    [Fact]
    public void EnableTwoFactor_ShouldSetSecret()
    {
        var user = CreateValidUser();
        user.EnableTwoFactor("SECRET123");

        user.TwoFactorEnabled.Should().BeTrue();
        user.TwoFactorSecret.Should().Be("SECRET123");
    }

    [Fact]
    public void DisableTwoFactor_ShouldClearSecret()
    {
        var user = CreateValidUser();
        user.EnableTwoFactor("SECRET123");
        user.DisableTwoFactor();

        user.TwoFactorEnabled.Should().BeFalse();
        user.TwoFactorSecret.Should().BeNull();
    }

    [Fact]
    public void ChangePassword_ShouldUpdateHash()
    {
        var user = CreateValidUser();
        user.ChangePassword("novohash456");

        user.PasswordHash.Should().Be("novohash456");
        user.LastPasswordChangeAt.Should().NotBeNull();
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAt()
    {
        var user = CreateValidUser();
        user.SoftDelete();

        user.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void IsLocked_WhenLockedUntilInPast_ShouldNotBeLocked()
    {
        var user = CreateValidUser();
        for (int i = 0; i < 4; i++)
            user.RecordFailedLogin();

        var act = () => user.RecordFailedLogin();
        act.Should().Throw<DomainException>().WithMessage("*bloqueada*");

        user.IsLocked().Should().BeTrue();

        user.RecordSuccessfulLogin();
        user.IsLocked().Should().BeFalse();
    }

    private static User CreateValidUser()
    {
        return User.Create("Usuário Teste", "teste@email.com", "hash123");
    }
}
