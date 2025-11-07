using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Enums;

namespace MisFinanzas.Infrastructure.Services
{
    public class PdfReportGenerator
    {
        public byte[] GeneratePdf(ReportDataDto reportData)
        {
            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Encabezado
            AddHeader(document);

            // Información del período
            AddPeriodInfo(document, reportData);

            // Resumen general
            AddSummary(document, reportData);

            // Comparativa (si existe)
            if (reportData.Comparison != null)
            {
                AddComparison(document, reportData.Comparison);
            }

            // Gastos por categoría
            if (reportData.ExpensesByCategory.Any())
            {
                AddExpensesByCategory(document, reportData);
            }

            // Ingresos por categoría
            if (reportData.IncomesByCategory.Any())
            {
                AddIncomesByCategory(document, reportData);
            }

            // Detalle de transacciones
            if (reportData.Transactions.Any())
            {
                AddTransactionsDetail(document, reportData);
            }

            // Pie de página
            AddFooter(document, reportData);

            document.Close();
            return stream.ToArray();
        }

        private void AddHeader(Document document)
        {
            var title = new Paragraph("MIS FINANZAS")
                .SetFontSize(20)
                .SetBold()
                .SetFontColor(ColorConstants.BLUE);
            document.Add(title);

            var subtitle = new Paragraph("Reporte Financiero")
                .SetFontSize(12)
                .SetFontColor(ColorConstants.GRAY);
            document.Add(subtitle);

            document.Add(new Paragraph("\n"));
        }

        private void AddPeriodInfo(Document document, ReportDataDto reportData)
        {
            var periodPara = new Paragraph()
                .Add(new Text($"Período: {reportData.PeriodDescription}\n").SetBold())
                .Add(new Text($"Desde: {reportData.StartDate:dd/MM/yyyy}  "))
                .Add(new Text($"Hasta: {reportData.EndDate:dd/MM/yyyy}\n"))
                .Add(new Text($"Usuario: {reportData.UserName}"))
                .SetFontSize(10)
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetPadding(10);

            document.Add(periodPara);
            document.Add(new Paragraph("\n"));
        }

        private void AddSummary(Document document, ReportDataDto reportData)
        {
            var title = new Paragraph("RESUMEN GENERAL")
                .SetFontSize(14)
                .SetBold()
                .SetFontColor(ColorConstants.BLUE);
            document.Add(title);

            var table = new Table(2).UseAllAvailableWidth();

            table.AddCell(new Cell().Add(new Paragraph("Total Ingresos:").SetBold()));
            table.AddCell(new Cell().Add(new Paragraph($"{reportData.Summary.TotalIncome:C}")
                .SetFontColor(ColorConstants.GREEN).SetBold()).SetTextAlignment(TextAlignment.RIGHT));

            table.AddCell(new Cell().Add(new Paragraph("Total Gastos:").SetBold()));
            table.AddCell(new Cell().Add(new Paragraph($"{reportData.Summary.TotalExpense:C}")
                .SetFontColor(ColorConstants.RED).SetBold()).SetTextAlignment(TextAlignment.RIGHT));

            table.AddCell(new Cell().Add(new Paragraph("Balance:").SetBold().SetFontSize(11)));
            table.AddCell(new Cell().Add(new Paragraph($"{reportData.Summary.Balance:C}")
                .SetFontColor(reportData.Summary.Balance >= 0 ? ColorConstants.BLUE : ColorConstants.RED)
                .SetBold().SetFontSize(11)).SetTextAlignment(TextAlignment.RIGHT));

            table.AddCell(new Cell().Add(new Paragraph("Promedio diario de gastos:")));
            table.AddCell(new Cell().Add(new Paragraph($"{reportData.Summary.AverageDailyExpense:C}"))
                .SetTextAlignment(TextAlignment.RIGHT));

            table.AddCell(new Cell().Add(new Paragraph("Total de transacciones:")));
            table.AddCell(new Cell().Add(new Paragraph(reportData.Summary.TotalTransactions.ToString()))
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(table);
            document.Add(new Paragraph("\n"));
        }

        private void AddComparison(Document document, ReportComparisonDto comparison)
        {
            var compPara = new Paragraph()
                .Add(new Text("Comparación con período anterior\n").SetBold())
                .Add(new Text($"Ingresos: {comparison.IncomeChangeDisplay}  "))
                .Add(new Text($"Gastos: {comparison.ExpenseChangeDisplay}  "))
                .Add(new Text($"Balance: {comparison.BalanceChangeDisplay}"))
                .SetFontSize(9)
                .SetBackgroundColor(new DeviceRgb(173, 216, 230))
                .SetPadding(10);

            document.Add(compPara);
            document.Add(new Paragraph("\n"));
        }

        private void AddExpensesByCategory(Document document, ReportDataDto reportData)
        {
            var title = new Paragraph("GASTOS POR CATEGORÍA")
                .SetFontSize(14)
                .SetBold()
                .SetFontColor(ColorConstants.BLUE);
            document.Add(title);

            var table = new Table(new float[] { 3, 2, 1, 1 }).UseAllAvailableWidth();

            // Encabezados
            table.AddHeaderCell(new Cell().Add(new Paragraph("Categoría").SetBold())
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Monto").SetBold())
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetTextAlignment(TextAlignment.RIGHT));
            table.AddHeaderCell(new Cell().Add(new Paragraph("%").SetBold())
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetTextAlignment(TextAlignment.CENTER));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Cant.").SetBold())
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetTextAlignment(TextAlignment.CENTER));

            // Filas
            foreach (var category in reportData.ExpensesByCategory)
            {
                table.AddCell(new Cell().Add(new Paragraph(category.CategoryName)));
                table.AddCell(new Cell().Add(new Paragraph($"{category.TotalAmount:C}")
                    .SetFontColor(ColorConstants.RED)).SetTextAlignment(TextAlignment.RIGHT));
                table.AddCell(new Cell().Add(new Paragraph($"{category.Percentage:F1}%"))
                    .SetTextAlignment(TextAlignment.CENTER));
                table.AddCell(new Cell().Add(new Paragraph(category.TransactionCount.ToString()))
                    .SetTextAlignment(TextAlignment.CENTER));
            }

            document.Add(table);
            document.Add(new Paragraph("\n"));
        }

        private void AddIncomesByCategory(Document document, ReportDataDto reportData)
        {
            var title = new Paragraph("INGRESOS POR CATEGORÍA")
                .SetFontSize(14)
                .SetBold()
                .SetFontColor(ColorConstants.BLUE);
            document.Add(title);

            var table = new Table(new float[] { 3, 2, 1, 1 }).UseAllAvailableWidth();

            // Encabezados
            table.AddHeaderCell(new Cell().Add(new Paragraph("Categoría").SetBold())
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Monto").SetBold())
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetTextAlignment(TextAlignment.RIGHT));
            table.AddHeaderCell(new Cell().Add(new Paragraph("%").SetBold())
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetTextAlignment(TextAlignment.CENTER));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Cant.").SetBold())
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetTextAlignment(TextAlignment.CENTER));

            // Filas
            foreach (var category in reportData.IncomesByCategory)
            {
                table.AddCell(new Cell().Add(new Paragraph(category.CategoryName)));
                table.AddCell(new Cell().Add(new Paragraph($"{category.TotalAmount:C}")
                    .SetFontColor(ColorConstants.GREEN)).SetTextAlignment(TextAlignment.RIGHT));
                table.AddCell(new Cell().Add(new Paragraph($"{category.Percentage:F1}%"))
                    .SetTextAlignment(TextAlignment.CENTER));
                table.AddCell(new Cell().Add(new Paragraph(category.TransactionCount.ToString()))
                    .SetTextAlignment(TextAlignment.CENTER));
            }

            document.Add(table);
            document.Add(new Paragraph("\n"));
        }

        private void AddTransactionsDetail(Document document, ReportDataDto reportData)
        {
            var title = new Paragraph("DETALLE DE TRANSACCIONES")
                .SetFontSize(14)
                .SetBold()
                .SetFontColor(ColorConstants.BLUE);
            document.Add(title);

            var table = new Table(new float[] { 1, 1, 2, 3, 1.5f }).UseAllAvailableWidth();

            // Encabezados
            table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha").SetBold().SetFontSize(9))
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Tipo").SetBold().SetFontSize(9))
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Categoría").SetBold().SetFontSize(9))
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Descripción").SetBold().SetFontSize(9))
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Monto").SetBold().SetFontSize(9))
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetTextAlignment(TextAlignment.RIGHT));

            // Filas (limitar a las primeras 100 transacciones)
            foreach (var transaction in reportData.Transactions.Take(100))
            {
                var typeText = transaction.Type == TransactionType.Income ? "Ingreso" : "Gasto";
                var amountColor = transaction.Type == TransactionType.Income
                    ? ColorConstants.GREEN
                    : ColorConstants.RED;

                table.AddCell(new Cell().Add(new Paragraph(transaction.Date.ToString("dd/MM/yyyy")).SetFontSize(8)));
                table.AddCell(new Cell().Add(new Paragraph(typeText).SetFontSize(8)));
                table.AddCell(new Cell().Add(new Paragraph(transaction.CategoryTitle).SetFontSize(8)));
                table.AddCell(new Cell().Add(new Paragraph(transaction.Description ?? "-").SetFontSize(8)));
                table.AddCell(new Cell().Add(new Paragraph($"{transaction.Amount:C}").SetFontSize(8)
                    .SetFontColor(amountColor)).SetTextAlignment(TextAlignment.RIGHT));
            }

            document.Add(table);

            // Nota si hay más transacciones
            if (reportData.Transactions.Count > 100)
            {
                var note = new Paragraph($"* Mostrando las primeras 100 transacciones de {reportData.Transactions.Count} totales")
                    .SetFontSize(8)
                    .SetItalic()
                    .SetFontColor(ColorConstants.GRAY);
                document.Add(note);
            }
        }

        private void AddFooter(Document document, ReportDataDto reportData)
        {
            var footer = new Paragraph($"Generado: {reportData.GeneratedAt:dd/MM/yyyy HH:mm}")
                .SetFontSize(8)
                .SetFontColor(ColorConstants.GRAY)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(footer);
        }
    }
}