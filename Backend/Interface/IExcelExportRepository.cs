using Backend.Models;

namespace Backend.Interface
{
    public interface IExcelExportRepository
    {
        Task<IEnumerable<Piece>> GetAllPiecesForExportAsync();
        Task<IEnumerable<Projet>> GetAllProjetsForExportAsync();
        Task<IEnumerable<PrintJob>> GetAllPrintJobsForExportAsync();
        Task<IEnumerable<Commande>> GetAllCommandesForExportAsync();
        Task<IEnumerable<MaterialStock>> GetAllMaterialsForExportAsync();
    }
}
