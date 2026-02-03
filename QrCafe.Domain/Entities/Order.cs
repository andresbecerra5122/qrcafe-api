using QrCafe.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }

        public Guid RestaurantId { get; set; }

        public OrderType OrderType { get; set; }

        public Guid? TableId { get; set; } // null si TAKEAWAY

        public string? CustomerName { get; set; }
        public string? Notes { get; set; }

        public OrderStatus Status { get; set; }

        public string Currency { get; set; } = null!; // COP/EUR

        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? PaidAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
