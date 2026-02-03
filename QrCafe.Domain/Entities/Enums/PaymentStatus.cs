using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Domain.Entities.Enums
{
    public enum PaymentStatus
    {
        PENDING,
        SUCCEEDED,
        FAILED,
        CANCELLED
    }
}
