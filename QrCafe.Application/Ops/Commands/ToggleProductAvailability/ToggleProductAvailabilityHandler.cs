using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Ops.Commands.ToggleProductAvailability
{
    public class ToggleProductAvailabilityHandler : IRequestHandler<ToggleProductAvailabilityCommand>
    {
        private readonly QrCafeDbContext _db;
        public ToggleProductAvailabilityHandler(QrCafeDbContext db) => _db = db;

        public async Task Handle(ToggleProductAvailabilityCommand request, CancellationToken ct)
        {
            var product = await _db.Products
                .SingleOrDefaultAsync(p => p.Id == request.ProductId, ct)
                ?? throw new KeyNotFoundException("Product not found.");

            product.IsAvailable = request.IsAvailable;
            product.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }
}
