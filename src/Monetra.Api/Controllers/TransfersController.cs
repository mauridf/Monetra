using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Core.Entities;
using Monetra.Core.Exceptions;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure.Repositories;

namespace Monetra.Api.Controllers;

[Authorize]
public class TransfersController : BaseController
{
    private readonly BankAccountRepository _accountRepo;
    private readonly WalletRepository _walletRepo;
    private readonly IRepository<Transfer> _transferRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public TransfersController(
        BankAccountRepository accountRepo,
        WalletRepository walletRepo,
        IRepository<Transfer> transferRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _accountRepo = accountRepo;
        _walletRepo = walletRepo;
        _transferRepo = transferRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Cria transferência entre contas.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferRequest request)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        // Validar contas
        var fromAccount = await _accountRepo.GetByIdAsync(request.FromAccountId)
            ?? throw new NotFoundException("Conta de origem não encontrada.");

        var toAccount = await _accountRepo.GetByIdAsync(request.ToAccountId)
            ?? throw new NotFoundException("Conta de destino não encontrada.");

        if (fromAccount.Id == toAccount.Id)
            throw new DomainException("Não é possível transferir para a mesma conta.");

        if (fromAccount.Balance < request.Amount)
            throw new InsufficientBalanceException(fromAccount.Balance, request.Amount);

        // Criar transferência
        var transfer = Transfer.CreateBetweenAccounts(
            userId,
            request.FromAccountId,
            request.ToAccountId,
            request.Amount,
            DateOnly.FromDateTime(request.TransferDate),
            request.Description);

        // Criar transações de saída e entrada
        var fromTransaction = Transaction.Create(
            userId, request.FromAccountId, request.Amount,
            "expense", DateOnly.FromDateTime(request.TransferDate),
            $"Transferência para {toAccount.Name}",
            paymentMethod: "transfer");

        var toTransaction = Transaction.Create(
            userId, request.ToAccountId, request.Amount,
            "income", DateOnly.FromDateTime(request.TransferDate),
            $"Transferência de {fromAccount.Name}",
            paymentMethod: "transfer");

        // Atualizar saldos
        fromAccount.Debit(request.Amount);
        toAccount.Credit(request.Amount);

        fromTransaction.SetBalances(fromAccount.Balance + request.Amount, fromAccount.Balance);
        toTransaction.SetBalances(toAccount.Balance - request.Amount, toAccount.Balance);

        fromTransaction.Pay(DateOnly.FromDateTime(request.TransferDate));
        toTransaction.Pay(DateOnly.FromDateTime(request.TransferDate));

        transfer.LinkTransactions(fromTransaction.Id, toTransaction.Id);

        await _transferRepo.AddAsync(transfer);
        await _unitOfWork.SaveChangesAsync();

        return Created(nameof(GetTransfer), new { id = transfer.Id }, new
        {
            TransferId = transfer.Id,
            FromTransactionId = fromTransaction.Id,
            ToTransactionId = toTransaction.Id,
            Amount = transfer.Amount,
            TransferDate = transfer.TransferDate,
            Status = transfer.Status
        });
    }

    /// <summary>
    /// Obtém detalhes de uma transferência.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTransfer(Guid id)
    {
        var transfer = await _transferRepo.GetByIdAsync(id);
        if (transfer == null)
            return NotFound();

        return Ok(new
        {
            transfer.Id,
            transfer.Amount,
            transfer.TransferDate,
            transfer.Description,
            transfer.Status,
            transfer.FromAccountId,
            transfer.ToAccountId,
            transfer.FromTransactionId,
            transfer.ToTransactionId
        });
    }

    /// <summary>
    /// Lista transferências do usuário.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTransfers()
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var transfers = await _transferRepo.FindAsync(t => t.UserId == userId);

        return Ok(transfers.Select(t => new
        {
            t.Id,
            t.Amount,
            t.TransferDate,
            t.Description,
            t.Status
        }));
    }
}

public class CreateTransferRequest
{
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransferDate { get; set; } = DateTime.UtcNow;
    public string? Description { get; set; }
}
