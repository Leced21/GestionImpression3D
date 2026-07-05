using Backend.Data;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class PrintJobRepository : IPrintJobRepository
    {
        private readonly AppDbContext _context;
        public PrintJobRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<PrintJob> CreateAsync(PrintJob job)
        {
            _context.PrintJobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var job = await GetByIdAsync(id);
            if (job == null) return false;

            _context.PrintJobs.Remove(job);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PrintJob>> GetAllAsync()
        {
            return await _context.PrintJobs
                .Include(j => j.Piece)
                .Include(j => j.Printer)
                .Include(j => j.Operator)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<PrintJob?> GetByIdAsync(int id)
        {
            return await _context.PrintJobs
                .Include(j => j.Piece)
                .Include(j => j.Printer)
                .Include(j => j.Operator)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<IEnumerable<PrintJob>> GetByPieceAsync(int pieceId)
        {
            return await _context.PrintJobs
                .Include(j => j.Piece)
                .Where(j => j.PieceId == pieceId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PrintJob>> GetByPrinterAsync(int printerId)
        {
            return await _context.PrintJobs
                .Include(j => j.Piece)
                .Where(j => j.PrinterId == printerId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PrintJob>> GetByStatusAsync(PrintJobStatus status)
        {
            return await _context.PrintJobs
                .Include(j => j.Piece)
                .Include(j => j.Printer)
                .Where(j => j.Status == status)
                .OrderBy(j => j.Priority)
                .ThenBy(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PrintJob>> GetPendingJobsAsync()
        {
            return await _context.PrintJobs
                .Include(j => j.Piece)
                .Where(j => j.Status == PrintJobStatus.Pending || j.Status == PrintJobStatus.Queued)
                .OrderBy(j => j.Priority == PrintJobPriority.Urgent ? 0 :
                  j.Priority == PrintJobPriority.High ? 1 :
                  j.Priority == PrintJobPriority.Normal ? 2 : 3)
                .ThenBy(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetQueueCountAsync()
        {
            return await _context.PrintJobs
                .CountAsync(j => j.Status == PrintJobStatus.Queued || j.Status == PrintJobStatus.Pending);
        }

        public async Task<PrintJob> UpdateAsync(PrintJob job)
        {
            _context.Entry(job).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return job;
        }
    }
}
