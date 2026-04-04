using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Ops.Commands.ReassignOrderToTable
{
    public class ReassignOrderToTableHandler : IRequestHandler<ReassignOrderToTableCommand>
    {
        private readonly QrCafeDbContext _db;

        public ReassignOrderToTableHandler(QrCafeDbContext db) => _db = db;

        public async Task Handle(ReassignOrderToTableCommand request, CancellationToken ct)
        {
            var restaurant = await _db.Restaurants
                .SingleOrDefaultAsync(r => r.Id == request.RestaurantId && r.IsActive, ct)
                ?? throw new KeyNotFoundException("Restaurante no encontrado.");

            if (!restaurant.EnableTableReassignment)
                throw new InvalidOperationException("El cambio de mesa está desactivado para este restaurante.");

            if (!restaurant.EnableDineIn)
                throw new InvalidOperationException("El servicio en mesa está desactivado para este restaurante.");

            var order = await _db.Orders
                .SingleOrDefaultAsync(o => o.Id == request.OrderId && o.RestaurantId == request.RestaurantId, ct)
                ?? throw new KeyNotFoundException("Pedido no encontrado.");

            if (order.OrderType != OrderType.DINE_IN)
                throw new InvalidOperationException("Solo se pueden mover pedidos en mesa.");

            if (order.TableId is null)
                throw new InvalidOperationException("El pedido no tiene mesa asignada.");

            if (order.Status == OrderStatus.PAID || order.Status == OrderStatus.CANCELLED)
                throw new InvalidOperationException("No se puede mover un pedido ya pagado o cancelado.");

            var targetTable = await _db.Tables
                .SingleOrDefaultAsync(
                    t => t.RestaurantId == request.RestaurantId
                         && t.Number == request.TargetTableNumber
                         && t.IsActive,
                    ct)
                ?? throw new ArgumentException("La mesa destino no existe o está inactiva.");

            if (order.TableId == targetTable.Id)
                return;

            var blocking = await _db.Orders.AnyAsync(
                o => o.RestaurantId == request.RestaurantId
                     && o.TableId == targetTable.Id
                     && o.Id != order.Id
                     && o.Status != OrderStatus.PAID
                     && o.Status != OrderStatus.CANCELLED,
                ct);

            if (blocking)
                throw new InvalidOperationException("La mesa destino ya tiene un pedido abierto.");

            order.TableId = targetTable.Id;
            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);
        }
    }
}
