using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Api.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<ProductType> ProductTypes { get; set; }
    public DbSet<Batch> Batches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- Converters for StronglyTypedIds ---
        var userIdConverter = new ValueConverter<UserId, Guid>(id => id.Value, value => new UserId(value));
        var orderIdConverter = new ValueConverter<OrderId, Guid>(id => id.Value, value => new OrderId(value));
        var paymentIdConverter = new ValueConverter<PaymentId, Guid>(id => id.Value, value => new PaymentId(value));
        var productTypeIdConverter = new ValueConverter<ProductTypeId, Guid>(id => id.Value, value => new ProductTypeId(value));
        var batchIdConverter = new ValueConverter<BatchId, Guid>(id => id.Value, value => new BatchId(value));

        // --- User entity ---
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.Id).HasConversion(userIdConverter);
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
                .HasForeignKey(p => p.UserId);
        });

        modelBuilder.Entity<Batch>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasConversion(batchIdConverter);
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.IsActive).IsRequired();

            b.OwnsOne(x => x.Number, n =>
            {
                n.Property(p => p.Value)
                 .HasColumnName("Number")
                 .IsRequired();
            });

            b.HasMany(x => x.Orders)
             .WithOne()
             .HasForeignKey(o => o.BatchId);
        });

        modelBuilder.Entity<Order>(b =>
        {
            b.HasKey(o => o.Id);
            b.Property(o => o.Id).HasConversion(orderIdConverter);
            b.Property(o => o.UserId).HasConversion(userIdConverter).IsRequired();

            b.OwnsOne(o => o.OrderDetail, detail =>
            {
                detail.Property(d => d.ProductTypeId)
                .HasConversion(
                id => id.Value,
                value => new ProductTypeId(value))
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
            });
        });

        // --- Payment entity ---
        modelBuilder.Entity<Payment>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).HasConversion(paymentIdConverter);
            b.Property(p => p.UserId).HasConversion(userIdConverter).IsRequired();

            b.OwnsOne(p => p.PaidAmount, money =>
            {
                money.Property(m => m.Amount)
                     .HasColumnName("PaidAmount")
                     .HasPrecision(18, 2);
            });

            b.Property(p => p.PaymentDate).IsRequired();
        });

        // --- ProductType entity ---
        modelBuilder.Entity<ProductType>(b =>
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