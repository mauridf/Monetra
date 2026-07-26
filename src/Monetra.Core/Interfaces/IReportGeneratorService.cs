using Monetra.Core.Reports;

namespace Monetra.Core.Interfaces;

public interface IReportGeneratorService
{
    Task<byte[]> GenerateMonthlyReportAsync(MonthlyReportData data, CancellationToken cancellationToken = default);
}
