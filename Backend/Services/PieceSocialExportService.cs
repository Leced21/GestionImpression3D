using Backend.Data;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace Backend.Services
{
    public class PieceSocialExportService : IPieceSocialExportService
    {
        private readonly AppDbContext _context;
        private readonly IPricingService _pricingService;

        public PieceSocialExportService(AppDbContext context, IPricingService pricingService)
        {
            _context = context;
            _pricingService = pricingService;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]?> ExportProductPostPngAsync(int pieceId, string format)
        {
            var piece = await _context.Pieces.AsNoTracking().FirstOrDefaultAsync(p => p.Id == pieceId);
            if (piece == null)
            {
                return null;
            }

            var pricing = await _pricingService.GetPiecePricingAsync(pieceId);
            return await Task.Run(() => RenderProductPost(piece, pricing, ResolveSize(format)));
        }

        public async Task<byte[]?> ExportProductPostPdfAsync(int pieceId, string format)
        {
            var png = await ExportProductPostPngAsync(pieceId, format);
            if (png == null)
            {
                return null;
            }

            var size = ResolveSize(format);
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(new PageSize(size.Width, size.Height, Unit.Point));
                        page.Margin(0);
                        page.Content().Image(png).FitArea();
                    });
                });

                return document.GeneratePdf();
            });
        }

        private static SocialExportSize ResolveSize(string format)
        {
            return string.Equals(format, "story", StringComparison.OrdinalIgnoreCase)
                ? new SocialExportSize(1080, 1920)
                : new SocialExportSize(1080, 1080);
        }

        private static byte[] RenderProductPost(Piece piece, PiecePricingDto? pricing, SocialExportSize size)
        {
            using var bitmap = new SKBitmap(size.Width, size.Height);
            using var canvas = new SKCanvas(bitmap);

            var background = new SKRect(0, 0, size.Width, size.Height);
            using var backgroundPaint = new SKPaint
            {
                IsAntialias = true,
                Shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, 0),
                    new SKPoint(size.Width, size.Height),
                    new[] { new SKColor(14, 40, 65), new SKColor(23, 119, 182), new SKColor(245, 247, 250) },
                    new[] { 0f, 0.62f, 1f },
                    SKShaderTileMode.Clamp)
            };
            canvas.DrawRect(background, backgroundPaint);

            var margin = size.Width * 0.075f;
            var panelTop = size.Height * 0.08f;
            var panelHeight = size.Height * 0.84f;
            using var panelPaint = new SKPaint { IsAntialias = true, Color = new SKColor(255, 255, 255, 238) };
            canvas.DrawRoundRect(new SKRect(margin, panelTop, size.Width - margin, panelTop + panelHeight), 36, 36, panelPaint);

            DrawBrand(canvas, margin + 46, panelTop + 78);

            var visualSize = Math.Min(size.Width * 0.52f, size.Height * 0.28f);
            var visualCenterX = size.Width / 2f;
            var visualCenterY = panelTop + (size.Height > size.Width ? size.Height * 0.31f : size.Height * 0.29f);
            DrawProductVisual(canvas, visualCenterX, visualCenterY, visualSize, GetMaterialLabel(piece));

            var textLeft = margin + 58;
            var textRight = size.Width - margin - 58;
            var titleTop = visualCenterY + visualSize * 0.48f + 58;
            using var titlePaint = TextPaint(64, new SKColor(14, 40, 65), true);
            DrawWrappedText(canvas, GetProductName(piece), textLeft, titleTop, textRight - textLeft, titlePaint, 1.05f, 2);

            if (!string.IsNullOrWhiteSpace(piece.SloganProduit))
            {
                using var sloganPaint = TextPaint(31, new SKColor(14, 40, 65), false);
                DrawWrappedText(canvas, piece.SloganProduit, textLeft, titleTop + 140, textRight - textLeft, sloganPaint, 1.12f, 1);
            }

            using var refPaint = TextPaint(28, new SKColor(76, 91, 111), false);
            canvas.DrawText($"REF {piece.Reference}", textLeft, titleTop + 188, refPaint);

            var description = GetProductDescription(piece)
                ?? $"Produit imprime en {GetMaterialLabel(piece)} par 3D Inspire.";
            using var descPaint = TextPaint(32, new SKColor(35, 47, 65), false);
            DrawWrappedText(canvas, description, textLeft, titleTop + 250, textRight - textLeft, descPaint, 1.32f, size.Height > size.Width ? 4 : 3);

            var price = piece.PrixVente > 0m
                ? piece.PrixVente
                : pricing?.RecommendedPriceHt ?? 0m;
            var footerTop = panelTop + panelHeight - 180;
            DrawPill(canvas, textLeft, footerTop, GetMaterialLabel(piece), new SKColor(227, 242, 253), new SKColor(14, 90, 145));
            DrawPill(canvas, textLeft + 210, footerTop, piece.Stock > 0 ? $"{piece.Stock} en stock" : "Sur commande", new SKColor(232, 245, 233), new SKColor(28, 105, 56));
            if (piece.PoidsProduitGrammes.HasValue)
            {
                DrawPill(canvas, textLeft + 470, footerTop, $"{piece.PoidsProduitGrammes:F0} g", new SKColor(255, 243, 224), new SKColor(132, 75, 0));
            }

            using var pricePaint = TextPaint(58, new SKColor(14, 40, 65), true);
            var priceText = price > 0m ? $"{price:F2} EUR HT" : "Prix sur devis";
            canvas.DrawText(priceText, textLeft, footerTop + 118, pricePaint);

            using var ctaPaint = TextPaint(28, new SKColor(76, 91, 111), false);
            canvas.DrawText(piece.AccrocheMarketing ?? "3D Inspire", textLeft, panelTop + panelHeight - 42, ctaPaint);

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 95);
            return data.ToArray();
        }

        private static SKPaint TextPaint(float size, SKColor color, bool bold)
        {
            return new SKPaint
            {
                IsAntialias = true,
                Color = color,
                TextSize = size,
                Typeface = SKTypeface.FromFamilyName("Segoe UI", bold ? SKFontStyle.Bold : SKFontStyle.Normal)
            };
        }

        private static void DrawBrand(SKCanvas canvas, float x, float y)
        {
            using var iconPaint = new SKPaint { IsAntialias = true, Color = new SKColor(14, 40, 65) };
            canvas.DrawRoundRect(new SKRect(x, y - 42, x + 54, y + 12), 14, 14, iconPaint);

            using var brandPaint = TextPaint(30, new SKColor(14, 40, 65), true);
            canvas.DrawText("3D INSPIRE", x + 74, y, brandPaint);
        }

        private static void DrawProductVisual(SKCanvas canvas, float cx, float cy, float size, string material)
        {
            using var shadowPaint = new SKPaint { IsAntialias = true, Color = new SKColor(14, 40, 65, 28) };
            canvas.DrawOval(new SKRect(cx - size * 0.46f, cy + size * 0.32f, cx + size * 0.46f, cy + size * 0.46f), shadowPaint);

            using var bodyPaint = new SKPaint
            {
                IsAntialias = true,
                Shader = SKShader.CreateLinearGradient(
                    new SKPoint(cx - size * 0.35f, cy - size * 0.42f),
                    new SKPoint(cx + size * 0.35f, cy + size * 0.38f),
                    new[] { new SKColor(70, 178, 255), new SKColor(14, 40, 65) },
                    null,
                    SKShaderTileMode.Clamp)
            };
            var body = new SKRoundRect(new SKRect(cx - size * 0.31f, cy - size * 0.42f, cx + size * 0.31f, cy + size * 0.38f), size * 0.12f, size * 0.12f);
            canvas.DrawRoundRect(body, bodyPaint);

            using var linePaint = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(255, 255, 255, 80),
                StrokeWidth = Math.Max(5, size * 0.012f)
            };

            for (var offset = -0.24f; offset <= 0.24f; offset += 0.12f)
            {
                canvas.DrawLine(cx - size * 0.27f, cy + size * offset, cx + size * 0.27f, cy + size * offset, linePaint);
            }

            using var materialPaint = TextPaint(28, SKColors.White, true);
            var label = material.ToUpperInvariant();
            var labelWidth = materialPaint.MeasureText(label);
            canvas.DrawText(label, cx - labelWidth / 2f, cy + size * 0.02f, materialPaint);
        }

        private static void DrawPill(SKCanvas canvas, float x, float y, string text, SKColor background, SKColor foreground)
        {
            using var textPaint = TextPaint(26, foreground, true);
            var width = textPaint.MeasureText(text) + 44;
            using var backgroundPaint = new SKPaint { IsAntialias = true, Color = background };
            canvas.DrawRoundRect(new SKRect(x, y, x + width, y + 52), 18, 18, backgroundPaint);
            canvas.DrawText(text, x + 22, y + 35, textPaint);
        }

        private static void DrawWrappedText(SKCanvas canvas, string text, float x, float y, float maxWidth, SKPaint paint, float lineHeight, int maxLines)
        {
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var lines = new List<string>();
            var current = string.Empty;

            foreach (var word in words)
            {
                var candidate = string.IsNullOrWhiteSpace(current) ? word : $"{current} {word}";
                if (paint.MeasureText(candidate) <= maxWidth)
                {
                    current = candidate;
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(current))
                {
                    lines.Add(current);
                }

                current = word;
                if (lines.Count == maxLines)
                {
                    break;
                }
            }

            if (lines.Count < maxLines && !string.IsNullOrWhiteSpace(current))
            {
                lines.Add(current);
            }

            if (lines.Count > maxLines)
            {
                lines = lines.Take(maxLines).ToList();
            }

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (i == maxLines - 1 && words.Length > 0 && string.Join(" ", lines) != text)
                {
                    line = TrimToWidth($"{line}...", maxWidth, paint);
                }

                canvas.DrawText(line, x, y + i * paint.TextSize * lineHeight, paint);
            }
        }

        private static string TrimToWidth(string text, float maxWidth, SKPaint paint)
        {
            var value = text;
            while (value.Length > 3 && paint.MeasureText(value) > maxWidth)
            {
                value = $"{value[..^4]}...";
            }

            return value;
        }

        private static string GetProductName(Piece piece)
        {
            return string.IsNullOrWhiteSpace(piece.NomCommercial) ? piece.Nom : piece.NomCommercial;
        }

        private static string? GetProductDescription(Piece piece)
        {
            return string.IsNullOrWhiteSpace(piece.DescriptionMarketing)
                ? piece.Description
                : piece.DescriptionMarketing;
        }

        private static string GetMaterialLabel(Piece piece)
        {
            return string.IsNullOrWhiteSpace(piece.MatiereMarketing)
                ? piece.Materiau.ToString()
                : piece.MatiereMarketing;
        }

        private readonly record struct SocialExportSize(int Width, int Height);
    }
}
