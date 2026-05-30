using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class PrinterService : IPrinterService
    {
        private readonly IPrinterRepository _printerRepository;
        private readonly IAuditLogger _auditLogger;
        public PrinterService(IPrinterRepository printerRepository, IAuditLogger auditLogger)
        {
            _printerRepository = printerRepository;
            _auditLogger = auditLogger;
        }
        public async Task<PrinterDto> CreateAsync(CreatePrinterRequest request)
        {
            var type = Enum.Parse<PrinterType>(request.Type);

            var printer = Printer.Create(
                request.Nom,
                request.Reference,
                request.Model,
                request.Brand,
                type,
                request.IpAddress,
                request.MaxPrintSizeX,
                request.MaxPrintSizeY,
                request.MaxPrintSizeZ
            );

            var created = await _printerRepository.CreateAsync(printer);

            await _auditLogger.LogCreationAsync(EntityType.PrintJob, created.Id, created.Nom);

            return MapToDto(created);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var printer = await _printerRepository.GetByIdAsync(id);
            if (printer == null) return false;

            var result = await _printerRepository.DeleteAsync(id);

            if (result)
            {
                await _auditLogger.LogDeletionAsync(EntityType.PrintJob, id, printer.Nom);
            }

            return result;
        }

        public async Task<IEnumerable<PrinterDto>> GetAllAsync()
        {
            var printers = await _printerRepository.GetAllAsync();
            return printers.Select(MapToDto);
        }

        public async Task<PrinterDto?> GetByIdAsync(int id)
        {
            var printer = await _printerRepository.GetByIdAsync(id);
            return printer != null ? MapToDto(printer) : null;
        }

        public async Task<PrinterStatisticsDto> GetStatisticsAsync()
        {
            var printers = await _printerRepository.GetAllAsync();

            return new PrinterStatisticsDto
            {
                TotalPrinters = printers.Count(),
                AvailablePrinters = printers.Count(p => p.Status == PrinterStatus.Available),
                PrintingPrinters = printers.Count(p => p.Status == PrinterStatus.Printing),
                MaintenancePrinters = printers.Count(p => p.Status == PrinterStatus.Maintenance),
                ErrorPrinters = printers.Count(p => p.Status == PrinterStatus.Error),
                TotalPrintJobs = printers.Sum(p => p.TotalPrintJobs),
                TotalPrintHours = printers.Sum(p => p.TotalPrintHours)
            };
        }

        public async Task<PrinterDto?> UpdateAsync(int id, UpdatePrinterRequest request)
        {
            var printer = await _printerRepository.GetByIdAsync(id);
            if (printer == null) return null;

            var oldNom = printer.Nom;

            // Mise à jour des propriétés
            var propertyInfo = printer.GetType().GetProperty("Nom");
            propertyInfo?.SetValue(printer, request.Nom);

            propertyInfo = printer.GetType().GetProperty("IpAddress");
            propertyInfo?.SetValue(printer, request.IpAddress);

            propertyInfo = printer.GetType().GetProperty("IsActive");
            propertyInfo?.SetValue(printer, request.IsActive);

            propertyInfo = printer.GetType().GetProperty("UpdatedAt");
            propertyInfo?.SetValue(printer, DateTime.UtcNow);

            var updated = await _printerRepository.UpdateAsync(printer);

            if (oldNom != request.Nom)
            {
                await _auditLogger.LogUpdateAsync(EntityType.PrintJob, id, "Nom", oldNom, request.Nom);
            }

            return MapToDto(updated);
        }

        public async Task<PrinterDto?> UpdateStatusAsync(int id, string status)
        {
            var printer = await _printerRepository.GetByIdAsync(id);
            if (printer == null) return null;

            var newStatus = Enum.Parse<PrinterStatus>(status);
            var oldStatus = printer.Status.ToString();

            switch (newStatus)
            {
                case PrinterStatus.Available:
                    printer.SetAvailable();
                    break;
                case PrinterStatus.Maintenance:
                    printer.SetMaintenance();
                    break;
                default:
                    printer.UpdateStatus(newStatus);
                    break;
            }

            var updated = await _printerRepository.UpdateAsync(printer);

            await _auditLogger.LogStatusChangeAsync(EntityType.PrintJob, id, oldStatus, status);

            return MapToDto(updated);
        }

        private static PrinterDto MapToDto(Printer printer)
        {
            return new PrinterDto
            {
                Id = printer.Id,
                Nom = printer.Nom,
                Reference = printer.Reference,
                Model = printer.Model,
                Brand = printer.Brand,
                Type = printer.Type.ToString(),
                Status = printer.Status.ToString(),
                IpAddress = printer.IpAddress,
                MaxPrintSizeX = printer.MaxPrintSizeX,
                MaxPrintSizeY = printer.MaxPrintSizeY,
                MaxPrintSizeZ = printer.MaxPrintSizeZ,
                TotalPrintHours = printer.TotalPrintHours,
                TotalPrintJobs = printer.TotalPrintJobs,
                LastMaintenance = printer.LastMaintenance,
                LastPrint = printer.LastPrint,
                IsActive = printer.IsActive
            };
        }
    }
}
