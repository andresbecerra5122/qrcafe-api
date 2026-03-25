using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Ops.Commands.DeleteRestaurantPaymentMethod
{
    public class DeleteRestaurantPaymentMethodHandler : IRequestHandler<DeleteRestaurantPaymentMethodCommand>
    {
        private readonly QrCafeDbContext _db;

        public DeleteRestaurantPaymentMethodHandler(QrCafeDbContext db) => _db = db;

        public async Task Handle(DeleteRestaurantPaymentMethodCommand request, CancellationToken ct)
        {
            var entity = await _db.RestaurantPaymentMethods
                .SingleOrDefaultAsync(m => m.Id == request.MethodId && m.RestaurantId == request.RestaurantId, ct)
                ?? throw new KeyNotFoundException("Payment method not found.");

            if (entity.Code is "CASH" or "CARD")
                throw new InvalidOperationException("Default payment methods cannot be deleted.");

            _db.RestaurantPaymentMethods.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }
}
