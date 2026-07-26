using Monetra.Core.Enums;
using Monetra.Core.Exceptions;

namespace Monetra.Core.Entities;

public class RecurringTransaction : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid BankAccountId { get; private set; }
    public BankAccount BankAccount { get; private set; } = null!;

    public Guid? CategoryId { get; private set; }
    public TransactionCategory? Category { get; private set; }

    public string Description { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public TransactionType TransactionType { get; private set; }

    // Recorrência
    public RecurrenceType RecurrenceType { get; private set; }
    public int IntervalValue { get; private set; }
    public string? IntervalUnit { get; private set; }
    public int? DayOfMonth { get; private set; }
    public int? DayOfWeekNumber { get; private set; }
    public int? MonthOfYear { get; private set; }

    // Ciclo
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public DateOnly NextExecution { get; private set; }
    public int? MaxExecutions { get; private set; }
    public int ExecutionsCount { get; private set; }

    // Status
    public bool IsActive { get; private set; }
    public bool AutoCreate { get; private set; }
    public int? NotifyBeforeDays { get; private set; }

    // Transações geradas
    private readonly List<Transaction> _generatedTransactions = new();
    public IReadOnlyCollection<Transaction> GeneratedTransactions => _generatedTransactions.AsReadOnly();

    private RecurringTransaction() { }

    private RecurringTransaction(
        Guid userId,
        Guid bankAccountId,
        string description,
        decimal amount,
        TransactionType transactionType,
        RecurrenceType recurrenceType,
        DateOnly startDate,
        DateOnly nextExecution)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        BankAccountId = bankAccountId;
        Description = description;
        Amount = amount;
        TransactionType = transactionType;
        RecurrenceType = recurrenceType;
        StartDate = startDate;
        NextExecution = nextExecution;
        IsActive = true;
        AutoCreate = true;
        ExecutionsCount = 0;
        IntervalValue = 1;
    }

    /// <summary>
    /// Cria uma nova transação recorrente.
    /// </summary>
    public static RecurringTransaction Create(
        Guid userId,
        Guid bankAccountId,
        string description,
        decimal amount,
        string transactionType,
        string recurrenceType,
        DateOnly startDate,
        Guid? categoryId = null,
        DateOnly? endDate = null,
        int? maxExecutions = null,
        int intervalValue = 1,
        int? dayOfMonth = null,
        int? dayOfWeek = null,
        int? monthOfYear = null,
        int? notifyBeforeDays = null)
    {
        if (amount <= 0)
            throw new DomainException("Valor deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Descrição é obrigatória.");

        if (!Enum.TryParse<TransactionType>(transactionType, true, out var txType))
            throw new DomainException($"Tipo de transação inválido: {transactionType}");

        if (!Enum.TryParse<RecurrenceType>(recurrenceType, true, out var recType))
            throw new DomainException($"Tipo de recorrência inválido: {recurrenceType}");

        // Calcular próxima execução
        var nextExecution = CalculateNextExecution(recType, startDate, intervalValue, dayOfMonth, dayOfWeek, monthOfYear);

        var recurring = new RecurringTransaction(
            userId, bankAccountId, description.Trim(), amount, txType, recType, startDate, nextExecution)
        {
            CategoryId = categoryId,
            EndDate = endDate,
            MaxExecutions = maxExecutions,
            IntervalValue = intervalValue,
            DayOfMonth = dayOfMonth,
            DayOfWeekNumber = dayOfWeek,
            MonthOfYear = monthOfYear,
            NotifyBeforeDays = notifyBeforeDays
        };

        return recurring;
    }

    /// <summary>
    /// Atualiza dados da recorrência.
    /// </summary>
    public void Update(
        string description,
        decimal amount,
        Guid? categoryId = null,
        DateOnly? endDate = null,
        int? maxExecutions = null,
        int? notifyBeforeDays = null)
    {
        Description = description.Trim();
        Amount = amount;
        CategoryId = categoryId;
        EndDate = endDate;
        MaxExecutions = maxExecutions;
        NotifyBeforeDays = notifyBeforeDays;
        SetUpdatedAt();
    }

    /// <summary>
    /// Pausa a recorrência.
    /// </summary>
    public void Pause()
    {
        if (!IsActive)
            throw new DomainException("Recorrência já está pausada.");

        IsActive = false;
        SetUpdatedAt();
    }

    /// <summary>
    /// Retoma a recorrência.
    /// </summary>
    public void Resume()
    {
        if (IsActive)
            throw new DomainException("Recorrência já está ativa.");

        IsActive = true;
        NextExecution = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        SetUpdatedAt();
    }

    /// <summary>
    /// Registra uma execução e calcula a próxima.
    /// </summary>
    public void RecordExecution()
    {
        ExecutionsCount++;

        if (MaxExecutions.HasValue && ExecutionsCount >= MaxExecutions.Value)
        {
            IsActive = false;
        }
        else if (EndDate.HasValue && NextExecution >= EndDate.Value)
        {
            IsActive = false;
        }
        else
        {
            NextExecution = CalculateNextExecution(
                RecurrenceType, NextExecution, IntervalValue, DayOfMonth, DayOfWeekNumber, MonthOfYear);
        }

        SetUpdatedAt();
    }

    /// <summary>
    /// Calcula a próxima data de execução.
    /// </summary>
    private static DateOnly CalculateNextExecution(
        RecurrenceType recurrenceType,
        DateOnly currentDate,
        int interval,
        int? dayOfMonth,
        int? dayOfWeek,
        int? monthOfYear)
    {
        return recurrenceType switch
        {
            RecurrenceType.Daily => currentDate.AddDays(interval),
            RecurrenceType.Weekly => currentDate.AddDays(7 * interval),
            RecurrenceType.Monthly => CalculateNextMonthly(currentDate, dayOfMonth ?? currentDate.Day),
            RecurrenceType.Yearly => new DateOnly(
                currentDate.Year + interval,
                monthOfYear ?? currentDate.Month,
                dayOfMonth ?? currentDate.Day),
            _ => currentDate.AddMonths(1)
        };
    }

    private static DateOnly CalculateNextMonthly(DateOnly currentDate, int targetDay)
    {
        var nextMonth = currentDate.AddMonths(1);
        var lastDayOfMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
        var actualDay = Math.Min(targetDay, lastDayOfMonth);

        var nextDate = new DateOnly(nextMonth.Year, nextMonth.Month, actualDay);

        // Se cair em final de semana, ajustar para próximo dia útil
        if (nextDate.DayOfWeek == DayOfWeek.Saturday)
            nextDate = nextDate.AddDays(2);
        else if (nextDate.DayOfWeek == DayOfWeek.Sunday)
            nextDate = nextDate.AddDays(1);

        return nextDate;
    }
}
