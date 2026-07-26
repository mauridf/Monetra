using Monetra.Core.Entities;
using Monetra.Core.Enums;

namespace Monetra.Core.Specifications;

public class MonthlyTransactionsSpecification
{
    public static Func<Transaction, bool> Create(int year, int month)
    {
        return t => t.TransactionDate.Year == year
            && t.TransactionDate.Month == month
            && t.DeletedAt == null;
    }

    public static decimal GetTotalIncome(IEnumerable<Transaction> transactions)
    {
        return transactions
            .Where(t => t.TransactionType == TransactionType.Income)
            .Sum(t => t.Amount);
    }

    public static decimal GetTotalExpense(IEnumerable<Transaction> transactions)
    {
        return transactions
            .Where(t => t.TransactionType == TransactionType.Expense)
            .Sum(t => t.Amount);
    }
}
