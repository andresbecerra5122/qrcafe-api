using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Application.Payments.Commands.CreateWompiCheckout
{
    public record CreateWompiCheckoutCommand(Guid OrderId) : IRequest<CreateWompiCheckoutResult>;

}
