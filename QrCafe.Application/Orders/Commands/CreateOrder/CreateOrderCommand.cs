using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Application.Orders.Commands.CreateOrder
{
    public record CreateOrderCommand(CreateOrderInput Input) : IRequest<CreateOrderResult>;
}
