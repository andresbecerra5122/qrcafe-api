using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Domain.Entities;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.WaiterCalls.Commands.CreateWaiterCall
{
    public class CreateWaiterCallHandler : IRequestHandler<CreateWaiterCallCommand, CreateWaiterCallResult>
    {
        private readonly QrCafeDbContext _db;
        public CreateWaiterCallHandler(QrCafeDbContext db) => _db = db;

        public async Task<CreateWaiterCallResult> Handle(CreateWaiterCallCommand request, CancellationToken ct)
        {
            var restaurant = await _db.Restaurants
                .AsNoTracking()
                .SingleOrDefaultAsync(r => r.Id == request.RestaurantId, ct)
                ?? throw new KeyNotFoundException("Restaurant not found.");

            int? tableNumber = null;
            Guid? tableId = null;

            if (!string.IsNullOrWhiteSpace(request.TableToken))
            {
                var table = await _db.Tables
                    .AsNoTracking()
                    .SingleOrDefaultAsync(t => t.RestaurantId == request.RestaurantId && t.Token == request.TableToken, ct);

                if (table != null)
                {
                    tableId = table.Id;
                    tableNumber = table.Number;
                }
            }

            var call = new WaiterCall
            {
                Id = Guid.NewGuid(),
                RestaurantId = request.RestaurantId,
                TableId = tableId,
                TableNumber = tableNumber,
                Status = "PENDING",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.WaiterCalls.Add(call);
            await _db.SaveChangesAsync(ct);

            return new CreateWaiterCallResult(call.Id);
        }
    }
}
