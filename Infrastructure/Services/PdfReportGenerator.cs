using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Enums;

namespace MisFinanzas.Infrastructure.Services
{
    public class PdfReportGenerator
    {
        /// Genera un PDF del reporte y lo devuelve como array de bytes
        public byte[] GeneratePdf(ReportDataDto reportData)
        {
            // Configurar licencia de QuestPDF (Community)
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // Encabezado
                    page.Header().Element(ComposeHeader);

                    // Contenido principal
                    page.Content().Column(column =>
                    {
                        column.Spacing(15);

                        // Información del período
                        column.Item().Element(c => ComposePeriodInfo(c, reportData));

                        // Resumen general
                        column.Item().Element(c => ComposeSummary(c, reportData));

                        // Comparativa (si existe)
                        if (reportData.Comparison != null)
                        {
                            column.Item().Element(c => ComposeComparison(c, reportData.Comparison));
                        }

                        // Gastos por categoría
                        if (reportData.ExpensesByCategory.Any())
                        {
                            column.Item().Element(c => ComposeExpensesByCategory(c, reportData));
                        }

                        // Ingresos por categoría
                        if (reportData.IncomesByCategory.Any())
                        {
                            column.Item().Element(c => ComposeIncomesByCategory(c, reportData));
                        }

                        // Detalle de transacciones
                        if (reportData.Transactions.Any())
                        {
                            column.Item().Element(c => ComposeTransactionsDetail(c, reportData));
                        }
                    });

                    // Pie de página
                    page.Footer()
                        .AlignCenter()
                        .DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium))
                        .Text(text =>
                        {
                            text.Span("Página ");
                            text.CurrentPageNumber();
                            text.Span(" de ");
                            text.TotalPages();
                            text.Span($" | Generado: {reportData.GeneratedAt:dd/MM/yyyy HH:mm}");
                        });
                });
            });

            return document.GeneratePdf();
        }

        #region Header

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>

            {

                column.Item().Row(row =>

                {

                    row.RelativeItem().Column(col =>

                    {

                        col.Item().Text("MIS FINANZAS")

                            .FontSize(20)

                            .Bold()

                            .FontColor(Colors.Blue.Darken2);



                        col.Item().Text("Reporte Financiero")

                            .FontSize(12)

                            .FontColor(Colors.Grey.Darken1);

                    });

                });
                column.Item().PaddingBottom(10).BorderBottom(1).BorderColor(Colors.Blue.Darken2);

            });
        }

        #endregion

        #region Period Info

        private void ComposePeriodInfo(IContainer container, ReportDataDto reportData)
        {
            container.Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
            {
                column.Spacing(3);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Período: {reportData.PeriodDescription}").Bold();
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Desde: {reportData.StartDate:dd/MM/yyyy}")
                        .FontSize(9);
                    row.RelativeItem().Text($"Hasta: {reportData.EndDate:dd/MM/yyyy}")
                        .FontSize(9);
                });

                column.Item().Text($"Usuario: {reportData.UserName}").FontSize(9);
            });
        }

        #endregion

        #region Summary

        private void ComposeSummary(IContainer container, ReportDataDto reportData)
        {
            container.Column(column =>
            {
                column.Item().Text("RESUMEN GENERAL")
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                column.Item().PaddingTop(5).BorderBottom(1).BorderColor(Colors.Grey.Medium);

                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                    });

                    // Ingresos
                    table.Cell().Text("Total Ingresos:").Bold();
                    table.Cell().AlignRight().Text($"{reportData.Summary.TotalIncome:C}")
                        .FontColor(Colors.Green.Darken2).Bold();

                    // Gastos
                    table.Cell().Text("Total Gastos:").Bold();
                    table.Cell().AlignRight().Text($"{reportData.Summary.TotalExpense:C}")
                        .FontColor(Colors.Red.Darken2).Bold();

                    // Balance
                    table.Cell().Text("Balance:").Bold().FontSize(11);
                    table.Cell().AlignRight().Text($"{reportData.Summary.Balance:C}")
                        .FontColor(reportData.Summary.Balance >= 0 ? Colors.Blue.Darken2 : Colors.Red.Darken2)
                        .Bold()
                        .FontSize(11);

                    // Promedio diario
                    table.Cell().PaddingTop(5).Text("Promedio diario de gastos:");
                    table.Cell().PaddingTop(5).AlignRight().Text($"{reportData.Summary.AverageDailyExpense:C}");

                    // Total transacciones
                    table.Cell().Text("Total de transacciones:");
                    table.Cell().AlignRight().Text(reportData.Summary.TotalTransactions.ToString());
                });
            });
        }

        #endregion

        #region Comparison

        private void ComposeComparison(IContainer container, ReportComparisonDto comparison)
        {
            container.Background(Colors.Blue.Lighten4).Padding(10).Column(column =>
            {
                column.Item().Text("Comparación con período anterior")
                    .FontSize(11)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                column.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text($"Ingresos: {comparison.IncomeChangeDisplay}").FontSize(9);
                    row.RelativeItem().Text($"Gastos: {comparison.ExpenseChangeDisplay}").FontSize(9);
                    row.RelativeItem().Text($"Balance: {comparison.BalanceChangeDisplay}").FontSize(9);
                });
            });
        }

        #endregion

        #region Expenses By Category

        private void ComposeExpensesByCategory(IContainer container, ReportDataDto reportData)
        {
            container.Column(column =>
            {
                column.Item().Text("GASTOS POR CATEGORÍA")
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                column.Item().PaddingTop(5).BorderBottom(1).BorderColor(Colors.Grey.Medium);

                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    // Encabezados
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Categoría").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Monto").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text("%").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text("Cant.").Bold();
                    });

                    // Filas
                    foreach (var category in reportData.ExpensesByCategory)
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5)
                            .Text($"{category.CategoryIcon} {category.CategoryName}");

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5)
                            .AlignRight().Text($"{category.TotalAmount:C}").FontColor(Colors.Red.Darken1);

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5)
                            .AlignCenter().Text($"{category.Percentage:F1}%");

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5)
                            .AlignCenter().Text(category.TransactionCount.ToString());
                    }
                });
            });
        }

        #endregion

        #region Incomes By Category

        private void ComposeIncomesByCategory(IContainer container, ReportDataDto reportData)
        {
            container.Column(column =>
            {
                column.Item().Text("INGRESOS POR CATEGORÍA")
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                column.Item().PaddingTop(5).BorderBottom(1).BorderColor(Colors.Grey.Medium);

                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    // Encabezados
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Categoría").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Monto").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text("%").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text("Cant.").Bold();
                    });

                    // Filas
                    foreach (var category in reportData.IncomesByCategory)
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5)
                            .Text($"{category.CategoryIcon} {category.CategoryName}");

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5)
                            .AlignRight().Text($"{category.TotalAmount:C}").FontColor(Colors.Green.Darken1);

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5)
                            .AlignCenter().Text($"{category.Percentage:F1}%");

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5)
                            .AlignCenter().Text(category.TransactionCount.ToString());
                    }
                });
            });
        }

        #endregion

        #region Transactions Detail

        private void ComposeTransactionsDetail(IContainer container, ReportDataDto reportData)
        {
            container.Column(column =>
            {
                column.Item().Text("DETALLE DE TRANSACCIONES")
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                column.Item().PaddingTop(5).BorderBottom(1).BorderColor(Colors.Grey.Medium);

                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1.5f);
                    });

                    // Encabezados
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Fecha").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Tipo").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Categoría").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Descripción").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Monto").Bold().FontSize(9);
                    });

                    // Filas (limitar a las primeras 50 transacciones para evitar PDFs muy largos)
                    foreach (var transaction in reportData.Transactions.Take(100))
                    {
                        var typeText = transaction.Type == TransactionType.Income ? "Ingreso" : "Gasto";
                        var amountColor = transaction.Type == TransactionType.Income
                            ? Colors.Green.Darken1
                            : Colors.Red.Darken1;

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3)
                            .Text(transaction.Date.ToString("dd/MM/yyyy")).FontSize(8);

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3)
                            .Text(typeText).FontSize(8);

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3)
                            .Text($"{transaction.CategoryIcon} {transaction.CategoryTitle}").FontSize(8);

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3)
                            .Text(transaction.Description ?? "-").FontSize(8);

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3)
                            .AlignRight().Text($"{transaction.Amount:C}").FontColor(amountColor).FontSize(8);
                    }
                });

                // Nota si hay más transacciones
                if (reportData.Transactions.Count > 100)
                {
                    column.Item().PaddingTop(5).Text($"* Mostrando las primeras 100 transacciones de {reportData.Transactions.Count} totales")
                        .FontSize(8)
                        .Italic()
                        .FontColor(Colors.Grey.Darken1);
                }
            });
        }

        #endregion
    }
}