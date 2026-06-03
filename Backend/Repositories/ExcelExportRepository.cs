using Backend.Data;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class ExcelExportRepository : IExcelExportRepository
    {
        private readonly AppDbContext _context;
        public ExcelExportRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Commande>> GetAllCommandesForExportAsync()
        {
            return await _context.Commandes
                .Include(c => c.Lignes)
                .ThenInclude(l => l.Piece)
                .OrderByDescending(c => c.DateCommande)
                .ToListAsync();
        }

        public async Task<IEnumerable<MaterialStock>> GetAllMaterialsForExportAsync()
        {
            return await _context.MaterialStocks
                .Where(m => m.IsActive)
                .OrderBy(m => m.Type)
                .ThenBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Piece>> GetAllPiecesForExportAsync()
        {
            return await _context.Pieces
            .OrderBy(p => p.Id)
            .ToListAsync();
        }

        public async Task<IEnumerable<PrintJob>> GetAllPrintJobsForExportAsync()
        {
            return await _context.PrintJobs
                .Include(j => j.Piece)
                .Include(j => j.Printer)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Projet>> GetAllProjetsForExportAsync()
        {
            return await _context.Projets
                .Include(p => p.ProjetPieces)
                .ThenInclude(pp => pp.Piece)
                .OrderBy(p => p.Id)
                .ToListAsync();
        }
    }
}
