using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Domain.Entities
{
    public class OrderItem
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public Guid ProductId { get; set; }

        // Snapshots para evitar problemas si cambian nombre/precio después
        public string ProductNameSnap { get; set; } = null!;
        public decimal UnitPriceSnap { get; set; }

        public int Qty { get; set; }

        public string? Notes { get; set; }

        public decimal LineTotal { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
