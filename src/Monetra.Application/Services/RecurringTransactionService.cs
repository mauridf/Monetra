using Microsoft.Extensions.Logging;
using Monetra.Core.Entities;
using Monetra.Core.Enums;
using Monetra.Core.Interfaces;

namespace Monetra.Application.Services;

public class RecurringTransactionService
{
    private readonly IRepository<RecurringTransaction> _recurringRepo;
    private readonly IRepository<Transaction> _transactionRepo;
    private readonly IRepository<BankAccount> _accountRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RecurringTransactionService> _logger;

    public RecurringTransactionService(
        IRepository<RecurringTransaction> recurringRepo,
        IRepository<Transaction> transactionRepo,
        IRepository<BankAccount> accountRepo,
        IUnitOfWork unitOfWork,
        ILogger<RecurringTransactionService> logger)
    {
        _recurringRepo = recurringRepo;
        _transactionRepo = transactionRepo;
        _accountRepo = accountRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Processa transações recorrentes que vencem hoje.
    /// </summary>
    public async Task<int> ProcessDueRecurrencesAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Buscar recorrências ativas com vencimento hoje
        var dueRecurrences = await _recurringRepo.FindAsync(
            r => r.IsActive && r.AutoCreate && r.NextExecution <= today,
            cancellationToken);

        var createdCount = 0;

        foreach (var recurring in dueRecurrences)
        {
            try
            {
                // Verificar se conta bancária ainda existe e está ativa
                var account = await _accountRepo.GetByIdAsync(recurring.BankAccountId, cancellationToken);
                if (account == null || !account.IsActive)
                {
                    _logger.LogWarning("Conta bancária inativa/excluída para recorrência {RecurrenceId}", recurring.Id);
                    recurring.Pause(); // Pausar recorrência se conta não existe mais
                    continue;
                }

                // Criar transação
                var transaction = Transaction.Create(
                    recurring.UserId,
                    recurring.BankAccountId,
                    recurring.Amount,
                    recurring.TransactionType.ToString(),
                    today,
                    recurring.Description,
                    recurring.CategoryId,
                    paymentMethod: PaymentMethod.Transfer.ToString()
                );

                // Atualizar saldo da conta
                if (recurring.TransactionType == TransactionType.Income)
                {
                    account.Credit(recurring.Amount);
                    transaction.Pay(today);
                }
                else
                {
                    account.Debit(recurring.Amount);
                    transaction.Pay(today);
                }

                transaction.SetBalances(
                    account.Balance - recurring.Amount,
                    account.Balance);

                await _transactionRepo.AddAsync(transaction, cancellationToken);

                // Registrar execução e calcular próxima data
                recurring.RecordExecution();

                _recurringRepo.Update(recurring);
                _accountRepo.Update(account);

                createdCount++;

                _logger.LogInformation(
                    "Transação recorrente criada: {RecurrenceId} -> {TransactionId}",
                    recurring.Id, transaction.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar recorrência {RecurrenceId}", recurring.Id);
            }
        }

        if (createdCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("{Count} transações recorrentes processadas", createdCount);
        }

        return createdCount;
    }
}
