using MediatR;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Domain.Entities;
using QrCafe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace QrCafe.Application.Orders.Commands.CreateOrder
{
    public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, CreateOrderResult>
    {
        private readonly QrCafeDbContext _db;
        public CreateOrderHandler(QrCafeDbContext db) => _db = db;

        public async Task<CreateOrderResult> Handle(CreateOrderCommand cmd, CancellationToken ct)
        {
            var req = cmd.Input;

            if (req.Items is null || req.Items.Count == 0)
                throw new ArgumentException("Items are required.");

            if (!Enum.TryParse<OrderType>(req.OrderType, true, out var orderType))
                throw new ArgumentException("Invalid orderType. Use DINE_IN, TAKEAWAY or DELIVERY.");

            var restaurant = await _db.Restaurants
                .SingleOrDefaultAsync(r => r.Id == req.RestaurantId && r.IsActive, ct)
                ?? throw new KeyNotFoundException("Restaurant not found.");

            TableEntity? table = null;

            if (orderType == OrderType.DINE_IN)
            {
                if (!restaurant.EnableDineIn)
                    throw new ArgumentException("DINE_IN is disabled for this restaurant.");

                if (string.IsNullOrWhiteSpace(req.TableToken))
                    throw new ArgumentException("tableToken is required for DINE_IN.");

                table = await _db.Tables.SingleOrDefaultAsync(
                    t => t.RestaurantId == restaurant.Id && t.Token == req.TableToken && t.IsActive, ct);

                if (table is null) throw new ArgumentException("Invalid table token.");
                if (!string.IsNullOrWhiteSpace(req.PaymentMethod))
                    throw new ArgumentException("paymentMethod is only allowed for DELIVERY.");
            }
            else if (orderType == OrderType.TAKEAWAY)
            {
                if (!string.IsNullOrWhiteSpace(req.TableToken))
                    throw new ArgumentException("tableToken must be null for TAKEAWAY.");

                if (!string.IsNullOrWhiteSpace(req.PaymentMethod))
                    throw new ArgumentException("paymentMethod is only allowed for DELIVERY.");
            }
            else if (orderType == OrderType.DELIVERY)
            {
                if (!restaurant.EnableDelivery)
                    throw new ArgumentException("DELIVERY is disabled for this restaurant.");

                if (!string.IsNullOrWhiteSpace(req.TableToken))
                    throw new ArgumentException("tableToken must be null for DELIVERY.");
                if (string.IsNullOrWhiteSpace(req.DeliveryAddress))
                    throw new ArgumentException("deliveryAddress is required for DELIVERY.");
                if (string.IsNullOrWhiteSpace(req.DeliveryPhone))
                    throw new ArgumentException("deliveryPhone is required for DELIVERY.");
                if (string.IsNullOrWhiteSpace(req.PaymentMethod))
                    throw new ArgumentException("paymentMethod is required for DELIVERY.");
                if (!Enum.TryParse<PaymentMethod>(req.PaymentMethod, true, out var deliveryMethod))
                    throw new ArgumentException("Invalid paymentMethod. Use CASH or CARD.");
                if (deliveryMethod == PaymentMethod.CASH && !restaurant.EnableDeliveryCash)
                    throw new ArgumentException("Cash is disabled for delivery.");
                if (deliveryMethod == PaymentMethod.CARD && !restaurant.EnableDeliveryCard)
                    throw new ArgumentException("Card is disabled for delivery.");
            }

            var productIds = req.Items.Select(i => i.ProductId).Distinct().ToList();

            var products = await _db.Products
                .Where(p => p.RestaurantId == restaurant.Id && p.IsActive && productIds.Contains(p.Id))
                .ToListAsync(ct);

            if (products.Count != productIds.Count)
                throw new ArgumentException("Some products are invalid for this restaurant.");

            foreach (var it in req.Items)
            {
                if (it.Qty < 1) throw new ArgumentException("Qty must be >= 1.");
                var p = products.First(x => x.Id == it.ProductId);
                if (!p.IsAvailable) throw new ArgumentException($"Product not available: {p.Name}");
            }

            decimal subtotal = 0;
            var orderItems = new List<OrderItem>();
            var now = DateTimeOffset.UtcNow;
            var categoryStations = await _db.Categories.AsNoTracking()
                .Where(c => c.RestaurantId == restaurant.Id)
                .ToDictionaryAsync(c => c.Id, c => c.PrepStation, ct);

            foreach (var it in req.Items)
            {
                var p = products.First(x => x.Id == it.ProductId);
                var lineTotal = p.Price * it.Qty;
                subtotal += lineTotal;
                var prepStation = QrCafe.Domain.Entities.Enums.PrepStation.KITCHEN;
                if (restaurant.EnableKitchenBarSplit)
                {
                    if (p.PrepStation.HasValue)
                    {
                        prepStation = p.PrepStation.Value;
                    }
                    else if (p.CategoryId.HasValue
                        && categoryStations.TryGetValue(p.CategoryId.Value, out var categoryStation))
                    {
                        prepStation = categoryStation;
                    }
                }

                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = p.Id,
                    ProductNameSnap = p.Name,
                    UnitPriceSnap = p.Price,
                    Qty = it.Qty,
                    Notes = it.Notes,
                    PrepStation = prepStation,
                    IsPrepared = false,
                    IsDelivered = false,
                    IsDone = false,
                    LineTotal = lineTotal,
                    CreatedAt = now
                });
            }

            var tax = Math.Round(subtotal * restaurant.TaxRate, 2);
            var total = subtotal + tax;

            Order? existingOpenTableOrder = null;
            if (orderType == OrderType.DINE_IN && table is not null)
            {
                existingOpenTableOrder = await _db.Orders
                    .Where(o =>
                        o.RestaurantId == restaurant.Id
                        && o.OrderType == OrderType.DINE_IN
                        && o.TableId == table.Id
                        && o.Status != OrderStatus.PAID
                        && o.Status != OrderStatus.CANCELLED
                        && o.Status != OrderStatus.PAYMENT_PENDING)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync(ct);
            }

            if (existingOpenTableOrder is not null)
            {
                foreach (var oi in orderItems) oi.OrderId = existingOpenTableOrder.Id;

                existingOpenTableOrder.Subtotal += subtotal;
                existingOpenTableOrder.Tax = Math.Round(existingOpenTableOrder.Subtotal * restaurant.TaxRate, 2);
                existingOpenTableOrder.Total = existingOpenTableOrder.Subtotal + existingOpenTableOrder.Tax;
                existingOpenTableOrder.UpdatedAt = now;

                if (existingOpenTableOrder.Status is OrderStatus.READY or OrderStatus.DELIVERED)
                {
                    existingOpenTableOrder.Status = OrderStatus.IN_PROGRESS;
                }

                await using var existingTx = await _db.Database.BeginTransactionAsync(ct);
                _db.OrderItems.AddRange(orderItems);
                await _db.SaveChangesAsync(ct);
                await existingTx.CommitAsync(ct);

                return new CreateOrderResult(
                    existingOpenTableOrder.Id,
                    existingOpenTableOrder.Status.ToString(),
                    existingOpenTableOrder.Currency,
                    existingOpenTableOrder.Subtotal,
                    existingOpenTableOrder.Tax,
                    existingOpenTableOrder.Total,
                    existingOpenTableOrder.OrderNumber);
            }

            var numbers = await _db.Database.SqlQueryRaw<long>(@"
                UPDATE public.restaurant_order_counters
                SET last_number = last_number + 1
                WHERE restaurant_id = {0}
                RETURNING last_number;
            ", restaurant.Id).ToListAsync(ct);
            var nextNumber = numbers.Single();

            var order = new Order
            {
                Id = Guid.NewGuid(),
                RestaurantId = restaurant.Id,
                OrderType = orderType,
                TableId = table?.Id,
                CustomerName = string.IsNullOrWhiteSpace(req.CustomerName) ? null : req.CustomerName.Trim(),
                Notes = string.IsNullOrWhiteSpace(req.Notes) ? null : req.Notes.Trim(),
                DeliveryAddress = string.IsNullOrWhiteSpace(req.DeliveryAddress) ? null : req.DeliveryAddress.Trim(),
                DeliveryReference = string.IsNullOrWhiteSpace(req.DeliveryReference) ? null : req.DeliveryReference.Trim(),
                DeliveryPhone = string.IsNullOrWhiteSpace(req.DeliveryPhone) ? null : req.DeliveryPhone.Trim(),
                PaymentMethod = string.IsNullOrWhiteSpace(req.PaymentMethod)
                    ? null
                    : Enum.TryParse<PaymentMethod>(req.PaymentMethod, true, out var method) ? method : null,
                Status = OrderStatus.CREATED,
                Currency = restaurant.Currency,
                Subtotal = subtotal,
                Tax = tax,
                Total = total,
                CreatedAt = now,
                UpdatedAt = now,
                OrderNumber = nextNumber
            };

            foreach (var oi in orderItems) oi.OrderId = order.Id;

            await using var tx = await _db.Database.BeginTransactionAsync(ct);


            // 1) Inserta primero la orden
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);

            // 2) Inserta items después (ya existe el order_id)
            _db.OrderItems.AddRange(orderItems);
            await _db.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);

            return new CreateOrderResult(order.Id, order.Status.ToString(), order.Currency, order.Subtotal, order.Tax, order.Total, order.OrderNumber);
        }
    }
}
