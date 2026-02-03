using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Application.Orders.Queries.GetOrderById
{
    public record GetOrderByIdQuery(Guid OrderId) : IRequest<GetOrderByIdResult?>;
}
