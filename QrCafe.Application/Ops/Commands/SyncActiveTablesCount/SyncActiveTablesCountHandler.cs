using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Domain.Entities;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Ops.Commands.SyncActiveTablesCount
{
    public class SyncActiveTablesCountHandler : IRequestHandler<SyncActiveTablesCountCommand, int>
    {
        private const int MaxTables = 200;
        private readonly QrCafeDbContext _db;

        public SyncActiveTablesCountHandler(QrCafeDbContext db) => _db = db;

        public async Task<int> Handle(SyncActiveTablesCountCommand request, CancellationToken ct)
        {
            if (request.ActiveCount < 0 || request.ActiveCount > MaxTables)
            {
                throw new ArgumentException($"activeCount must be between 0 and {MaxTables}.");
            }

            var now = DateTimeOffset.UtcNow;
            var tables = await _db.Tables
                .Where(t => t.RestaurantId == request.RestaurantId)
                .OrderBy(t => t.Number)
                .ToListAsync(ct);

            var activeTables = tables.Where(t => t.IsActive).OrderBy(t => t.Number).ToList();
            var currentActiveCount = activeTables.Count;

            if (currentActiveCount == request.ActiveCount)
            {
                return currentActiveCount;
            }

            if (request.ActiveCount > currentActiveCount)
            {
                var toActivateCount = request.ActiveCount - currentActiveCount;
                var inactiveTables = tables.Where(t => !t.IsActive).OrderBy(t => t.Number).ToList();

                foreach (var table in inactiveTables)
                {
                    if (toActivateCount == 0) break;
                    table.IsActive = true;
                    table.UpdatedAt = now;
                    toActivateCount--;
                }

                if (toActivateCount > 0)
                {
                    var maxNumber = tables.Count == 0 ? 0 : tables.Max(t => t.Number);
                    for (var i = 1; i <= toActivateCount; i++)
                    {
                        _db.Tables.Add(new TableEntity
                        {
                            Id = Guid.NewGuid(),
                            RestaurantId = request.RestaurantId,
                            Number = maxNumber + i,
                            Token = $"mesa-{maxNumber + i}-{Guid.NewGuid():N}",
                            IsActive = true,
                            CreatedAt = now,
                            UpdatedAt = now
                        });
                    }
                }

                await _db.SaveChangesAsync(ct);
                return request.ActiveCount;
            }

            var toDeactivateCount = currentActiveCount - request.ActiveCount;
            var activeTableIds = activeTables.Select(t => t.Id).ToList();
            var blockedTableIds = await _db.Orders.AsNoTracking()
                .Where(o => o.TableId != null
                    && activeTableIds.Contains(o.TableId.Value)
                    && o.Status != OrderStatus.PAID
                    && o.Status != OrderStatus.CANCELLED)
                .Select(o => o.TableId!.Value)
                .Distinct()
                .ToListAsync(ct);
            var blockedSet = blockedTableIds.ToHashSet();

            var candidates = activeTables
                .OrderByDescending(t => t.Number)
                .Where(t => !blockedSet.Contains(t.Id))
                .ToList();

            if (candidates.Count < toDeactivateCount)
            {
                throw new ArgumentException("No se puede reducir ese numero de mesas porque hay pedidos activos en mesas que deberian desactivarse.");
            }

            for (var i = 0; i < toDeactivateCount; i++)
            {
                candidates[i].IsActive = false;
                candidates[i].UpdatedAt = now;
            }

            await _db.SaveChangesAsync(ct);
            return request.ActiveCount;
        }
    }
}
