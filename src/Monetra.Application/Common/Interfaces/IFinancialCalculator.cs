namespace Monetra.Application.Common.Interfaces;

/// <summary>
/// Serviço para cálculos financeiros.
/// </summary>
public interface IFinancialCalculator
{
    /// <summary>
    /// Calcula valor mensal necessário para atingir meta.
    /// </summary>
    decimal CalculateMonthlyNeeded(decimal targetAmount, decimal currentAmount, DateOnly targetDate);

    /// <summary>
    /// Calcula percentual de progresso.
    /// </summary>
    decimal CalculateProgress(decimal current, decimal target);

    /// <summary>
    /// Calcula projeção de conclusão baseada em contribuições mensais.
    /// </summary>
    int CalculateMonthsToGoal(decimal remaining, decimal monthlyContribution);
}
