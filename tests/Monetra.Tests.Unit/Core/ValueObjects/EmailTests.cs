using FluentAssertions;
using Monetra.Core.Exceptions;
using Monetra.Core.ValueObjects;

namespace Monetra.Tests.Unit.Core.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldSucceed()
    {
        var email = Email.Create("joao@email.com");
        email.Value.Should().Be("joao@email.com");
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldThrow()
    {
        var act = () => Email.Create("invalido");
        act.Should().Throw<DomainException>().WithMessage("*não é válido*");
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldThrow()
    {
        var act = () => Email.Create("");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithNullEmail_ShouldThrow()
    {
        var act = () => Email.Create(null!);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equals_SameEmail_ShouldBeEqual()
    {
        var email1 = Email.Create("user@email.com");
        var email2 = Email.Create("user@email.com");

        email1.Should().Be(email2);
    }

    [Fact]
    public void Equals_DifferentEmail_ShouldNotBeEqual()
    {
        var email1 = Email.Create("user1@email.com");
        var email2 = Email.Create("user2@email.com");

        email1.Should().NotBe(email2);
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        var email = Email.Create("teste@email.com");
        string value = email.Value;
        value.Should().Be("teste@email.com");
    }

    [Fact]
    public void ToString_ReturnsEmailValue()
    {
        var email = Email.Create("teste@email.com");
        email.ToString().Should().Be("teste@email.com");
    }
}
