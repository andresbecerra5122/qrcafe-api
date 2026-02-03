using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Domain.Entities
{
    public class TableEntity
    {
        public Guid Id { get; set; }

        public Guid RestaurantId { get; set; }
        public int Number { get; set; }

        /// <summary>
        /// Token largo que va embebido en el QR
        /// </summary>
        public string Token { get; set; } = null!;

        public bool IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
