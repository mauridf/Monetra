using Monetra.Core.Exceptions;

namespace Monetra.Core.ValueObjects;

public class DateRange : ValueObject
{
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }

    private DateRange(DateOnly startDate, DateOnly endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public static DateRange Create(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
            throw new DomainException("Data final não pode ser anterior à data inicial.");

        return new DateRange(startDate, endDate);
    }

    public bool Overlaps(DateRange other)
    {
        return StartDate <= other.EndDate && EndDate >= other.StartDate;
    }

    public bool Contains(DateOnly date)
    {
        return date >= StartDate && date <= EndDate;
    }

    public int DaysBetween()
    {
        return EndDate.DayNumber - StartDate.DayNumber;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }

    public override string ToString() => $"{StartDate:yyyy-MM-dd} até {EndDate:yyyy-MM-dd}";
}
