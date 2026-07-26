using FluentAssertions;
using Monetra.Core.Entities;
using Monetra.Core.Enums;
using Monetra.Core.Exceptions;

namespace Monetra.Tests.Unit.Core.Entities;

public class TransactionTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var transaction = Transaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), 100m, "Expense",
            DateOnly.FromDateTime(DateTime.UtcNow), "Compra mercado", null, null, null, null);

        transaction.Should().NotBeNull();
        transaction.Id.Should().NotBeEmpty();
        transaction.Status.Should().Be(TransactionStatus.Pending);
        transaction.Amount.Should().Be(100m);
        transaction.Description.Should().Be("Compra mercado");
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldThrowDomainException()
    {
        var act = () => Transaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), 0m, "Expense",
            DateOnly.FromDateTime(DateTime.UtcNow), "Teste");

        act.Should().Throw<DomainException>().WithMessage("*maior que zero*");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowDomainException()
    {
        var act = () => Transaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), -50m, "Expense",
            DateOnly.FromDateTime(DateTime.UtcNow), "Teste");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithEmptyDescription_ShouldThrowDomainException()
    {
        var act = () => Transaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), 100m, "Expense",
            DateOnly.FromDateTime(DateTime.UtcNow), "");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Pay_WhenPending_ShouldChangeStatusToCompleted()
    {
        var transaction = CreateValidTransaction();
        transaction.Pay(DateOnly.FromDateTime(DateTime.UtcNow));

        transaction.Status.Should().Be(TransactionStatus.Completed);
        transaction.PaidDate.Should().NotBeNull();
    }

    [Fact]
    public void Pay_WhenAlreadyCompleted_ShouldThrowDomainException()
    {
        var transaction = CreateValidTransaction();
        transaction.Pay(DateOnly.FromDateTime(DateTime.UtcNow));

        var act = () => transaction.Pay(DateOnly.FromDateTime(DateTime.UtcNow));
        act.Should().Throw<DomainException>().WithMessage("*já está paga*");
    }

    [Fact]
    public void Pay_WhenCancelled_ShouldThrowDomainException()
    {
        var transaction = CreateValidTransaction();
        transaction.Cancel();

        var act = () => transaction.Pay(DateOnly.FromDateTime(DateTime.UtcNow));
        act.Should().Throw<DomainException>().WithMessage("*cancelada*");
    }

    [Fact]
    public void Cancel_WhenPending_ShouldChangeStatusToCancelled()
    {
        var transaction = CreateValidTransaction();
        transaction.Cancel();

        transaction.Status.Should().Be(TransactionStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenCompleted_ShouldSucceed()
    {
        var transaction = CreateValidTransaction();
        transaction.Pay(DateOnly.FromDateTime(DateTime.UtcNow));

        transaction.Cancel();

        transaction.Status.Should().Be(TransactionStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenReconciled_ShouldThrowDomainException()
    {
        var transaction = CreateValidTransaction();
        transaction.Pay(DateOnly.FromDateTime(DateTime.UtcNow));
        transaction.Reconcile();

        var act = () => transaction.Cancel();
        act.Should().Throw<DomainException>().WithMessage("*estorno*");
    }

    [Fact]
    public void Reconcile_ShouldSetIsReconciledToTrue()
    {
        var transaction = CreateValidTransaction();
        transaction.Reconcile();

        transaction.IsReconciled.Should().BeTrue();
    }

    [Fact]
    public void Reconcile_WhenCancelled_ShouldThrowDomainException()
    {
        var transaction = CreateValidTransaction();
        transaction.Cancel();

        var act = () => transaction.Reconcile();
        act.Should().Throw<DomainException>().WithMessage("*cancelada*");
    }

    [Fact]
    public void SetBalances_ShouldUpdateBalances()
    {
        var transaction = CreateValidTransaction();
        transaction.SetBalances(1000m, 900m);

        transaction.BalanceBefore.Should().Be(1000m);
        transaction.BalanceAfter.Should().Be(900m);
    }

    [Fact]
    public void Update_ShouldModifyFields()
    {
        var transaction = CreateValidTransaction();
        var newDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        transaction.Update(200m, newDate, "Descrição atualizada", null, null, null, null);

        transaction.Amount.Should().Be(200m);
        transaction.Description.Should().Be("Descrição atualizada");
        transaction.TransactionDate.Should().Be(newDate);
    }

    [Fact]
    public void Update_WhenReconciled_ShouldThrowDomainException()
    {
        var transaction = CreateValidTransaction();
        transaction.Reconcile();

        var act = () => transaction.Update(200m, DateOnly.FromDateTime(DateTime.UtcNow), "teste", null, null, null, null);
        act.Should().Throw<DomainException>().WithMessage("*conciliada*");
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAt()
    {
        var transaction = CreateValidTransaction();
        transaction.SoftDelete();

        transaction.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Create_IncomeTransaction_ShouldHaveCorrectType()
    {
        var transaction = Transaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), 5000m, "Income",
            DateOnly.FromDateTime(DateTime.UtcNow), "Salário");

        transaction.TransactionType.Should().Be(TransactionType.Income);
    }

    [Fact]
    public void Create_ExpenseTransaction_ShouldHaveCorrectType()
    {
        var transaction = Transaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), 150m, "Expense",
            DateOnly.FromDateTime(DateTime.UtcNow), "Conta de luz");

        transaction.TransactionType.Should().Be(TransactionType.Expense);
    }

    [Fact]
    public void Create_WithDueDate_ShouldSetDueDate()
    {
        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15));
        var transaction = Transaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), 100m, "Expense",
            DateOnly.FromDateTime(DateTime.UtcNow), "Assinatura",
            dueDate: dueDate);

        transaction.DueDate.Should().Be(dueDate);
    }

    [Fact]
    public void Create_WithPaymentMethod_ShouldSetPaymentMethod()
    {
        var transaction = Transaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), 100m, "Expense",
            DateOnly.FromDateTime(DateTime.UtcNow), "Compra",
            paymentMethod: "Pix");

        transaction.PaymentMethod.Should().Be(PaymentMethod.Pix);
    }

    [Fact]
    public void Create_WithTags_ShouldSetTags()
    {
        var tags = new[] { "essencial", "recorrente" };
        var transaction = Transaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), 100m, "Expense",
            DateOnly.FromDateTime(DateTime.UtcNow), "Compra",
            tags: [.. tags]);

        transaction.Tags.Should().BeEquivalentTo(tags);
    }

    private static Transaction CreateValidTransaction()
    {
        return Transaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), 100m, "Expense",
            DateOnly.FromDateTime(DateTime.UtcNow), "Transação teste");
    }
}
