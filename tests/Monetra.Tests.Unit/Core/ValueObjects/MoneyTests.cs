using FluentAssertions;
using Monetra.Core.Exceptions;
using Monetra.Core.ValueObjects;

namespace Monetra.Tests.Unit.Core.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidValues_ShouldSucceed()
    {
        var money = Money.Create(100.50m, "BRL");
        money.Amount.Should().Be(100.50m);
        money.CurrencyCode.Should().Be("BRL");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrow()
    {
        var act = () => Money.Create(-10m, "BRL");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithEmptyCurrency_ShouldDefaultToBRL()
    {
        var money = Money.Create(50m);
        money.CurrencyCode.Should().Be("BRL");
    }

    [Fact]
    public void Add_TwoMoneySameCurrency_ShouldSum()
    {
        var m1 = Money.Create(100m, "BRL");
        var m2 = Money.Create(50m, "BRL");

        var result = m1.Add(m2);
        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void Subtract_ShouldReturnDifference()
    {
        var m1 = Money.Create(100m, "BRL");
        var m2 = Money.Create(30m, "BRL");

        var result = m1.Subtract(m2);
        result.Amount.Should().Be(70m);
    }

    [Fact]
    public void Equals_SameAmountAndCurrency_ShouldBeEqual()
    {
        var m1 = Money.Create(100m, "BRL");
        var m2 = Money.Create(100m, "BRL");

        m1.Should().Be(m2);
    }

    [Fact]
    public void ToString_ShouldFormatCorrectly()
    {
        var money = Money.Create(1234.56m, "BRL");
        money.ToString().Should().Be("BRL 1.234,56");
    }
}
