using Monetra.Core.Entities;

namespace Monetra.Core.Specifications;

public class CompletedProfileSpecification
{
    public static bool IsSatisfiedBy(Person person)
    {
        return person != null
            && !string.IsNullOrWhiteSpace(person.Phone)
            && person.BirthDate.HasValue
            && !string.IsNullOrWhiteSpace(person.City)
            && !string.IsNullOrWhiteSpace(person.State);
    }
}
