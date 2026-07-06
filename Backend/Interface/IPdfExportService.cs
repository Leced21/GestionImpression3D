using Backend.Models;

namespace Backend.Interface
{
    public interface IPdfExportService
    {
        Task<byte[]> ExportProjetToPdfAsync(Projet projet);
        Task<byte[]> ExportPieceToPdfAsync(Piece piece);
        Task<byte[]> ExportDevisToPdfAsync(Projet projet);
        Task<byte[]> ExportDevisPdfAsync(Devis devis);
        Task<byte[]> ExportFacturePdfAsync(Facture facture);
        Task<byte[]> ExportFicheProduitPdfAsync(Piece piece, STLMetadata? stlMetadata);
    }
}
