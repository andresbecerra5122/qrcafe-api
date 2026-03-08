using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Domain.Entities.Enums
{
    public enum OrderStatus
    {
        CREATED,
        PAYMENT_PENDING,
        PAID,
        IN_PROGRESS,
        READY,
        OUT_FOR_DELIVERY,
        DELIVERED,
        CANCELLED
    }
}
