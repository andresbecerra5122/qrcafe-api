using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Application.Ops.Commands.UpdateOrderStatus
{
    public record UpdateOrderStatusCommand(Guid OrderId, string Status) : IRequest;
}
