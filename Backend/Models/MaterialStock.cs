using Backend.Enums;

namespace Backend.Models
{
    public class MaterialStock
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public MaterialType Type { get; private set; }
        public string Brand { get; private set; } = string.Empty;
        public string Color { get; private set; } = string.Empty;
        public string? Reference { get; private set; }
        public decimal Quantity { get; private set; }
        public MaterialUnit Unit { get; private set; }
        public decimal MinThreshold { get; private set; }
        public decimal MaxThreshold { get; private set; }
        public string? Location { get; private set; }
        public string? Supplier { get; private set; }
        public decimal UnitPrice { get; private set; }
        public string? Currency { get; private set; }
        public DateTime? LastRestockedAt { get; private set; }
        public DateTime? LastUsedAt { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public string? Notes { get; private set; }

        private MaterialStock() { }

        public static MaterialStock Create(
            string name,
            MaterialType type,
            string brand,
            string color,
            decimal quantity,
            MaterialUnit unit,
            decimal minThreshold,
            decimal maxThreshold,
            decimal unitPrice,
            string? location = null,
            string? supplier = null)
        {
            return new MaterialStock
            {
                Name = name,
                Type = type,
                Brand = brand,
                Color = color,
                Quantity = quantity,
                Unit = unit,
                MinThreshold = minThreshold,
                MaxThreshold = maxThreshold,
                UnitPrice = unitPrice,
                Location = location,
                Supplier = supplier,
                Currency = "EUR",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void AddStock(decimal quantity, string? note = null)
        {
            if (quantity <= 0)
                throw new ArgumentException("La quantité doit être positive");

            Quantity += quantity;
            LastRestockedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveStock(decimal quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("La quantité doit être positive");

            if (Quantity - quantity < 0)
                throw new InvalidOperationException($"Stock insuffisant. Disponible: {Quantity} {Unit}");

            Quantity -= quantity;
            LastUsedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsLowStock()
        {
            return Quantity <= MinThreshold;
        }

        public bool IsCriticalStock()
        {
            return Quantity <= MinThreshold / 2;
        }

        public bool IsOverStock()
        {
            return MaxThreshold > 0 && Quantity >= MaxThreshold;
        }

        public void UpdateThresholds(decimal minThreshold, decimal maxThreshold)
        {
            MinThreshold = minThreshold;
            MaxThreshold = maxThreshold;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePrice(decimal unitPrice)
        {
            UnitPrice = unitPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
