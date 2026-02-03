using Microsoft.EntityFrameworkCore;
using QrCafe.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Infrastructure.Data
{
    public class QrCafeDbContext : DbContext
    {
        public QrCafeDbContext(DbContextOptions<QrCafeDbContext> options) : base(options) { }

        public DbSet<Restaurant> Restaurants => Set<Restaurant>();
        public DbSet<TableEntity> Tables => Set<TableEntity>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");

            modelBuilder.Entity<Restaurant>(e =>
            {
                e.ToTable("restaurants");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Name).HasColumnName("name");
                e.Property(x => x.Slug).HasColumnName("slug");
                e.Property(x => x.CountryCode).HasColumnName("country_code");
                e.Property(x => x.Currency).HasColumnName("currency");
                e.Property(x => x.TimeZone).HasColumnName("timezone");
                e.Property(x => x.TaxRate).HasColumnName("tax_rate");
                e.Property(x => x.IsActive).HasColumnName("is_active");
                e.Property(x => x.CreatedAt).HasColumnName("created_at");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<TableEntity>(e =>
            {
                e.ToTable("tables");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.RestaurantId).HasColumnName("restaurant_id");
                e.Property(x => x.Number).HasColumnName("number");
                e.Property(x => x.Token).HasColumnName("token");
                e.Property(x => x.IsActive).HasColumnName("is_active");
                e.Property(x => x.CreatedAt).HasColumnName("created_at");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Category>(e =>
            {
                e.ToTable("categories");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.RestaurantId).HasColumnName("restaurant_id");
                e.Property(x => x.Name).HasColumnName("name");
                e.Property(x => x.Sort).HasColumnName("sort");
                e.Property(x => x.CreatedAt).HasColumnName("created_at");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Product>(e =>
            {
                e.ToTable("products");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.RestaurantId).HasColumnName("restaurant_id");
                e.Property(x => x.CategoryId).HasColumnName("category_id");
                e.Property(x => x.Name).HasColumnName("name");
                e.Property(x => x.Description).HasColumnName("description");
                e.Property(x => x.Price).HasColumnName("price");
                e.Property(x => x.IsActive).HasColumnName("is_active");
                e.Property(x => x.IsAvailable).HasColumnName("is_available");
                e.Property(x => x.Sort).HasColumnName("sort");
                e.Property(x => x.CreatedAt).HasColumnName("created_at");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
                e.Property(x => x.ImageUrl).HasColumnName("image_url");
            });

            modelBuilder.Entity<Order>(e =>
            {
                e.ToTable("orders");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.RestaurantId).HasColumnName("restaurant_id");
                e.Property(x => x.OrderType).HasColumnName("order_type").HasConversion<string>();
                e.Property(x => x.TableId).HasColumnName("table_id");
                e.Property(x => x.CustomerName).HasColumnName("customer_name");
                e.Property(x => x.Notes).HasColumnName("notes");
                e.Property(x => x.Status).HasColumnName("status").HasConversion<string>();
                e.Property(x => x.Currency).HasColumnName("currency");
                e.Property(x => x.Subtotal).HasColumnName("subtotal");
                e.Property(x => x.Tax).HasColumnName("tax");
                e.Property(x => x.Total).HasColumnName("total");
                e.Property(x => x.CreatedAt).HasColumnName("created_at");
                e.Property(x => x.PaidAt).HasColumnName("paid_at");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<OrderItem>(e =>
            {
                e.ToTable("order_items");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.OrderId).HasColumnName("order_id");
                e.Property(x => x.ProductId).HasColumnName("product_id");
                e.Property(x => x.ProductNameSnap).HasColumnName("product_name_snap");
                e.Property(x => x.UnitPriceSnap).HasColumnName("unit_price_snap");
                e.Property(x => x.Qty).HasColumnName("qty");
                e.Property(x => x.Notes).HasColumnName("notes");
                e.Property(x => x.LineTotal).HasColumnName("line_total");
                e.Property(x => x.CreatedAt).HasColumnName("created_at");

                // ✅ Relación para que EF respete el orden de inserts
                e.HasOne<Order>()
                 .WithMany()
                 .HasForeignKey(x => x.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Payment>(e =>
            {
                e.ToTable("payments");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.OrderId).HasColumnName("order_id");
                e.Property(x => x.Provider).HasColumnName("provider").HasConversion<string>();
                e.Property(x => x.ProviderRef).HasColumnName("provider_ref");
                e.Property(x => x.Status).HasColumnName("status").HasConversion<string>();
                e.Property(x => x.Amount).HasColumnName("amount");
                e.Property(x => x.Currency).HasColumnName("currency");
                e.Property(x => x.CreatedAt).HasColumnName("created_at");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            });
        }
    }
}
