using Monetra.Core.Events;
using Monetra.Core.Exceptions;

namespace Monetra.Core.Entities;

public class Person : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    // Dados pessoais
    public string? Phone { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public string? Occupation { get; private set; }
    public string? MonthlyIncomeRange { get; private set; }

    // Endereço
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string Country { get; private set; }

    private Person() { }

    private Person(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Country = "Brasil";
    }

    /// <summary>
    /// Cria um novo perfil para o usuário.
    /// </summary>
    public static Person Create(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId é obrigatório.");

        var person = new Person(userId);

        person.AddDomainEvent(new ProfileUpdatedEvent(userId, person.Id, DateTime.UtcNow));

        return person;
    }

    /// <summary>
    /// Atualiza dados do perfil.
    /// </summary>
    public void Update(
        string? phone = null,
        DateOnly? birthDate = null,
        string? occupation = null,
        string? monthlyIncomeRange = null,
        string? city = null,
        string? state = null)
    {
        // Validação de idade mínima
        if (birthDate.HasValue)
        {
            var age = DateTime.Now.Year - birthDate.Value.Year;
            if (birthDate.Value > DateOnly.FromDateTime(DateTime.Now.AddYears(-age)))
                age--;

            if (age < 14)
                throw new DomainException("Usuário deve ter pelo menos 14 anos.");
        }

        Phone = phone;
        BirthDate = birthDate;
        Occupation = occupation;
        MonthlyIncomeRange = monthlyIncomeRange;
        City = city;
        State = state;

        SetUpdatedAt();
    }
}
