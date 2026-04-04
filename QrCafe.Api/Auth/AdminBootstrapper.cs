using Microsoft.EntityFrameworkCore;
using QrCafe.Domain.Entities;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Api.Auth
{
    public class AdminBootstrapper
    {
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher _passwordHasher;

        public AdminBootstrapper(IConfiguration configuration, IPasswordHasher passwordHasher)
        {
            _configuration = configuration;
            _passwordHasher = passwordHasher;
        }

        public async Task RunAsync(IServiceProvider serviceProvider, CancellationToken ct = default)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<QrCafeDbContext>();

            await EnsureStaffTableAsync(db, ct);
            await EnsureDeliverySchemaAsync(db, ct);
            await SeedInitialAdminAsync(db, ct);
            await EnsureSeedSuperAdminAsync(db, ct);
        }

        private static async Task EnsureStaffTableAsync(QrCafeDbContext db, CancellationToken ct)
        {
            const string sql = """
                CREATE TABLE IF NOT EXISTS public.staff_users (
                    id uuid PRIMARY KEY,
                    restaurant_id uuid NOT NULL,
                    full_name text NOT NULL,
                    email text NOT NULL,
                    password_hash text NOT NULL,
                    role text NOT NULL,
                    is_active boolean NOT NULL,
                    created_at timestamptz NOT NULL,
                    updated_at timestamptz NOT NULL,
                    last_login_at timestamptz NULL
                );

                CREATE UNIQUE INDEX IF NOT EXISTS ux_staff_users_restaurant_email
                    ON public.staff_users (restaurant_id, email);
                """;

            await db.Database.ExecuteSqlRawAsync(sql, ct);
        }

        private static async Task EnsureDeliverySchemaAsync(QrCafeDbContext db, CancellationToken ct)
        {
            const string sql = """
                ALTER TABLE IF EXISTS public.restaurants
                    ADD COLUMN IF NOT EXISTS enable_dine_in boolean NOT NULL DEFAULT true,
                    ADD COLUMN IF NOT EXISTS enable_delivery boolean NOT NULL DEFAULT false,
                    ADD COLUMN IF NOT EXISTS enable_delivery_cash boolean NOT NULL DEFAULT true,
                    ADD COLUMN IF NOT EXISTS enable_delivery_card boolean NOT NULL DEFAULT true,
                    ADD COLUMN IF NOT EXISTS enable_pay_at_cashier boolean NOT NULL DEFAULT false,
                    ADD COLUMN IF NOT EXISTS enable_kitchen_bar_split boolean NOT NULL DEFAULT false;

                ALTER TABLE IF EXISTS public.categories
                    ADD COLUMN IF NOT EXISTS prep_station text NOT NULL DEFAULT 'KITCHEN';

                ALTER TABLE IF EXISTS public.products
                    ADD COLUMN IF NOT EXISTS prep_station text NULL;

                ALTER TABLE IF EXISTS public.orders
                    ADD COLUMN IF NOT EXISTS delivery_address text NULL,
                    ADD COLUMN IF NOT EXISTS delivery_reference text NULL,
                    ADD COLUMN IF NOT EXISTS delivery_phone varchar(50) NULL,
                    ADD COLUMN IF NOT EXISTS delivery_fee numeric(12,2) NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS payment_method_label text NULL;

                ALTER TABLE IF EXISTS public.order_items
                    ADD COLUMN IF NOT EXISTS is_done boolean NOT NULL DEFAULT false,
                    ADD COLUMN IF NOT EXISTS prep_station text NOT NULL DEFAULT 'KITCHEN',
                    ADD COLUMN IF NOT EXISTS is_prepared boolean NOT NULL DEFAULT false,
                    ADD COLUMN IF NOT EXISTS is_delivered boolean NOT NULL DEFAULT false;

                CREATE TABLE IF NOT EXISTS public.restaurant_order_counters (
                    restaurant_id uuid PRIMARY KEY REFERENCES public.restaurants(id),
                    last_number bigint NOT NULL DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS public.restaurant_payment_methods (
                    id uuid PRIMARY KEY,
                    restaurant_id uuid NOT NULL REFERENCES public.restaurants(id) ON DELETE CASCADE,
                    code varchar(30) NOT NULL,
                    label varchar(80) NOT NULL,
                    is_active boolean NOT NULL DEFAULT true,
                    sort int NOT NULL DEFAULT 0,
                    created_at timestamptz NOT NULL DEFAULT now(),
                    updated_at timestamptz NOT NULL DEFAULT now()
                );

                CREATE UNIQUE INDEX IF NOT EXISTS ux_restaurant_payment_methods_restaurant_code
                    ON public.restaurant_payment_methods (restaurant_id, code);
                """;

            await db.Database.ExecuteSqlRawAsync(sql, ct);

            const string backfillSql = """
                UPDATE public.order_items
                SET is_prepared = is_done
                WHERE is_prepared IS DISTINCT FROM is_done;
                """;
            await db.Database.ExecuteSqlRawAsync(backfillSql, ct);

            const string seedMethodsSql = """
                INSERT INTO public.restaurant_payment_methods (id, restaurant_id, code, label, is_active, sort, created_at, updated_at)
                SELECT gen_random_uuid(), r.id, 'CASH', 'Efectivo', true, 1, now(), now()
                FROM public.restaurants r
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM public.restaurant_payment_methods rpm
                    WHERE rpm.restaurant_id = r.id AND rpm.code = 'CASH'
                );

                INSERT INTO public.restaurant_payment_methods (id, restaurant_id, code, label, is_active, sort, created_at, updated_at)
                SELECT gen_random_uuid(), r.id, 'CARD', 'Tarjeta', true, 2, now(), now()
                FROM public.restaurants r
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM public.restaurant_payment_methods rpm
                    WHERE rpm.restaurant_id = r.id AND rpm.code = 'CARD'
                );
                """;
            await db.Database.ExecuteSqlRawAsync(seedMethodsSql, ct);
        }

        private async Task SeedInitialAdminAsync(QrCafeDbContext db, CancellationToken ct)
        {
            var restaurantIdRaw = _configuration["BOOTSTRAP_ADMIN_RESTAURANT_ID"];
            var emailRaw = _configuration["BOOTSTRAP_ADMIN_EMAIL"];
            var password = _configuration["BOOTSTRAP_ADMIN_PASSWORD"];
            var fullName = _configuration["BOOTSTRAP_ADMIN_FULL_NAME"] ?? "Initial Admin";
            var roleRaw = _configuration["BOOTSTRAP_ADMIN_ROLE"] ?? StaffRole.Manager.ToString();

            if (string.IsNullOrWhiteSpace(restaurantIdRaw)
                || string.IsNullOrWhiteSpace(emailRaw)
                || string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            if (!Guid.TryParse(restaurantIdRaw, out var restaurantId))
            {
                return;
            }

            var email = emailRaw.Trim().ToLowerInvariant();

            var restaurantExists = await db.Restaurants
                .AsNoTracking()
                .AnyAsync(r => r.Id == restaurantId, ct);

            if (!restaurantExists)
            {
                return;
            }

            var userExists = await db.StaffUsers.AnyAsync(
                u => u.RestaurantId == restaurantId && u.Email == email,
                ct
            );
            if (userExists)
            {
                return;
            }

            if (!Enum.TryParse<StaffRole>(roleRaw, true, out var role))
            {
                role = StaffRole.Manager;
            }

            var now = DateTimeOffset.UtcNow;
            db.StaffUsers.Add(new StaffUser
            {
                Id = Guid.NewGuid(),
                RestaurantId = restaurantId,
                FullName = fullName.Trim(),
                Email = email,
                PasswordHash = _passwordHasher.Hash(password),
                Role = role,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            await db.SaveChangesAsync(ct);
        }

        private async Task EnsureSeedSuperAdminAsync(QrCafeDbContext db, CancellationToken ct)
        {
            var defaultRestaurantId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";
            var restaurantIdRaw = _configuration["SEED_SUPERADMIN_RESTAURANT_ID"] ?? defaultRestaurantId;
            var emailRaw = _configuration["SEED_SUPERADMIN_EMAIL"] ?? "superadmin@qrcafe.local";
            var password = _configuration["SEED_SUPERADMIN_PASSWORD"] ?? "Admin123!";
            var fullName = _configuration["SEED_SUPERADMIN_FULL_NAME"] ?? "Super Admin";

            if (!Guid.TryParse(restaurantIdRaw, out var restaurantId))
            {
                return;
            }

            var email = emailRaw.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            var restaurantExists = await db.Restaurants.AsNoTracking().AnyAsync(r => r.Id == restaurantId, ct);
            if (!restaurantExists)
            {
                return;
            }

            var exists = await db.StaffUsers.AsNoTracking().AnyAsync(
                u => u.RestaurantId == restaurantId && u.Email == email,
                ct
            );
            if (exists)
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;
            db.StaffUsers.Add(new StaffUser
            {
                Id = Guid.NewGuid(),
                RestaurantId = restaurantId,
                FullName = fullName.Trim(),
                Email = email,
                PasswordHash = _passwordHasher.Hash(password),
                Role = StaffRole.SuperAdmin,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });
            await db.SaveChangesAsync(ct);
        }
    }
}
