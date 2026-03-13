using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Ops.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand>
    {
        private readonly QrCafeDbContext _db;
        public UpdateOrderStatusHandler(QrCafeDbContext db) => _db = db;

        public async Task Handle(UpdateOrderStatusCommand request, CancellationToken ct)
        {
            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
                throw new ArgumentException("Invalid status.");

            var order = await _db.Orders.SingleOrDefaultAsync(o => o.Id == request.OrderId, ct);
            if (order is null) throw new KeyNotFoundException("Order not found.");

            if (newStatus == OrderStatus.READY)
            {
                var pendingItems = await _db.OrderItems
                    .Where(i => i.OrderId == order.Id && !i.IsDone)
                    .ToListAsync(ct);

                foreach (var item in pendingItems)
                {
                    item.IsDone = true;
                }
            }

            order.Status = newStatus;
            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);
        }
    }
}
