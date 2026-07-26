using FluentAssertions;
using Monetra.Application.Services;

namespace Monetra.Tests.Unit.Application.Services;

public class FinancialCalculatorServiceTests
{
    private readonly FinancialCalculatorService _sut = new();

    [Fact]
    public void CalculateMonthlyNeeded_ShouldReturnCorrectValue()
    {
        var needed = _sut.CalculateMonthlyNeeded(12000m, 2000m, new DateOnly(2027, 1, 1));
        // 10000 remaining over 6 months (Jul-Dec)
        needed.Should().BeApproximately(1666.67m, 0.01m);
    }

    [Fact]
    public void CalculateProgress_When50Percent_ShouldReturn50()
    {
        var progress = _sut.CalculateProgress(5000m, 10000m);
        progress.Should().Be(50m);
    }

    [Fact]
    public void CalculateProgress_WhenZeroTarget_ShouldReturnZero()
    {
        var progress = _sut.CalculateProgress(100m, 0m);
        progress.Should().Be(0m);
    }

    [Fact]
    public void CalculateMonthsToGoal_ShouldReturnCorrectCount()
    {
        var months = _sut.CalculateMonthsToGoal(10000m, 1000m);
        months.Should().Be(10);
    }

    [Fact]
    public void CalculateMonthsToGoal_WithZeroContribution_ShouldReturnMaxValue()
    {
        var months = _sut.CalculateMonthsToGoal(10000m, 0m);
        months.Should().Be(int.MaxValue);
    }

    [Fact]
    public void CalculateFutureBalance_ShouldReturnCorrectValue()
    {
        var balance = _sut.CalculateFutureBalance(1000m, 500m, 12, 0.005m);
        balance.Should().BeGreaterThan(7000m);
    }

    [Fact]
    public void CalculateAverageMonthlyExpense_ShouldReturnAverage()
    {
        var expenses = new List<decimal> { 1000m, 2000m, 3000m };
        var avg = _sut.CalculateAverageMonthlyExpense(expenses);
        avg.Should().Be(2000m);
    }

    [Fact]
    public void SuggestBudgetLimit_ShouldReturnAverageWithMargin()
    {
        var expenses = new List<decimal> { 1000m, 2000m, 1500m };
        var limit = _sut.SuggestBudgetLimit(expenses, 10m);
        limit.Should().BeApproximately(1650m, 0.01m);
    }
}
