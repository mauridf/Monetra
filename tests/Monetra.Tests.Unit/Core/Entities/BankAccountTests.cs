using FluentAssertions;
using Monetra.Core.Entities;
using Monetra.Core.Enums;
using Monetra.Core.Exceptions;

namespace Monetra.Tests.Unit.Core.Entities;

public class BankAccountTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var account = BankAccount.Create(
            Guid.NewGuid(), "Nubank", "Checking");

        account.Should().NotBeNull();
        account.Name.Should().Be("Nubank");
        account.Balance.Should().Be(0);
        account.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithBalance_ShouldSetInitialBalance()
    {
        var account = BankAccount.Create(
            Guid.NewGuid(), "Itaú", "Checking",
            initialBalance: 1000m, balanceDate: DateOnly.FromDateTime(DateTime.UtcNow));

        account.Balance.Should().Be(1000m);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowDomainException()
    {
        var act = () => BankAccount.Create(Guid.NewGuid(), "", "Checking");
        act.Should().Throw<DomainException>().WithMessage("*obrigatório*");
    }

    [Fact]
    public void Credit_ShouldIncreaseBalance()
    {
        var account = CreateValidAccount();
        account.Credit(500m);

        account.Balance.Should().Be(500m);
    }

    [Fact]
    public void Debit_ShouldDecreaseBalance()
    {
        var account = CreateValidAccount(1000m);
        account.Debit(300m);

        account.Balance.Should().Be(700m);
    }

    [Fact]
    public void Debit_WhenInsufficientBalance_ShouldThrowException()
    {
        var account = CreateValidAccount(100m);

        var act = () => account.Debit(200m);
        act.Should().Throw<InsufficientBalanceException>();
    }

    [Fact]
    public void Archive_ShouldSetIsArchived()
    {
        var account = CreateValidAccount();
        account.Archive();

        account.IsArchived.Should().BeTrue();
    }

    [Fact]
    public void Update_ShouldModifyFields()
    {
        var account = CreateValidAccount();
        account.Update("Novo Nome", AccountType.Savings, "Bradesco", color: "#FF0000");

        account.Name.Should().Be("Novo Nome");
        account.AccountType.Should().Be(AccountType.Savings);
        account.BankName.Should().Be("Bradesco");
        account.Color.Should().Be("#FF0000");
    }

    private static BankAccount CreateValidAccount(decimal balance = 0)
    {
        return BankAccount.Create(
            Guid.NewGuid(),
            "Conta Corrente",
            "Checking",
            initialBalance: balance,
            balanceDate: balance > 0 ? DateOnly.FromDateTime(DateTime.UtcNow) : null);
    }
}
