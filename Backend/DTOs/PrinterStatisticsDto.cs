namespace Backend.DTOs
{
    public class PrinterStatisticsDto
    {
        public int TotalPrinters { get; set; }
        public int AvailablePrinters { get; set; }
        public int PrintingPrinters { get; set; }
        public int MaintenancePrinters { get; set; }
        public int ErrorPrinters { get; set; }
        public int TotalPrintJobs { get; set; }
        public int TotalPrintHours { get; set; }
    }
}
