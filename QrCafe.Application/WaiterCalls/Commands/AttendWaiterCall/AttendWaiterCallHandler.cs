using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.WaiterCalls.Commands.AttendWaiterCall
{
    public class AttendWaiterCallHandler : IRequestHandler<AttendWaiterCallCommand>
    {
        private readonly QrCafeDbContext _db;
        public AttendWaiterCallHandler(QrCafeDbContext db) => _db = db;

        public async Task Handle(AttendWaiterCallCommand request, CancellationToken ct)
        {
            var call = await _db.WaiterCalls
                .SingleOrDefaultAsync(c => c.Id == request.WaiterCallId, ct)
                ?? throw new KeyNotFoundException("Waiter call not found.");

            if (call.Status == "ATTENDED")
                return;

            call.Status = "ATTENDED";
            call.AttendedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }
}
