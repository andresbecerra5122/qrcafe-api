using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Domain.Entities
{
    public class Restaurant
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;

        public string CountryCode { get; set; } = null!; // CO / ES
        public string Currency { get; set; } = null!;    // COP / EUR
        public string TimeZone { get; set; } = null!;    // America/Bogota, Europe/Madrid

        /// <summary>
        /// 0..1  (Ej: 0.19 = 19% IVA)
        /// </summary>
        public decimal TaxRate { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
