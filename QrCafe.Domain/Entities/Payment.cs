using QrCafe.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public PaymentProvider Provider { get; set; }

        public string? ProviderRef { get; set; } // PaymentIntentId, etc.

        public PaymentStatus Status { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = null!; // COP/EUR

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
