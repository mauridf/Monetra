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

public class WalletRepositoryTests : IDisposable
{
    private readonly MonetraDbContext _context;
    private readonly WalletRepository _sut;
    private readonly Guid _userId;

    public WalletRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<MonetraDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MonetraDbContext(options,
            new AuditInterceptor(NullLogger<AuditInterceptor>.Instance),
            new DomainEventInterceptor(new StubPublisher(), NullLogger<DomainEventInterceptor>.Instance));
        _sut = new WalletRepository(_context);
        _userId = Guid.NewGuid();

        SeedData();
    }

    [Fact]
    public async Task GetActiveByUserAsync_ShouldReturnOnlyActiveWallets()
    {
        var wallets = await _sut.GetActiveByUserAsync(_userId);

        wallets.Should().HaveCount(2);
        wallets.Should().AllSatisfy(w => w.Status.Should().Be(WalletStatus.Active));
        wallets.Should().AllSatisfy(w => w.IsArchived.Should().BeFalse());
    }

    [Fact]
    public async Task GetProgressAsync_ShouldReturnProgressForAllWallets()
    {
        var progress = await _sut.GetProgressAsync(_userId);

        progress.Should().HaveCount(2);
        progress.Should().Contain(p => p.Name == "Reserva de Emergência");
    }

    private void SeedData()
    {
        var activeWallet = Wallet.Create(_userId, "Reserva de Emergência",
            "EmergencyFund", 10000m, null, "Reserva financeira");
        activeWallet.Contribute(2500m, "Aporte inicial");

        var activeWallet2 = Wallet.Create(_userId, "Viagem Europa",
            "Goal", 15000m, new DateOnly(2027, 6, 1), "Sonho");
        activeWallet2.Contribute(5000m, "Acumulado");

        var completedWallet = Wallet.Create(_userId, "Concluída",
            "Goal", 1000m, null, "");
        completedWallet.Contribute(1000m, "Completo");
        completedWallet.Complete();

        _context.Wallets.AddRange(activeWallet, activeWallet2, completedWallet);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
