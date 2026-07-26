using Monetra.Application.Common.Interfaces;

namespace Monetra.Application.Services;

/// <summary>
/// Serviço para cálculos financeiros e projeções.
/// </summary>
public class FinancialCalculatorService : IFinancialCalculator
{
    /// <summary>
    /// Calcula valor mensal necessário para atingir meta até data alvo.
    /// </summary>
    public decimal CalculateMonthlyNeeded(decimal targetAmount, decimal currentAmount, DateOnly targetDate)
    {
        if (currentAmount >= targetAmount)
            return 0;

        var remaining = targetAmount - currentAmount;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Calcular meses restantes
        var monthsRemaining = ((targetDate.Year - today.Year) * 12) + targetDate.Month - today.Month;

        if (monthsRemaining <= 0)
            return remaining; // Se já passou da data, retorna o total restante

        return Math.Round(remaining / monthsRemaining, 2);
    }

    /// <summary>
    /// Calcula percentual de progresso (0-100).
    /// </summary>
    public decimal CalculateProgress(decimal current, decimal target)
    {
        if (target <= 0)
            return 0;

        var progress = (current / target) * 100;
        return Math.Round(Math.Min(progress, 100), 2);
    }

    /// <summary>
    /// Calcula meses para atingir meta baseado em contribuição mensal.
    /// </summary>
    public int CalculateMonthsToGoal(decimal remaining, decimal monthlyContribution)
    {
        if (monthlyContribution <= 0)
            return int.MaxValue;

        return (int)Math.Ceiling(remaining / monthlyContribution);
    }

    /// <summary>
    /// Calcula projeção de saldo futuro.
    /// </summary>
    public decimal CalculateFutureBalance(decimal currentBalance, decimal monthlySavings, int months, decimal annualInterestRate = 0)
    {
        var monthlyRate = annualInterestRate / 12 / 100;
        var futureBalance = (double)currentBalance;

        for (int i = 0; i < months; i++)
        {
            futureBalance = futureBalance * (1 + (double)monthlyRate) + (double)monthlySavings;
        }

        return Math.Round((decimal)futureBalance, 2);
    }

    /// <summary>
    /// Calcula média mensal de gastos.
    /// </summary>
    public decimal CalculateAverageMonthlyExpense(List<decimal> monthlyExpenses)
    {
        if (monthlyExpenses.Count == 0)
            return 0;

        return Math.Round(monthlyExpenses.Average(), 2);
    }

    /// <summary>
    /// Sugere limite de orçamento baseado em médias históricas.
    /// </summary>
    public decimal SuggestBudgetLimit(List<decimal> monthlyExpenses, decimal bufferPercent = 10)
    {
        var average = CalculateAverageMonthlyExpense(monthlyExpenses);
        var buffer = average * (bufferPercent / 100);
        return Math.Round(average + buffer, 2);
    }
}
