using FluentAssertions;
using Monetra.Core.Entities;
using Monetra.Core.Enums;
using Monetra.Core.Exceptions;

namespace Monetra.Tests.Unit.Core.Entities;

public class WalletTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var wallet = Wallet.Create(
            Guid.NewGuid(), "Reserva de Emergência", "EmergencyFund",
            10000m, null, "Reserva para 6 meses");

        wallet.Should().NotBeNull();
        wallet.Name.Should().Be("Reserva de Emergência");
        wallet.Status.Should().Be(WalletStatus.Active);
    }

    [Fact]
    public void Contribute_ShouldIncreaseCurrentAmount()
    {
        var wallet = CreateValidWallet();
        wallet.Contribute(500m, "Primeira contribuição");

        wallet.CurrentAmount.Should().Be(500m);
    }

    [Fact]
    public void Contribute_WhenExceedsTarget_ShouldThrow()
    {
        var wallet = CreateValidWallet(target: 1000m);
        wallet.Contribute(800m, "Parcial");

        var act = () => wallet.Contribute(300m, "Excesso");
        act.Should().Throw<DomainException>().WithMessage("*excede*");
    }

    [Fact]
    public void Contribute_WithAmountLessThanMinimum_ShouldThrow()
    {
        var wallet = CreateValidWallet();
        var act = () => wallet.Contribute(5m, "Muito pouco");

        act.Should().Throw<DomainException>().WithMessage("*mínima*");
    }

    [Fact]
    public void Withdraw_ShouldDecreaseCurrentAmount()
    {
        var wallet = CreateValidWallet(target: 10000m);
        wallet.Contribute(5000m, "Acumulado");
        wallet.Withdraw(1000m, "Emergência médica");

        wallet.CurrentAmount.Should().Be(4000m);
    }

    [Fact]
    public void Withdraw_WhenExceedsCurrent_ShouldThrow()
    {
        var wallet = CreateValidWallet();
        wallet.Contribute(500m, "Poupança");

        var act = () => wallet.Withdraw(600m, "Retirada maior que saldo");
        act.Should().Throw<DomainException>().WithMessage("*insuficiente*");
    }

    [Fact]
    public void Complete_ShouldAutoCompleteWhenTargetReached()
    {
        var wallet = CreateValidWallet(target: 1000m);
        wallet.Contribute(1000m, "Completo");

        wallet.Status.Should().Be(WalletStatus.Completed);
        wallet.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_WhenNotReachedTarget_ShouldSucceed()
    {
        var wallet = CreateValidWallet(target: 10000m);
        wallet.Contribute(500m, "Parcial");

        wallet.Complete();

        wallet.Status.Should().Be(WalletStatus.Completed);
    }

    [Fact]
    public void GetProgressPercentage_ShouldReturnCorrectValue()
    {
        var wallet = CreateValidWallet(target: 1000m);
        wallet.Contribute(250m, "25%");

        var progress = wallet.GetProgressPercentage();
        progress.Should().Be(25m);
    }

    [Fact]
    public void Update_ShouldModifyFields()
    {
        var wallet = CreateValidWallet();
        wallet.Update("Nova Meta", 50000m, null, "Descrição atualizada");

        wallet.Name.Should().Be("Nova Meta");
        wallet.TargetAmount.Should().Be(50000m);
    }

    private static Wallet CreateValidWallet(decimal target = 10000m)
    {
        return Wallet.Create(
            Guid.NewGuid(), "Meta Teste", "Goal",
            target, null, "Descrição da meta");
    }
}
