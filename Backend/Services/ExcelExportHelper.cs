using OfficeOpenXml;
using System.Drawing;

namespace Backend.Services
{
    /// <summary>
    /// Classe helper pour les opérations courantes d'export Excel
    /// </summary>
    public static class ExcelExportHelper
    {
        private const string HEADER_BACKGROUND_COLOR = "LightGray";

        /// <summary>
        /// Crée et configure les en-têtes avec styling
        /// </summary>
        public static void CreateHeaders(ExcelWorksheet worksheet, params string[] headers)
        {
            for (int col = 1; col <= headers.Length; col++)
            {
                worksheet.Cells[1, col].Value = headers[col - 1];
            }

            ApplyHeaderStyle(worksheet, headers.Length);
        }

        /// <summary>
        /// Applique le style standard aux en-têtes
        /// </summary>
        private static void ApplyHeaderStyle(ExcelWorksheet worksheet, int columnCount)
        {
            using (var range = worksheet.Cells[1, 1, 1, columnCount])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }
        }

        /// <summary>
        /// Ajoute une ligne avec des valeurs et retourne le numéro de ligne suivant
        /// </summary>
        public static int AddRow(ExcelWorksheet worksheet, int row, params object?[] values)
        {
            for (int col = 1; col <= values.Length; col++)
            {
                worksheet.Cells[row, col].Value = values[col - 1];
            }
            return row + 1;
        }

        /// <summary>
        /// Ajoute une ligne avec des valeurs et couleur conditionnelle
        /// </summary>
        public static int AddRowWithConditionalColor(ExcelWorksheet worksheet, int row, 
            Func<bool> colorCondition, int colorColumn, Color color, params object?[] values)
        {
            AddRow(worksheet, row, values);

            if (colorCondition())
            {
                worksheet.Cells[row, colorColumn].Style.Font.Color.SetColor(color);
            }

            return row + 1;
        }

        /// <summary>
        /// Finalise le worksheet : ajuste les colonnes et retourne le byte array
        /// </summary>
        public static byte[] FinalizeExport(ExcelPackage package, ExcelWorksheet worksheet)
        {
            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }
    }
}
