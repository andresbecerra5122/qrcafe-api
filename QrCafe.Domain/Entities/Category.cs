using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; set; }

        public Guid RestaurantId { get; set; }

        public string Name { get; set; } = null!;
        public int Sort { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

}
