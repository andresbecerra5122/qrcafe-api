using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QrCafe.Domain.Entities.Enums;

namespace QrCafe.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; set; }

        public Guid RestaurantId { get; set; }

        public string Name { get; set; } = null!;
        public int Sort { get; set; }
        public PrepStation PrepStation { get; set; } = PrepStation.KITCHEN;

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

}
