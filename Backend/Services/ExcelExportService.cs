using Backend.Interface;
using OfficeOpenXml;

namespace Backend.Services
{
    public class ExcelExportService : IExcelExportService
    {
        private readonly IExcelExportRepository _exportRepository;
        public ExcelExportService(IExcelExportRepository exportRepository)
        {
            _exportRepository = exportRepository;
            ExcelPackage.License.SetNonCommercialPersonal("3DInspire"); // Nécessaire pour EPPlus 5+
        }
        
        public async Task<byte[]> ExportCommandesToExcelAsync()
        {
            var commandes = await _exportRepository.GetAllCommandesForExportAsync();

            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Commandes");

                ExcelExportHelper.CreateHeaders(worksheet,
                    "ID", "N° Commande", "Client", "Email", "Total (€)", "Statut", "Date Commande", "Date Livraison");

                int row = 2;
                foreach (var commande in commandes)
                {
                    row = ExcelExportHelper.AddRow(worksheet, row,
                        commande.Id,
                        commande.NumeroCommande,
                        commande.ClientNom,
                        commande.ClientEmail,
                        commande.Total,
                        commande.Statut,
                        commande.DateCommande.ToString("dd/MM/yyyy"),
                        commande.DateLivraison?.ToString("dd/MM/yyyy") ?? "Non livrée");
                }

                return ExcelExportHelper.FinalizeExport(package, worksheet);
            });
        }

        public async Task<byte[]> ExportMaterialStockToExcelAsync()
        {
            var materials = await _exportRepository.GetAllMaterialsForExportAsync();

            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Stocks Matière");

                ExcelExportHelper.CreateHeaders(worksheet,
                    "ID", "Nom", "Type", "Marque", "Couleur", "Quantité", "Unité", "Seuil Min",
                    "Prix Unitaire (€)", "Valeur Totale (€)", "Emplacement", "Fournisseur", "Statut");

                int row = 2;
                foreach (var material in materials)
                {
                    var valeurTotale = material.Quantity * material.UnitPrice;
                    var isLowStock = material.IsLowStock();

                    row = ExcelExportHelper.AddRowWithConditionalColor(worksheet, row,
                        () => isLowStock, 13, System.Drawing.Color.Red,
                        material.Id,
                        material.Name,
                        material.Type.ToString(),
                        material.Brand,
                        material.Color,
                        material.Quantity,
                        material.Unit.ToString(),
                        material.MinThreshold,
                        material.UnitPrice,
                        valeurTotale,
                        material.Location ?? "N/A",
                        material.Supplier ?? "N/A",
                        isLowStock ? "Stock bas" : "OK");
                }

                return ExcelExportHelper.FinalizeExport(package, worksheet);
            });
        }

        public async Task<byte[]> ExportPiecesToExcelAsync()
        {
            var pieces = await _exportRepository.GetAllPiecesForExportAsync();
            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Pièces");

                ExcelExportHelper.CreateHeaders(worksheet,
                    "ID", "Nom", "Référence", "Statut", "Catégorie", "Matériau", "Prix Vente (€)",
                    "Coût Matière (€)", "Coût Machine (€)", "Coût MO (€)", "Stock", "Date Création");

                int row = 2;
                foreach (var piece in pieces)
                {
                    row = ExcelExportHelper.AddRow(worksheet, row,
                        piece.Id,
                        piece.Nom,
                        piece.Reference,
                        piece.Statut,
                        piece.Categorie ?? "N/A",
                        piece.Materiau ?? "N/A",
                        piece.PrixVente,
                        piece.CoutMatiere,
                        piece.CoutMachine,
                        piece.CoutMainOeuvre,
                        piece.Stock,
                        piece.DateCreation.ToString("dd/MM/yyyy"));
                }

                return ExcelExportHelper.FinalizeExport(package, worksheet);
            });
        }

        public async Task<byte[]> ExportPrintJobsToExcelAsync()
        {
            var jobs = await _exportRepository.GetAllPrintJobsForExportAsync();

            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Jobs Impression");

                ExcelExportHelper.CreateHeaders(worksheet,
                    "ID", "Job N°", "Pièce", "Imprimante", "Quantité", "Statut", "Priorité",
                    "Durée (min)", "Matériau (g)", "Date Création", "Date Fin");

                int row = 2;
                foreach (var job in jobs)
                {
                    row = ExcelExportHelper.AddRow(worksheet, row,
                        job.Id,
                        job.JobNumber,
                        job.Piece?.Nom ?? "N/A",
                        job.Printer?.Nom ?? "Non assignée",
                        $"{job.QuantityCompleted}/{job.Quantity}",
                        job.Status.ToString(),
                        job.Priority.ToString(),
                        job.ActualDurationMinutes ?? job.EstimatedDurationMinutes,
                        job.ActualMaterialGrams > 0 ? job.ActualMaterialGrams : job.EstimatedMaterialGrams,
                        job.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                        job.CompletedAt?.ToString("dd/MM/yyyy HH:mm") ?? "En cours");
                }

                return ExcelExportHelper.FinalizeExport(package, worksheet);
            });
        }

        public async Task<byte[]> ExportProjetsToExcelAsync()
        {
            var projets = await _exportRepository.GetAllProjetsForExportAsync();

            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Projets");

                ExcelExportHelper.CreateHeaders(worksheet,
                    "ID", "Nom", "Référence", "Statut", "Client", "Budget (€)",
                    "Nombre Pièces", "Date Création", "Livraison Prévue");

                int row = 2;
                foreach (var projet in projets)
                {
                    row = ExcelExportHelper.AddRow(worksheet, row,
                        projet.Id,
                        projet.Nom,
                        projet.Reference,
                        projet.Statut,
                        projet.ClientNom ?? "N/A",
                        projet.Budget,
                        projet.ProjetPieces?.Count ?? 0,
                        projet.DateCreation?.ToString("dd/MM/yyyy") ?? "N/A",
                        projet.DateLivraisonPrevue?.ToString("dd/MM/yyyy") ?? "N/A");
                }

                return ExcelExportHelper.FinalizeExport(package, worksheet);
            });
        }
    }
}
