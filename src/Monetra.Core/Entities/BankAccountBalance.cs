using Monetra.Core.Exceptions;

namespace Monetra.Core.Entities;

public class BankAccountBalance : Entity<Guid>
{
    public Guid BankAccountId { get; private set; }
    public BankAccount BankAccount { get; private set; } = null!;
    public decimal Balance { get; private set; }
    public DateOnly BalanceDate { get; private set; }

    private BankAccountBalance() { }

    private BankAccountBalance(Guid bankAccountId, decimal balance, DateOnly balanceDate)
    {
        Id = Guid.NewGuid();
        BankAccountId = bankAccountId;
        Balance = balance;
        BalanceDate = balanceDate;
    }

    public static BankAccountBalance Create(Guid bankAccountId, decimal balance, DateOnly balanceDate)
    {
        if (bankAccountId == Guid.Empty)
            throw new DomainException("BankAccountId é obrigatório.");

        return new BankAccountBalance(bankAccountId, balance, balanceDate);
    }
}
