using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Monetra.Core.Entities;
using Monetra.Core.Enums;
using Monetra.Infrastructure.Data;
using Monetra.Infrastructure.Data.Interceptors;
using Monetra.Infrastructure.Repositories;

namespace Monetra.Tests.Integration.Repositories;

public class TransactionRepositoryTests : IDisposable
{
    private readonly MonetraDbContext _context;
    private readonly TransactionRepository _sut;
    private readonly Guid _userId;

    public TransactionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<MonetraDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MonetraDbContext(options,
            new AuditInterceptor(NullLogger<AuditInterceptor>.Instance),
            new DomainEventInterceptor(new StubPublisher(), NullLogger<DomainEventInterceptor>.Instance));
        _sut = new TransactionRepository(_context);
        _userId = Guid.NewGuid();

        SeedData();
    }

    [Fact]
    public async Task GetFilteredAsync_WithoutFilters_ShouldReturnAllUserTransactions()
    {
        var (items, total) = await _sut.GetFilteredAsync(_userId);

        total.Should().Be(3);
        items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetFilteredAsync_WithTypeFilter_ShouldFilterByType()
    {
        var (items, total) = await _sut.GetFilteredAsync(_userId, type: TransactionType.Expense);

        total.Should().Be(2);
        items.Should().AllSatisfy(t => t.TransactionType.Should().Be(TransactionType.Expense));
    }

    [Fact]
    public async Task GetFilteredAsync_WithDateRange_ShouldFilterByDate()
    {
        var start = new DateOnly(2026, 7, 1);
        var end = new DateOnly(2026, 7, 15);

        var (items, total) = await _sut.GetFilteredAsync(_userId, startDate: start, endDate: end);

        total.Should().Be(2);
    }

    [Fact]
    public async Task GetPendingAsync_ShouldReturnOnlyPendingTransactions()
    {
        var pending = await _sut.GetPendingAsync(_userId);

        pending.Should().AllSatisfy(t => t.Status.Should().Be(TransactionStatus.Pending));
    }

    [Fact]
    public async Task GetMonthlyTotalsAsync_ShouldCalculateCorrectly()
    {
        var (income, expense) = await _sut.GetMonthlyTotalsAsync(_userId, 2026, 7);

        income.Should().Be(5000m);
        expense.Should().Be(350m);
    }

    [Fact]
    public async Task GetFilteredAsync_WithSearch_ShouldFilterByDescription()
    {
        var (items, total) = await _sut.GetFilteredAsync(_userId, search: "salário");

        total.Should().Be(1);
        items.First().Description.Should().Contain("salário");
    }

    private void SeedData()
    {
        var accountId = Guid.NewGuid();
        var account = BankAccount.Create(_userId, "Conta Teste", "Checking");
        _context.BankAccounts.Add(account);
        _context.SaveChanges();

        var tx1 = Transaction.Create(_userId, accountId, 5000m, "Income",
            new DateOnly(2026, 7, 5), "Salário julho");
        tx1.Pay(new DateOnly(2026, 7, 5));

        var tx2 = Transaction.Create(_userId, accountId, 200m, "Expense",
            new DateOnly(2026, 7, 10), "Supermercado");
        tx2.Pay(new DateOnly(2026, 7, 10));

        var tx3 = Transaction.Create(_userId, accountId, 150m, "Expense",
            new DateOnly(2026, 7, 20), "Conta de luz",
            dueDate: new DateOnly(2026, 7, 25));

        var tx4 = Transaction.Create(_userId, accountId, 100m, "Expense",
            new DateOnly(2026, 8, 1), "Assinatura Netflix");

        _context.Transactions.AddRange(tx1, tx2, tx3, tx4);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

public class StubPublisher : IPublisher
{
    public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => Task.CompletedTask;
}
