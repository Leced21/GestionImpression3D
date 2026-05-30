using Backend.Data;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class PrinterRepository : IPrinterRepository
    {
        private readonly AppDbContext _context;
        public PrinterRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Printer> CreateAsync(Printer printer)
        {
            _context.Printers.Add(printer);
            await _context.SaveChangesAsync();
            return printer;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var printer = await GetByIdAsync(id);
            if (printer == null) return false;

            _context.Printers.Remove(printer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Printer>> GetAllAsync()
        {
            return await _context.Printers
            .OrderBy(p => p.Nom)
            .ToListAsync();
        }

        public async Task<int> GetAvailableCountAsync()
        {
            return await _context.Printers
            .CountAsync(p => p.Status == PrinterStatus.Available && p.IsActive);
        }

        public async Task<Printer?> GetByIdAsync(int id)
        {
            return await _context.Printers.FindAsync(id);
        }

        public async Task<IEnumerable<Printer>> GetByStatusAsync(PrinterStatus status)
        {
            return await _context.Printers
            .Where(p => p.Status == status)
            .OrderBy(p => p.Nom)
            .ToListAsync();
        }

        public async Task<Printer> UpdateAsync(Printer printer)
        {
            _context.Entry(printer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return printer;
        }
    }
}
