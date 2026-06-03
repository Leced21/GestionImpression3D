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

                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "N° Commande";
                worksheet.Cells[1, 3].Value = "Client";
                worksheet.Cells[1, 4].Value = "Email";
                worksheet.Cells[1, 5].Value = "Total (€)";
                worksheet.Cells[1, 6].Value = "Statut";
                worksheet.Cells[1, 7].Value = "Date Commande";
                worksheet.Cells[1, 8].Value = "Date Livraison";

                using (var range = worksheet.Cells[1, 1, 1, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                int row = 2;
                foreach (var commande in commandes)
                {
                    worksheet.Cells[row, 1].Value = commande.Id;
                    worksheet.Cells[row, 2].Value = commande.NumeroCommande;
                    worksheet.Cells[row, 3].Value = commande.ClientNom;
                    worksheet.Cells[row, 4].Value = commande.ClientEmail;
                    worksheet.Cells[row, 5].Value = commande.Total;
                    worksheet.Cells[row, 6].Value = commande.Statut;
                    worksheet.Cells[row, 7].Value = commande.DateCommande.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 8].Value = commande.DateLivraison?.ToString("dd/MM/yyyy") ?? "Non livrée";
                    row++;
                }

                worksheet.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            });
        }

        public async Task<byte[]> ExportMaterialStockToExcelAsync()
        {
            var materials = await _exportRepository.GetAllMaterialsForExportAsync();

            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Stocks Matière");

                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Nom";
                worksheet.Cells[1, 3].Value = "Type";
                worksheet.Cells[1, 4].Value = "Marque";
                worksheet.Cells[1, 5].Value = "Couleur";
                worksheet.Cells[1, 6].Value = "Quantité";
                worksheet.Cells[1, 7].Value = "Unité";
                worksheet.Cells[1, 8].Value = "Seuil Min";
                worksheet.Cells[1, 9].Value = "Prix Unitaire (€)";
                worksheet.Cells[1, 10].Value = "Valeur Totale (€)";
                worksheet.Cells[1, 11].Value = "Emplacement";
                worksheet.Cells[1, 12].Value = "Fournisseur";
                worksheet.Cells[1, 13].Value = "Statut";

                using (var range = worksheet.Cells[1, 1, 1, 13])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                int row = 2;
                foreach (var material in materials)
                {
                    var valeurTotale = material.Quantity * material.UnitPrice;

                    worksheet.Cells[row, 1].Value = material.Id;
                    worksheet.Cells[row, 2].Value = material.Name;
                    worksheet.Cells[row, 3].Value = material.Type.ToString();
                    worksheet.Cells[row, 4].Value = material.Brand;
                    worksheet.Cells[row, 5].Value = material.Color;
                    worksheet.Cells[row, 6].Value = material.Quantity;
                    worksheet.Cells[row, 7].Value = material.Unit.ToString();
                    worksheet.Cells[row, 8].Value = material.MinThreshold;
                    worksheet.Cells[row, 9].Value = material.UnitPrice;
                    worksheet.Cells[row, 10].Value = valeurTotale;
                    worksheet.Cells[row, 11].Value = material.Location ?? "N/A";
                    worksheet.Cells[row, 12].Value = material.Supplier ?? "N/A";
                    worksheet.Cells[row, 13].Value = material.IsLowStock() ? "Stock bas" : "OK";

                    if (material.IsLowStock())
                    {
                        worksheet.Cells[row, 13].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    }
                    row++;
                }

                worksheet.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            });
        }

        public async Task<byte[]> ExportPiecesToExcelAsync()
        {
            var pieces = await _exportRepository.GetAllPiecesForExportAsync();
            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Pièces");

                // En-têtes
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Nom";
                worksheet.Cells[1, 3].Value = "Référence";
                worksheet.Cells[1, 4].Value = "Statut";
                worksheet.Cells[1, 5].Value = "Catégorie";
                worksheet.Cells[1, 6].Value = "Matériau";
                worksheet.Cells[1, 7].Value = "Prix Vente (€)";
                worksheet.Cells[1, 8].Value = "Coût Matière (€)";
                worksheet.Cells[1, 9].Value = "Coût Machine (€)";
                worksheet.Cells[1, 10].Value = "Coût MO (€)";
                worksheet.Cells[1, 11].Value = "Stock";
                worksheet.Cells[1, 12].Value = "Date Création";

                using (var range = worksheet.Cells[1, 1, 1, 12])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                int row = 2;
                foreach (var piece in pieces)
                {
                    worksheet.Cells[row, 1].Value = piece.Id;
                    worksheet.Cells[row, 2].Value = piece.Nom;
                    worksheet.Cells[row, 3].Value = piece.Reference;
                    worksheet.Cells[row, 4].Value = piece.Statut;
                    worksheet.Cells[row, 5].Value = piece.Categorie ?? "N/A";
                    worksheet.Cells[row, 6].Value = piece.Materiau ?? "N/A";
                    worksheet.Cells[row, 7].Value = piece.PrixVente;
                    worksheet.Cells[row, 8].Value = piece.CoutMatiere;
                    worksheet.Cells[row, 9].Value = piece.CoutMachine;
                    worksheet.Cells[row, 10].Value = piece.CoutMainOeuvre;
                    worksheet.Cells[row, 11].Value = piece.Stock;
                    worksheet.Cells[row, 12].Value = piece.DateCreation.ToString("dd/MM/yyyy");
                    row++;
                }

                worksheet.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            });
        }

        public async Task<byte[]> ExportPrintJobsToExcelAsync()
        {
            var jobs = await _exportRepository.GetAllPrintJobsForExportAsync();

            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Jobs Impression");

                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Job N°";
                worksheet.Cells[1, 3].Value = "Pièce";
                worksheet.Cells[1, 4].Value = "Imprimante";
                worksheet.Cells[1, 5].Value = "Quantité";
                worksheet.Cells[1, 6].Value = "Statut";
                worksheet.Cells[1, 7].Value = "Priorité";
                worksheet.Cells[1, 8].Value = "Durée (min)";
                worksheet.Cells[1, 9].Value = "Matériau (g)";
                worksheet.Cells[1, 10].Value = "Date Création";
                worksheet.Cells[1, 11].Value = "Date Fin";

                using (var range = worksheet.Cells[1, 1, 1, 11])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                int row = 2;
                foreach (var job in jobs)
                {
                    worksheet.Cells[row, 1].Value = job.Id;
                    worksheet.Cells[row, 2].Value = job.JobNumber;
                    worksheet.Cells[row, 3].Value = job.Piece?.Nom ?? "N/A";
                    worksheet.Cells[row, 4].Value = job.Printer?.Nom ?? "Non assignée";
                    worksheet.Cells[row, 5].Value = $"{job.QuantityCompleted}/{job.Quantity}";
                    worksheet.Cells[row, 6].Value = job.Status.ToString();
                    worksheet.Cells[row, 7].Value = job.Priority.ToString();
                    worksheet.Cells[row, 8].Value = job.ActualDurationMinutes ?? job.EstimatedDurationMinutes;
                    worksheet.Cells[row, 9].Value = job.ActualMaterialGrams > 0 ? job.ActualMaterialGrams : job.EstimatedMaterialGrams;
                    worksheet.Cells[row, 10].Value = job.CreatedAt.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cells[row, 11].Value = job.CompletedAt?.ToString("dd/MM/yyyy HH:mm") ?? "En cours";
                    row++;
                }

                worksheet.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            });
        }

        public async Task<byte[]> ExportProjetsToExcelAsync()
        {
            var projets = await _exportRepository.GetAllProjetsForExportAsync();

            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Projets");

                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Nom";
                worksheet.Cells[1, 3].Value = "Référence";
                worksheet.Cells[1, 4].Value = "Statut";
                worksheet.Cells[1, 5].Value = "Client";
                worksheet.Cells[1, 6].Value = "Budget (€)";
                worksheet.Cells[1, 7].Value = "Nombre Pièces";
                worksheet.Cells[1, 8].Value = "Date Création";
                worksheet.Cells[1, 9].Value = "Livraison Prévue";

                using (var range = worksheet.Cells[1, 1, 1, 9])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                int row = 2;
                foreach (var projet in projets)
                {
                    worksheet.Cells[row, 1].Value = projet.Id;
                    worksheet.Cells[row, 2].Value = projet.Nom;
                    worksheet.Cells[row, 3].Value = projet.Reference;
                    worksheet.Cells[row, 4].Value = projet.Statut;
                    worksheet.Cells[row, 5].Value = projet.ClientNom ?? "N/A";
                    worksheet.Cells[row, 6].Value = projet.Budget;
                    worksheet.Cells[row, 7].Value = projet.ProjetPieces?.Count ?? 0;
                    worksheet.Cells[row, 8].Value = projet.DateCreation?.ToString("dd/MM/yyyy") ?? "N/A";
                    worksheet.Cells[row, 9].Value = projet.DateLivraisonPrevue?.ToString("dd/MM/yyyy") ?? "N/A";
                    row++;
                }

                worksheet.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            });
        }
    }
}
