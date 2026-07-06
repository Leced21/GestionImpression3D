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
                            .Text("Ce devis est valable 30 jours - 3D Inspire")
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
                            .Text("3D Inspire - Fiche Technique")
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
                                    table.Cell().Text(piece.Statut.ToString());

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
                            .Text($"Généré le {DateTime.Now:dd/MM/yyyy HH:mm} - 3D Inspire")
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
                            .Text("3D Inspire - Fiche Projet")
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
                                    table.Cell().Text(projet.Statut.ToString());

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
                                x.Span(" - 3D Inspire");
                            });
                    });
                });

                return document.GeneratePdf();
            });
        }

        public async Task<byte[]> ExportDevisPdfAsync(Devis devis)
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Column(column =>
                        {
                            column.Item().AlignCenter().Text("DEVIS")
                                .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);
                            column.Item().AlignCenter().Text($"N° {devis.NumeroDevis}")
                                .FontSize(12).FontColor(Colors.Grey.Medium);
                        });

                        page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                });

                                table.Cell().Text("Client:").Bold();
                                table.Cell().Text(devis.Client?.Nom ?? "N/A");

                                table.Cell().Text("Email:").Bold();
                                table.Cell().Text(devis.Client?.Email ?? "N/A");

                                table.Cell().Text("Date d'émission:").Bold();
                                table.Cell().Text(devis.DateEmission.ToString("dd/MM/yyyy"));

                                table.Cell().Text("Valable jusqu'au:").Bold();
                                table.Cell().Text(devis.DateValidite.ToString("dd/MM/yyyy"));

                                table.Cell().Text("Statut:").Bold();
                                table.Cell().Text(devis.Statut.ToString());
                            });

                            column.Item().PaddingTop(15).Text("📦 Lignes du devis").SemiBold().FontSize(14);
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(4);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Désignation").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Qté").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Prix unitaire").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Total").Bold();
                                });

                                foreach (var ligne in devis.Lignes)
                                {
                                    table.Cell().Padding(5).Text(ligne.Description);
                                    table.Cell().Padding(5).Text(ligne.Quantite.ToString());
                                    table.Cell().Padding(5).Text($"{ligne.PrixUnitaire:F2} €");
                                    table.Cell().Padding(5).Text($"{ligne.Total:F2} €");
                                }

                                table.Cell().ColumnSpan(3).Padding(5).Text("Total HT:").Bold().AlignRight();
                                table.Cell().Padding(5).Text($"{devis.TotalHT:F2} €").Bold();

                                table.Cell().ColumnSpan(3).Padding(5).Text($"TVA ({devis.TVA:F0}%):").AlignRight();
                                table.Cell().Padding(5).Text($"{(devis.TotalTTC - devis.TotalHT):F2} €");

                                table.Cell().ColumnSpan(3).Padding(5).Text("Total TTC:").Bold().AlignRight();
                                table.Cell().Padding(5).Text($"{devis.TotalTTC:F2} €").Bold().FontColor(Colors.Green.Medium);
                            });

                            if (!string.IsNullOrWhiteSpace(devis.Conditions))
                            {
                                column.Item().PaddingTop(15).Text("💬 Conditions").SemiBold().FontSize(14);
                                column.Item().Text(devis.Conditions).FontSize(10);
                            }

                            if (!string.IsNullOrWhiteSpace(devis.Notes))
                            {
                                column.Item().PaddingTop(10).Text("📝 Notes").SemiBold().FontSize(14);
                                column.Item().Text(devis.Notes).FontSize(10);
                            }
                        });

                        page.Footer().AlignCenter()
                            .Text($"Devis généré le {DateTime.Now:dd/MM/yyyy HH:mm} - 3D Inspire")
                            .FontSize(10).FontColor(Colors.Grey.Medium);
                    });
                });

                return document.GeneratePdf();
            });
        }

        public async Task<byte[]> ExportFicheProduitPdfAsync(Piece piece, STLMetadata? stlMetadata)
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Column(column =>
                        {
                            column.Item().AlignCenter().Text("FICHE PRODUIT")
                                .SemiBold().FontSize(22).FontColor(Colors.Blue.Medium);
                            column.Item().AlignCenter().Text(piece.Nom)
                                .FontSize(14).FontColor(Colors.Grey.Medium);
                        });

                        page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                        {
                            column.Item().Text("1. Informations générales").SemiBold().FontSize(14);
                            column.Item().PaddingBottom(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                });

                                table.Cell().Text("Nom du produit :").Bold();
                                table.Cell().Text(piece.Nom);

                                table.Cell().Text("Référence :").Bold();
                                table.Cell().Text(piece.Reference);

                                table.Cell().Text("Catégorie :").Bold();
                                CellOrPlaceholder(table.Cell(), piece.Categorie);

                                table.Cell().Text("Marque :").Bold();
                                table.Cell().Text("3D Inspire");
                            });

                            column.Item().Text("2. Description").SemiBold().FontSize(14);
                            CellOrPlaceholder(column.Item().PaddingBottom(10), piece.Description);

                            column.Item().Text("3. Caractéristiques techniques").SemiBold().FontSize(14);
                            column.Item().PaddingBottom(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                });

                                table.Cell().Text("Dimensions (L x l x h) :").Bold();
                                if (stlMetadata != null)
                                    table.Cell().Text($"{stlMetadata.BoundingBoxX} x {stlMetadata.BoundingBoxY} x {stlMetadata.BoundingBoxZ} mm");
                                else
                                    CellOrPlaceholder(table.Cell(), null, "À compléter (fichier STL non analysé)");

                                table.Cell().Text("Volume :").Bold();
                                if (stlMetadata != null)
                                    table.Cell().Text($"{stlMetadata.Volume} cm³");
                                else
                                    CellOrPlaceholder(table.Cell(), null, "À compléter (fichier STL non analysé)");

                                table.Cell().Text("Poids estimé :").Bold();
                                if (stlMetadata != null)
                                    table.Cell().Text($"{stlMetadata.EstimatedWeight} g");
                                else
                                    CellOrPlaceholder(table.Cell(), null, "À compléter (fichier STL non analysé)");

                                table.Cell().Text("Matériau(x) :").Bold();
                                CellOrPlaceholder(table.Cell(), piece.Materiau);

                                table.Cell().Text("Couleur(s) :").Bold();
                                CellOrPlaceholder(table.Cell(), piece.Couleurs);

                                table.Cell().Text("Capacité / Contenance :").Bold();
                                CellOrPlaceholder(table.Cell(), piece.CapaciteContenance);

                                table.Cell().Text("Normes / Certifications :").Bold();
                                CellOrPlaceholder(table.Cell(), piece.NormesCertifications);
                            });

                            column.Item().Text("4. Utilisation").SemiBold().FontSize(14);
                            column.Item().PaddingBottom(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                });

                                table.Cell().Text("Instructions d'utilisation :").Bold();
                                CellOrPlaceholder(table.Cell(), piece.InstructionsUtilisation);

                                table.Cell().Text("Précautions d'usage :").Bold();
                                CellOrPlaceholder(table.Cell(), piece.PrecautionsUsage);

                                table.Cell().Text("Public cible :").Bold();
                                CellOrPlaceholder(table.Cell(), piece.PublicCible);
                            });

                            column.Item().Text("5. Conditionnement et logistique").SemiBold().FontSize(14);
                            column.Item().PaddingBottom(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                });

                                table.Cell().Text("Conditionnement :").Bold();
                                CellOrPlaceholder(table.Cell(), piece.Conditionnement);

                                table.Cell().Text("Dimensions du colis :").Bold();
                                CellOrPlaceholder(table.Cell(), piece.DimensionsColis);

                                table.Cell().Text("Poids du colis :").Bold();
                                CellOrPlaceholder(table.Cell(), piece.PoidsColisKg.HasValue ? $"{piece.PoidsColisKg} kg" : null);

                                table.Cell().Text("Quantité minimum de commande :").Bold();
                                CellOrPlaceholder(table.Cell(), piece.MoqUnites.HasValue ? $"{piece.MoqUnites} unités" : null);

                                table.Cell().Text("Délai de livraison estimé :").Bold();
                                CellOrPlaceholder(table.Cell(), piece.DelaiLivraisonJours.HasValue ? $"{piece.DelaiLivraisonJours} jours" : null);
                            });

                            column.Item().Text("6. Éléments marketing").SemiBold().FontSize(14);
                            column.Item().PaddingBottom(5).Text("Points forts / avantages :").Bold();
                            CellOrPlaceholder(column.Item().PaddingBottom(10), piece.PointsForts);

                            column.Item().PaddingBottom(5).Text("FAQ (questions fréquentes) :").Bold();
                            CellOrPlaceholder(column.Item().PaddingBottom(10), piece.Faq);

                            column.Item().Text("7. Prix").SemiBold().FontSize(14);
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                });

                                var tva = piece.PrixVente * 0.2m;
                                table.Cell().Text("Prix unitaire HT :").Bold();
                                table.Cell().Text($"{piece.PrixVente:F2} €");

                                table.Cell().Text("Prix unitaire TTC (TVA 20%) :").Bold();
                                table.Cell().Text($"{(piece.PrixVente + tva):F2} €").FontColor(Colors.Green.Medium);

                                table.Cell().Text("Tarifs dégressifs :").Bold();
                                CellOrPlaceholder(table.Cell(), piece.TarifsDegressifs);
                            });
                        });

                        page.Footer().AlignCenter()
                            .Text($"Généré le {DateTime.Now:dd/MM/yyyy HH:mm} - 3D Inspire")
                            .FontSize(10).FontColor(Colors.Grey.Medium);
                    });
                });

                return document.GeneratePdf();
            });
        }

        private static void CellOrPlaceholder(IContainer cell, string? value, string placeholder = "À compléter")
        {
            if (string.IsNullOrWhiteSpace(value))
                cell.Text(placeholder).Italic().FontColor(Colors.Grey.Medium);
            else
                cell.Text(value);
        }

        public async Task<byte[]> ExportFacturePdfAsync(Facture facture)
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Column(column =>
                        {
                            column.Item().AlignCenter().Text("FACTURE")
                                .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);
                            column.Item().AlignCenter().Text($"N° {facture.NumeroFacture}")
                                .FontSize(12).FontColor(Colors.Grey.Medium);
                        });

                        page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                });

                                table.Cell().Text("Client:").Bold();
                                table.Cell().Text(facture.Client?.Nom ?? "N/A");

                                table.Cell().Text("Email:").Bold();
                                table.Cell().Text(facture.Client?.Email ?? "N/A");

                                table.Cell().Text("Date d'émission:").Bold();
                                table.Cell().Text(facture.DateEmission.ToString("dd/MM/yyyy"));

                                table.Cell().Text("Date d'échéance:").Bold();
                                table.Cell().Text(facture.DateEcheance.ToString("dd/MM/yyyy"));

                                table.Cell().Text("Statut:").Bold();
                                table.Cell().Text(facture.Statut.ToString());
                            });

                            column.Item().PaddingTop(15).Text("📦 Détail").SemiBold().FontSize(14);
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(4);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Désignation").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Qté").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Prix unitaire").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Total").Bold();
                                });

                                foreach (var ligne in facture.Lignes)
                                {
                                    table.Cell().Padding(5).Text(ligne.Description);
                                    table.Cell().Padding(5).Text(ligne.Quantite.ToString());
                                    table.Cell().Padding(5).Text($"{ligne.PrixUnitaire:F2} €");
                                    table.Cell().Padding(5).Text($"{ligne.Total:F2} €");
                                }

                                table.Cell().ColumnSpan(3).Padding(5).Text("Total HT:").Bold().AlignRight();
                                table.Cell().Padding(5).Text($"{facture.TotalHT:F2} €").Bold();

                                table.Cell().ColumnSpan(3).Padding(5).Text($"TVA ({facture.TVA:F0}%):").AlignRight();
                                table.Cell().Padding(5).Text($"{(facture.TotalTTC - facture.TotalHT):F2} €");

                                table.Cell().ColumnSpan(3).Padding(5).Text("Total TTC:").Bold().AlignRight();
                                table.Cell().Padding(5).Text($"{facture.TotalTTC:F2} €").Bold().FontColor(Colors.Green.Medium);
                            });

                            if (!string.IsNullOrWhiteSpace(facture.Notes))
                            {
                                column.Item().PaddingTop(15).Text("📝 Notes").SemiBold().FontSize(14);
                                column.Item().Text(facture.Notes).FontSize(10);
                            }
                        });

                        page.Footer().AlignCenter()
                            .Text($"Facture générée le {DateTime.Now:dd/MM/yyyy HH:mm} - 3D Inspire")
                            .FontSize(10).FontColor(Colors.Grey.Medium);
                    });
                });

                return document.GeneratePdf();
            });
        }
    }
}
