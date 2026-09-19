namespace Backend.Interface
{
    public interface IPieceSocialExportService
    {
        Task<byte[]?> ExportProductPostPngAsync(int pieceId, string format);
        Task<byte[]?> ExportProductPostPdfAsync(int pieceId, string format);
    }
}
