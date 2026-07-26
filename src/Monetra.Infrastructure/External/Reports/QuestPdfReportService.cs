using Microsoft.Extensions.Logging;
using Monetra.Core.Interfaces;
using Monetra.Core.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Monetra.Infrastructure.External.Reports;

public class QuestPdfReportService : IReportGeneratorService
{
    private readonly ILogger<QuestPdfReportService> _logger;

    public QuestPdfReportService(ILogger<QuestPdfReportService> logger)
    {
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateMonthlyReportAsync(MonthlyReportData data, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Gerando relatório mensal: {Month}/{Year}", data.MonthName, data.Year);

        return await Task.Run(() =>
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Helvetica"));

                    page.Header()
                        .AlignCenter()
                        .PaddingBottom(10)
                        .Text($"Relatório Financeiro - {data.MonthName}/{data.Year}")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Black);

                    page.Header()
                        .AlignCenter()
                        .Text($"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Medium);

                    page.Content().Column(column =>
                    {
                        column.Item().PaddingVertical(10).Row(row =>
                        {
                            row.RelativeItem().Padding(5).Background(Colors.Green.Lighten5).Column(col =>
                            {
                                col.Item().Text("Receitas").FontSize(9).FontColor(Colors.Grey.Darken1);
                                col.Item().Text(data.TotalIncome.ToString("C2")).FontSize(14).Bold().FontColor(Colors.Green.Darken1);
                            });
                            row.RelativeItem().Padding(5).Background(Colors.Red.Lighten5).Column(col =>
                            {
                                col.Item().Text("Despesas").FontSize(9).FontColor(Colors.Grey.Darken1);
                                col.Item().Text(data.TotalExpense.ToString("C2")).FontSize(14).Bold().FontColor(Colors.Red.Darken1);
                            });
                            row.RelativeItem().Padding(5).Background(Colors.Blue.Lighten5).Column(col =>
                            {
                                col.Item().Text("Saldo").FontSize(9).FontColor(Colors.Grey.Darken1);
                                col.Item().Text(data.Balance.ToString("C2")).FontSize(14).Bold().FontColor(Colors.Blue.Darken1);
                            });
                        });

                        column.Item().PaddingTop(15).Text("Gastos por Categoria").FontSize(14).Bold();

                        if (data.CategoryBreakdown.Count > 0)
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(3);
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Padding(3).Text("Categoria").Bold().FontSize(9);
                                    header.Cell().Padding(3).Text("Valor").Bold().FontSize(9).AlignRight();
                                    header.Cell().Padding(3).Text("%").Bold().FontSize(9).AlignRight();
                                });

                                foreach (var cat in data.CategoryBreakdown)
                                {
                                    table.Cell().Padding(2).Text(cat.CategoryName).FontSize(9);
                                    table.Cell().Padding(2).Text(cat.Amount.ToString("C2")).FontSize(9).AlignRight();
                                    table.Cell().Padding(2).Text($"{cat.Percentage:F1}%").FontSize(9).AlignRight();
                                }
                            });
                        }

                        column.Item().PaddingTop(15).Text("Transações do Período").FontSize(14).Bold();

                        if (data.Transactions.Count > 0)
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(60);
                                    c.RelativeColumn(3);
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Padding(3).Text("Data").Bold().FontSize(9);
                                    header.Cell().Padding(3).Text("Descrição").Bold().FontSize(9);
                                    header.Cell().Padding(3).Text("Categoria").Bold().FontSize(9);
                                    header.Cell().Padding(3).Text("Valor").Bold().FontSize(9).AlignRight();
                                });

                                foreach (var tx in data.Transactions)
                                {
                                    table.Cell().Padding(2).Text(tx.Date.ToString("dd/MM")).FontSize(8);
                                    table.Cell().Padding(2).Text(tx.Description).FontSize(8);
                                    table.Cell().Padding(2).Text(tx.Category).FontSize(8);
                                    table.Cell().Padding(2).Text(tx.Amount.ToString("C2")).FontSize(8).AlignRight();
                                }
                            });
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Monetra - Simplificando suas finanças")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Medium);
                });
            }).GeneratePdf();
        }, cancellationToken);
    }
}
