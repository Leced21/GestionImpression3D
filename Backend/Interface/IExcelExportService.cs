using Backend.DTOs;
using Backend.Models;

namespace Backend.Interface
{
    public interface IExcelExportService
    {
        Task<byte[]> ExportPiecesToExcelAsync();
        Task<byte[]> ExportProjetsToExcelAsync();
        Task<byte[]> ExportPrintJobsToExcelAsync();
        Task<byte[]> ExportCommandesToExcelAsync();
        Task<byte[]> ExportMaterialStockToExcelAsync();
    }
}
