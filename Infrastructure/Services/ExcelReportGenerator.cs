using ClosedXML.Excel;
using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Enums;

namespace MisFinanzas.Infrastructure.Services
{
    public class ExcelReportGenerator
    {
        /// Genera un archivo Excel del reporte y lo devuelve como array de bytes
        public byte[] GenerateExcel(ReportDataDto reportData, string logoPath)
        {
            using (var workbook = new XLWorkbook())
            {
                // Crear hojas
                CreateSummarySheet(workbook, reportData, logoPath);
                CreateExpensesByCategorySheet(workbook, reportData);
                CreateIncomesByCategorySheet(workbook, reportData);
                CreateTransactionsDetailSheet(workbook, reportData);

                // Convertir a bytes
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return stream.ToArray();
                }
            }
        }

        #region Summary Sheet

        private void CreateSummarySheet(XLWorkbook workbook, ReportDataDto reportData, string logoPath)
        {
            var worksheet = workbook.Worksheets.Add("Resumen");

            int row = 1;

            // 🔥 LOGO en A1-B1
            try
            {
                if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
                {
                    var picture = worksheet.AddPicture(logoPath);
                    picture.MoveTo(worksheet.Cell(row, 1)); // A1
                    picture.Scale(0.06); // Tamaño pequeño
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading logo in Excel: {ex.Message}");
            }

            // 🔥 TÍTULO en C1-K1 (más amplio)
            var titleRange = worksheet.Range(row, 3, row, 11);
            titleRange.Merge();
            worksheet.Cell(row, 3).Value = "REPORTE DE MIS FINANZAS";
            worksheet.Cell(row, 3).Style.Font.Bold = true;
            worksheet.Cell(row, 3).Style.Font.FontSize = 18;
            worksheet.Cell(row, 3).Style.Font.FontColor = XLColor.White;
            worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            titleRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            titleRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

            // Altura de la fila del encabezado
            worksheet.Row(row).Height = 45;

            row += 2; // Saltar a fila 3

            // 🔥 INFORMACIÓN DEL REPORTE (Columna B-C, debajo del logo)
            int infoStartRow = row;
            worksheet.Cell(row, 2).Value = "Usuario:";
            worksheet.Cell(row, 2).Style.Font.Bold = true;
            worksheet.Cell(row, 3).Value = reportData.UserName;
            row++;

            worksheet.Cell(row, 2).Value = "Período:";
            worksheet.Cell(row, 2).Style.Font.Bold = true;
            worksheet.Cell(row, 3).Value = reportData.PeriodDescription;
            row++;

            worksheet.Cell(row, 2).Value = "Desde:";
            worksheet.Cell(row, 2).Style.Font.Bold = true;
            worksheet.Cell(row, 3).Value = reportData.StartDate.ToString("dd/MM/yyyy");
            row++;

            worksheet.Cell(row, 2).Value = "Hasta:";
            worksheet.Cell(row, 2).Style.Font.Bold = true;
            worksheet.Cell(row, 3).Value = reportData.EndDate.ToString("dd/MM/yyyy");
            row++;

            worksheet.Cell(row, 2).Value = "Generado:";
            worksheet.Cell(row, 2).Style.Font.Bold = true;
            worksheet.Cell(row, 3).Value = reportData.GeneratedAt.ToString("dd/MM/yyyy HH:mm");

            // Bordes para la información del reporte
            var infoRange = worksheet.Range(infoStartRow, 2, row, 3);
            infoRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            infoRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            infoRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F9F9F9");

            row += 2; // Espacio antes del resumen

            // 🔥 RESUMEN GENERAL (Columna B-D, izquierda)
            int summaryRow = row;
            worksheet.Cell(summaryRow, 2).Value = "RESUMEN GENERAL";
            worksheet.Cell(summaryRow, 2).Style.Font.Bold = true;
            worksheet.Cell(summaryRow, 2).Style.Font.FontSize = 12;
            worksheet.Cell(summaryRow, 2).Style.Font.FontColor = XLColor.White;
            worksheet.Range(summaryRow, 2, summaryRow, 4).Merge();
            worksheet.Range(summaryRow, 2, summaryRow, 4).Style.Fill.BackgroundColor = XLColor.DarkBlue;
            worksheet.Range(summaryRow, 2, summaryRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            worksheet.Range(summaryRow, 2, summaryRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            summaryRow++;

            int summaryStartRow = summaryRow;

            // Total Ingresos
            worksheet.Cell(summaryRow, 2).Value = "Total Ingresos:";
            worksheet.Cell(summaryRow, 2).Style.Font.Bold = true;
            worksheet.Cell(summaryRow, 3).Value = reportData.Summary.TotalIncome;
            worksheet.Cell(summaryRow, 3).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(summaryRow, 3).Style.Font.FontColor = XLColor.DarkGreen;
            worksheet.Cell(summaryRow, 3).Style.Font.Bold = true;
            worksheet.Cell(summaryRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Range(summaryRow, 3, summaryRow, 4).Merge();
            summaryRow++;

            // Total Gastos
            worksheet.Cell(summaryRow, 2).Value = "Total Gastos:";
            worksheet.Cell(summaryRow, 2).Style.Font.Bold = true;
            worksheet.Cell(summaryRow, 3).Value = reportData.Summary.TotalExpense;
            worksheet.Cell(summaryRow, 3).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(summaryRow, 3).Style.Font.FontColor = XLColor.DarkRed;
            worksheet.Cell(summaryRow, 3).Style.Font.Bold = true;
            worksheet.Cell(summaryRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Range(summaryRow, 3, summaryRow, 4).Merge();
            summaryRow++;

            // Balance
            worksheet.Cell(summaryRow, 2).Value = "Balance:";
            worksheet.Cell(summaryRow, 2).Style.Font.Bold = true;
            worksheet.Cell(summaryRow, 3).Value = reportData.Summary.Balance;
            worksheet.Cell(summaryRow, 3).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(summaryRow, 3).Style.Font.FontColor = reportData.Summary.Balance >= 0
                ? XLColor.DarkBlue
                : XLColor.DarkRed;
            worksheet.Cell(summaryRow, 3).Style.Font.Bold = true;
            worksheet.Cell(summaryRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Range(summaryRow, 3, summaryRow, 4).Merge();
            summaryRow++;

            // Promedio diario
            worksheet.Cell(summaryRow, 2).Value = "Promedio diario:";
            worksheet.Cell(summaryRow, 2).Value = "Promedio diario de gastos:";
            worksheet.Cell(summaryRow, 3).Value = reportData.Summary.AverageDailyExpense;
            worksheet.Cell(summaryRow, 3).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(summaryRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Range(summaryRow, 3, summaryRow, 4).Merge();
            summaryRow++;

            // Total transacciones
            worksheet.Cell(summaryRow, 2).Value = "Total de transacciones:";
            worksheet.Cell(summaryRow, 3).Value = reportData.Summary.TotalTransactions;
            worksheet.Cell(summaryRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Range(summaryRow, 3, summaryRow, 4).Merge();

            // Bordes para el resumen general
            var summaryRange = worksheet.Range(summaryStartRow, 2, summaryRow, 4);
            summaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            summaryRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // 🔥 COMPARACIÓN (Columna F-H, derecha, al lado del resumen)
            if (reportData.Comparison != null)
            {
                int compRow = row; // Misma fila que el resumen

                worksheet.Cell(compRow, 6).Value = "COMPARACIÓN CON PERÍODO ANTERIOR";
                worksheet.Cell(compRow, 6).Style.Font.Bold = true;
                worksheet.Cell(compRow, 6).Style.Font.FontSize = 12;
                worksheet.Cell(compRow, 6).Style.Font.FontColor = XLColor.White;
                worksheet.Range(compRow, 6, compRow, 8).Merge();
                worksheet.Range(compRow, 6, compRow, 8).Style.Fill.BackgroundColor = XLColor.FromHtml("#4682B4");
                worksheet.Range(compRow, 6, compRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                worksheet.Range(compRow, 6, compRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                compRow++;

                int compStartRow = compRow;

                worksheet.Cell(compRow, 6).Value = "Cambio en Ingresos:";
                worksheet.Cell(compRow, 6).Style.Font.Bold = true;
                worksheet.Cell(compRow, 7).Value = reportData.Comparison.IncomeChangeDisplay;
                worksheet.Range(compRow, 7, compRow, 8).Merge();
                compRow++;

                worksheet.Cell(compRow, 6).Value = "Cambio en Gastos:";
                worksheet.Cell(compRow, 6).Style.Font.Bold = true;
                worksheet.Cell(compRow, 7).Value = reportData.Comparison.ExpenseChangeDisplay;
                worksheet.Range(compRow, 7, compRow, 8).Merge();
                compRow++;

                worksheet.Cell(compRow, 6).Value = "Cambio en Balance:";
                worksheet.Cell(compRow, 6).Style.Font.Bold = true;
                worksheet.Cell(compRow, 7).Value = reportData.Comparison.BalanceChangeDisplay;
                worksheet.Range(compRow, 7, compRow, 8).Merge();

                // Bordes para la comparación
                var compRange = worksheet.Range(compStartRow, 6, compRow, 8);
                compRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                compRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                compRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#E6F2FF");
            }

            // Ajustar anchos de columna
            worksheet.Column(1).Width = 3;  // Espacio izquierdo
            worksheet.Column(2).Width = 25; // Labels
            worksheet.Column(3).Width = 15; // Valores
            worksheet.Column(4).Width = 5;  // Espacio
            worksheet.Column(5).Width = 3;  // Espacio
            worksheet.Column(6).Width = 25; // Comparación labels
            worksheet.Column(7).Width = 15; // Comparación valores
            worksheet.Column(8).Width = 5;  // Espacio
        }

        #endregion

        #region Expenses By Category Sheet

        private void CreateExpensesByCategorySheet(XLWorkbook workbook, ReportDataDto reportData)
        {
            var worksheet = workbook.Worksheets.Add("Gastos por Categoría");

            // Encabezados
            worksheet.Cell(1, 1).Value = "Categoría";
            worksheet.Cell(1, 2).Value = "Monto Total";
            worksheet.Cell(1, 3).Value = "Porcentaje";
            worksheet.Cell(1, 4).Value = "Cantidad";

            // Estilo de encabezados
            var headerRange = worksheet.Range(1, 1, 1, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Datos
            int row = 2;
            int startDataRow = row;
            foreach (var category in reportData.ExpensesByCategory)
            {
                worksheet.Cell(row, 1).Value = category.CategoryName;
                worksheet.Cell(row, 2).Value = category.TotalAmount;
                worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell(row, 3).Value = category.Percentage / 100;
                worksheet.Cell(row, 3).Style.NumberFormat.Format = "0.0%";
                worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 4).Value = category.TransactionCount;
                worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Filas alternas
                if ((row - startDataRow) % 2 == 1)
                {
                    worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#F0F0F0");
                }

                row++;
            }

            // Total
            if (reportData.ExpensesByCategory.Any())
            {
                var totalRange = worksheet.Range(row, 1, row, 4);
                totalRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#FFE6E6");
                totalRange.Style.Border.TopBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(row, 1).Value = "TOTAL";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 2).Value = reportData.ExpensesByCategory.Sum(c => c.TotalAmount);
                worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 2).Style.Font.Bold = true;
                worksheet.Cell(row, 2).Style.Font.FontColor = XLColor.DarkRed;
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            }

            // Bordes a todos los datos
            if (reportData.ExpensesByCategory.Any())
            {
                var dataRange = worksheet.Range(startDataRow, 1, row, 4);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            // Ajustar anchos
            worksheet.Column(1).Width = 30;
            worksheet.Column(2).Width = 15;
            worksheet.Column(3).Width = 12;
            worksheet.Column(4).Width = 10;
        }

        #endregion

        #region Incomes By Category Sheet

        private void CreateIncomesByCategorySheet(XLWorkbook workbook, ReportDataDto reportData)
        {
            var worksheet = workbook.Worksheets.Add("Ingresos por Categoría");

            // Encabezados
            worksheet.Cell(1, 1).Value = "Categoría";
            worksheet.Cell(1, 2).Value = "Monto Total";
            worksheet.Cell(1, 3).Value = "Porcentaje";
            worksheet.Cell(1, 4).Value = "Cantidad";

            // Estilo de encabezados
            var headerRange = worksheet.Range(1, 1, 1, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.DarkGreen;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Datos
            int row = 2;
            int startDataRow = row;
            foreach (var category in reportData.IncomesByCategory)
            {
                worksheet.Cell(row, 1).Value = category.CategoryName;
                worksheet.Cell(row, 2).Value = category.TotalAmount;
                worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell(row, 3).Value = category.Percentage / 100;
                worksheet.Cell(row, 3).Style.NumberFormat.Format = "0.0%";
                worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 4).Value = category.TransactionCount;
                worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Filas alternas
                if ((row - startDataRow) % 2 == 1)
                {
                    worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#F0F0F0");
                }

                row++;
            }

            // Total
            if (reportData.IncomesByCategory.Any())
            {
                var totalRange = worksheet.Range(row, 1, row, 4);
                totalRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#E6FFE6");
                totalRange.Style.Border.TopBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(row, 1).Value = "TOTAL";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 2).Value = reportData.IncomesByCategory.Sum(c => c.TotalAmount);
                worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 2).Style.Font.Bold = true;
                worksheet.Cell(row, 2).Style.Font.FontColor = XLColor.DarkGreen;
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            }

            // Bordes a todos los datos
            if (reportData.IncomesByCategory.Any())
            {
                var dataRange = worksheet.Range(startDataRow, 1, row, 4);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            // Ajustar anchos
            worksheet.Column(1).Width = 30;
            worksheet.Column(2).Width = 15;
            worksheet.Column(3).Width = 12;
            worksheet.Column(4).Width = 10;
        }
        #endregion

        #region Transactions Detail Sheet

        private void CreateTransactionsDetailSheet(XLWorkbook workbook, ReportDataDto reportData)
        {
            var worksheet = workbook.Worksheets.Add("Detalle Transacciones");

            // Encabezados
            worksheet.Cell(1, 1).Value = "Fecha";
            worksheet.Cell(1, 2).Value = "Tipo";
            worksheet.Cell(1, 3).Value = "Categoría";
            worksheet.Cell(1, 4).Value = "Descripción";
            worksheet.Cell(1, 5).Value = "Monto";

            // Estilo de encabezados
            var headerRange = worksheet.Range(1, 1, 1, 5);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.DarkGray;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Datos
            int row = 2;
            int startDataRow = row;
            foreach (var transaction in reportData.Transactions)
            {
                worksheet.Cell(row, 1).Value = transaction.Date.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 2).Value = transaction.Type == TransactionType.Income ? "Ingreso" : "Gasto";
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 3).Value = transaction.CategoryTitle;

                worksheet.Cell(row, 4).Value = transaction.Description ?? "-";

                worksheet.Cell(row, 5).Value = transaction.Amount;
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                // Color según tipo
                if (transaction.Type == TransactionType.Income)
                {
                    worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.DarkGreen;
                    worksheet.Cell(row, 5).Style.Font.Bold = true;
                }
                else
                {
                    worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.DarkRed;
                    worksheet.Cell(row, 5).Style.Font.Bold = true;
                }

                // Filas alternas
                if ((row - startDataRow) % 2 == 1)
                {
                    worksheet.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#F0F0F0");
                }

                row++;
            }

            // Bordes a todos los datos
            if (reportData.Transactions.Any())
            {
                var dataRange = worksheet.Range(startDataRow, 1, row - 1, 5);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            // Ajustar anchos
            worksheet.Column(1).Width = 12;
            worksheet.Column(2).Width = 10;
            worksheet.Column(3).Width = 25;
            worksheet.Column(4).Width = 40;
            worksheet.Column(5).Width = 15;

            // Aplicar filtros
            var tableRange = worksheet.Range(1, 1, row - 1, 5);
            tableRange.SetAutoFilter();
        }

        #endregion
    }
}