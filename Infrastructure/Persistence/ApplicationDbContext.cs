using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Api.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<ProductType> ProductTypes { get; set; }
    public DbSet<Batch> Batches { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var customerIdConverter = new ValueConverter<CustomerId, Guid>(id => id.Value, value => new CustomerId(value));
        var orderIdConverter = new ValueConverter<OrderId, Guid>(id => id.Value, value => new OrderId(value));
        var paymentIdConverter = new ValueConverter<PaymentId, Guid>(id => id.Value, value => new PaymentId(value));
        var productTypeIdConverter = new ValueConverter<ProductTypeId, Guid>(id => id.Value, value => new ProductTypeId(value));
        var batchIdConverter = new ValueConverter<BatchId, Guid>(id => id.Value, value => new BatchId(value));

        builder.Entity<IdentityUser>();
        builder.Entity<IdentityRole>();
        builder.Entity<IdentityUserRole<string>>();
        builder.Entity<IdentityUserClaim<string>>();
        builder.Entity<IdentityUserLogin<string>>();
        builder.Entity<IdentityRoleClaim<string>>();
        builder.Entity<IdentityUserToken<string>>();

        builder.Entity<Customer>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.Id).HasConversion(customerIdConverter);
            b.Property(u => u.RegisteredAt).IsRequired();

            b.OwnsOne(u => u.Name, name =>
            {
                name.Property(n => n.Value)
                    .HasColumnName("Name")
                    .IsRequired()
                    .HasMaxLength(100);
            });

            b.HasMany(u => u.Payments)
                .WithOne()
                .HasForeignKey(p => p.CustomerId);
        });

        builder.Entity<Batch>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Id)
                .HasConversion(batchIdConverter);

            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.IsActive).IsRequired();

            b.OwnsOne(x => x.Number, n =>
            {
                n.Property(p => p.Value)
                 .HasColumnName("Number")
                 .IsRequired();
            });

            b.OwnsOne(x => x.Stock, stock =>
            {
                stock.Property(s => s.Available)
                     .HasColumnName("AvailableStock")
                     .IsRequired();
            });

            b.HasMany(x => x.Orders)
             .WithOne()
             .HasForeignKey(o => o.BatchId);
        });

        builder.Entity<Order>(b =>
        {
            b.HasKey(o => o.Id);

            b.Property(o => o.Id).HasConversion(orderIdConverter);
            b.Property(o => o.CustomerId).HasConversion(customerIdConverter);
            b.Property(o => o.BatchId).HasConversion(batchIdConverter);

            b.OwnsMany(o => o.OrderDetails, detail =>
            {
                detail.WithOwner().HasForeignKey("OrderId");

                detail.Property<Guid>("Id");
                detail.HasKey("Id");

                detail.Property(d => d.ProductTypeId)
                      .HasConversion(productTypeIdConverter)
                      .HasColumnName("ProductTypeId");

                detail.OwnsOne(d => d.UnitPrice, price =>
                {
                    price.Property(p => p.Amount)
                         .HasColumnName("UnitPrice")
                         .HasPrecision(18, 2);
                });

                detail.Property(d => d.Quantity).HasColumnName("Quantity");
                detail.Property(d => d.PlacedAt).HasColumnName("PlacedAt");
                detail.Property(d => d.DueDate).HasColumnName("DueDate");

                detail.ToTable("OrderDetails");
            });
        });

        builder.Entity<Payment>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).HasConversion(paymentIdConverter);
            b.Property(p => p.CustomerId).HasConversion(customerIdConverter).IsRequired();

            b.OwnsOne(p => p.PaidAmount, money =>
            {
                money.Property(m => m.Amount)
                     .HasColumnName("PaidAmount")
                     .HasPrecision(18, 2);
            });

            b.Property(p => p.PaymentDate).IsRequired();
        });

        builder.Entity<ProductType>(b =>
        {
            b.HasKey(pt => pt.Id);
            b.Property(pt => pt.Id).HasConversion(productTypeIdConverter);
            b.Property(pt => pt.Name).IsRequired().HasMaxLength(100);

            b.OwnsOne(pt => pt.UnitPrice, money =>
            {
                money.Property(m => m.Amount)
                     .HasColumnName("UnitPrice")
                     .HasPrecision(18, 2);
            });
        });
    }
}