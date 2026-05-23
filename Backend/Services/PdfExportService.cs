using Backend.Interface;
using Backend.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Backend.Services
{
    public class PdfExportService : IPdfExportService
    {
        public PdfExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }
        public async Task<byte[]> ExportDevisToPdfAsync(Projet projet)
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);

                        page.Header()
                            .Column(column =>
                            {
                                column.Item().AlignCenter().Text("DEVIS")
                                    .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);

                                column.Item().AlignCenter().Text($"N° {projet.Reference}")
                                    .FontSize(12).FontColor(Colors.Grey.Medium);
                            });

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Cell().Text("Date:").Bold();
                                    table.Cell().Text(DateTime.Now.ToString("dd/MM/yyyy"));

                                    table.Cell().Text("Client:").Bold();
                                    table.Cell().Text(string.IsNullOrEmpty(projet.ClientNom) ? "À définir" : projet.ClientNom);

                                    table.Cell().Text("Email:").Bold();
                                    table.Cell().Text(string.IsNullOrEmpty(projet.ClientEmail) ? "À définir" : projet.ClientEmail);
                                });

                                column.Item().PaddingTop(15).Text("📦 Détail des prestations").SemiBold().FontSize(14);
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Désignation").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Qté").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Montant").Bold();
                                    });

                                    decimal total = 0;
                                    foreach (var item in projet.ProjetPieces)
                                    {
                                        var prixUnitaire = item.Piece?.PrixVente ?? 0;
                                        var sousTotal = prixUnitaire * item.Quantite;
                                        total += sousTotal;

                                        table.Cell().Padding(5).Text(item.Piece?.Nom ?? "N/A");
                                        table.Cell().Padding(5).Text(item.Quantite.ToString());
                                        table.Cell().Padding(5).Text($"{sousTotal:F2} €");
                                    }

                                    table.Cell().ColumnSpan(2).Padding(5).Text("Total HT:").Bold().AlignRight();
                                    table.Cell().Padding(5).Text($"{total:F2} €").Bold();

                                    var tva = total * 0.2m;
                                    var totalTtc = total + tva;

                                    table.Cell().ColumnSpan(2).Padding(5).Text("TVA (20%):").AlignRight();
                                    table.Cell().Padding(5).Text($"{tva:F2} €");

                                    table.Cell().ColumnSpan(2).Padding(5).Text("Total TTC:").Bold().AlignRight();
                                    table.Cell().Padding(5).Text($"{totalTtc:F2} €").Bold().FontColor(Colors.Green.Medium);
                                });

                                column.Item().PaddingTop(15).Text("💬 Conditions")
                                    .SemiBold().FontSize(14);
                                column.Item().Text("• Délai de livraison : 2 à 3 semaines après validation du devis")
                                    .FontSize(10);
                                column.Item().Text("• Paiement : 30% à la commande, 70% à la livraison")
                                    .FontSize(10);
                                column.Item().Text("• Garantie : 12 mois sur les pièces imprimées")
                                    .FontSize(10);
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text("Ce devis est valable 30 jours - PrintFlow3D")
                            .FontSize(10).FontColor(Colors.Grey.Medium);
                    });
                });

                return document.GeneratePdf();
            });
        }

        public async Task<byte[]> ExportPieceToPdfAsync(Piece piece)
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);

                        page.Header()
                            .AlignCenter()
                            .Text("PrintFlow3D - Fiche Technique")
                            .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                column.Item().Text(piece.Nom).SemiBold().FontSize(16);
                                column.Item().PaddingBottom(10).Text($"Référence: {piece.Reference}").FontColor(Colors.Grey.Medium);

                                column.Item().Text("📊 Informations générales").SemiBold().FontSize(14);
                                column.Item().PaddingBottom(10).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Cell().Text("Statut:").Bold();
                                    table.Cell().Text(piece.Statut);

                                    table.Cell().Text("Description:").Bold();
                                    table.Cell().Text(string.IsNullOrEmpty(piece.Description) ? "Aucune description" : piece.Description);

                                    table.Cell().Text("Date création:").Bold();
                                    table.Cell().Text(piece.DateCreation.ToString("dd/MM/yyyy"));
                                });

                                column.Item().Text("💰 Analyse financière").SemiBold().FontSize(14);
                                column.Item().PaddingBottom(10).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Cell().Text("Coût matière:").Bold();
                                    table.Cell().Text($"{piece.CoutMatiere:F2} €");

                                    table.Cell().Text("Coût machine:").Bold();
                                    table.Cell().Text($"{piece.CoutMachine:F2} €");

                                    table.Cell().Text("Coût main d'œuvre:").Bold();
                                    table.Cell().Text($"{piece.CoutMainOeuvre:F2} €");

                                    table.Cell().Text("Coût total:").Bold();
                                    table.Cell().Text($"{piece.CoutMatiere + piece.CoutMachine + piece.CoutMainOeuvre:F2} €");

                                    table.Cell().Text("Prix de vente:").Bold();
                                    table.Cell().Text($"{piece.PrixVente:F2} €").FontColor(Colors.Green.Medium);
                                });

                                if (!string.IsNullOrEmpty(piece.Categorie) || !string.IsNullOrEmpty(piece.Materiau))
                                {
                                    column.Item().Text("🔧 Caractéristiques").SemiBold().FontSize(14);
                                    column.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(2);
                                        });

                                        if (!string.IsNullOrEmpty(piece.Categorie))
                                        {
                                            table.Cell().Text("Catégorie:").Bold();
                                            table.Cell().Text(piece.Categorie);
                                        }

                                        if (!string.IsNullOrEmpty(piece.Materiau))
                                        {
                                            table.Cell().Text("Matériau:").Bold();
                                            table.Cell().Text(piece.Materiau);
                                        }

                                        if (piece.Stock > 0)
                                        {
                                            table.Cell().Text("Stock disponible:").Bold();
                                            table.Cell().Text($"{piece.Stock} unités");
                                        }
                                    });
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text($"Généré le {DateTime.Now:dd/MM/yyyy HH:mm} - PrintFlow3D")
                            .FontSize(10).FontColor(Colors.Grey.Medium);
                    });
                });

                return document.GeneratePdf();
            });
        }

        public async Task<byte[]> ExportProjetToPdfAsync(Projet projet)
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        // En-tête
                        page.Header()
                            .AlignCenter()
                            .Text("PrintFlow3D - Fiche Projet")
                            .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                        // Contenu
                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                // Informations projet
                                column.Item().Text("📋 Informations générales").SemiBold().FontSize(14);
                                column.Item().PaddingBottom(10).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Cell().Text("Nom du projet:").Bold();
                                    table.Cell().Text(projet.Nom);

                                    table.Cell().Text("Référence:").Bold();
                                    table.Cell().Text(projet.Reference);

                                    table.Cell().Text("Statut:").Bold();
                                    table.Cell().Text(projet.Statut);

                                    table.Cell().Text("Date création:").Bold();
                                    table.Cell().Text(projet.DateCreation?.ToString("dd/MM/yyyy") ?? "");

                                    if (projet.DateLivraisonPrevue.HasValue)
                                    {
                                        table.Cell().Text("Livraison prévue:").Bold();
                                        table.Cell().Text(projet.DateLivraisonPrevue.Value.ToString("dd/MM/yyyy"));
                                    }
                                });

                                // Client
                                column.Item().PaddingTop(15).Text("👤 Client").SemiBold().FontSize(14);
                                column.Item().PaddingBottom(10).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Cell().Text("Nom:").Bold();
                                    table.Cell().Text(string.IsNullOrEmpty(projet.ClientNom) ? "Non renseigné" : projet.ClientNom);

                                    table.Cell().Text("Email:").Bold();
                                    table.Cell().Text(string.IsNullOrEmpty(projet.ClientEmail) ? "Non renseigné" : projet.ClientEmail);

                                    table.Cell().Text("Budget:").Bold();
                                    table.Cell().Text($"{projet.Budget:F2} €");
                                });

                                // Liste des pièces
                                column.Item().PaddingTop(15).Text("📦 Pièces du projet").SemiBold().FontSize(14);
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Pièce").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Référence").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Qté").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Prix unit.").Bold();
                                    });

                                    decimal total = 0;
                                    foreach (var item in projet.ProjetPieces)
                                    {
                                        var prixUnitaire = item.Piece?.PrixVente ?? 0;
                                        var sousTotal = prixUnitaire * item.Quantite;
                                        total += sousTotal;

                                        table.Cell().Padding(5).Text(item.Piece?.Nom ?? "N/A");
                                        table.Cell().Padding(5).Text(item.Piece?.Reference ?? "N/A");
                                        table.Cell().Padding(5).Text(item.Quantite.ToString());
                                        table.Cell().Padding(5).Text($"{prixUnitaire:F2} €");
                                    }

                                    table.Cell().ColumnSpan(3).Padding(5).Text("Total:").Bold().AlignRight();
                                    table.Cell().Padding(5).Text($"{total:F2} €").Bold();
                                });
                            });

                        // Pied de page
                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Document généré le ");
                                x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontColor(Colors.Grey.Medium);
                                x.Span(" - PrintFlow3D");
                            });
                    });
                });

                return document.GeneratePdf();
            });
        }
    }
}
