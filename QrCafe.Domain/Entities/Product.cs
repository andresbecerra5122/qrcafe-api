using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QrCafe.Domain.Entities.Enums;

namespace QrCafe.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }

        public Guid RestaurantId { get; set; }
        public Guid? CategoryId { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public decimal Price { get; set; }

        public bool IsActive { get; set; }
        public bool IsAvailable { get; set; }

        public int Sort { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public string ImageUrl { get; set; }
        public PrepStation? PrepStation { get; set; }
    }
}
