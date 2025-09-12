using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Api.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<ProductType> ProductTypes { get; set; }
    public DbSet<Batch> Batches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- Converters for StronglyTypedIds ---
        var userIdConverter = new ValueConverter<UserId, Guid>(
            id => id.Value,
            value => new UserId(value));

        var orderIdConverter = new ValueConverter<OrderId, Guid>(
            id => id.Value,
            value => new OrderId(value));

        var paymentIdConverter = new ValueConverter<PaymentId, Guid>(
            id => id.Value,
            value => new PaymentId(value));

        var productTypeIdConverter = new ValueConverter<ProductTypeId, Guid>(
            id => id.Value,
            value => new ProductTypeId(value));

        var batchIdConverter = new ValueConverter<BatchId, Guid>(
            id => id.Value,
            value => new BatchId(value));

        var batchNumberConverter = new ValueConverter<BatchNumber, int>(
            id => id.Value,
            value => new BatchNumber(value));

        // --- User entity ---
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.Id).HasConversion(userIdConverter);
            b.Property(u => u.Name).IsRequired().HasMaxLength(100);
            b.Property(u => u.Preferred);
            b.HasMany(u => u.Orders).WithOne().HasForeignKey(o => o.UserId);
            b.HasMany(u => u.Payments).WithOne().HasForeignKey(p => p.UserId);
        });

        modelBuilder.Entity<ProductType>(b =>
        {
            b.HasKey(pt => pt.Id);
            b.Property(pt => pt.Id).HasConversion(productTypeIdConverter);
            b.Property(pt => pt.UnitPrice).HasPrecision(18, 2);
        });

        // --- Order entity ---
        modelBuilder.Entity<Order>(b =>
        {
            b.HasKey(o => o.Id);
            b.Property(o => o.Id).HasConversion(orderIdConverter);
            b.Property(o => o.UserId).HasConversion(userIdConverter).IsRequired();
            b.Property(o => o.ProductTypeId).HasConversion(productTypeIdConverter).IsRequired();
            b.Property(o => o.BatchNumber).HasConversion(batchNumberConverter).HasColumnName("BatchNumber");

            b.HasOne<ProductType>()
                .WithMany()
                .HasForeignKey(o => o.ProductTypeId);

            b.OwnsOne(o => o.OrderDetail, detail =>
            {
                detail.OwnsOne(d => d.Total, money =>
                {
                    money.Property(m => m.Amount)
                         .HasColumnName("Amount")
                         .HasPrecision(18, 2);
                });

                detail.Property(d => d.OrderDate).HasColumnName("OrderDate");
                detail.Property(d => d.DueDate).HasColumnName("DueDate");
            });
        });

        modelBuilder.Entity<Payment>(builder =>
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasConversion(paymentIdConverter);

            builder.Property(p => p.UserId)
                .HasConversion(userIdConverter);

            builder.OwnsOne(p => p.PaidAmount, money =>
            {
                money.Property(m => m.Amount).HasColumnName("PaidAmount").HasPrecision(18, 2);
            });

            builder.OwnsOne(p => p.RemainingAmount, money =>
            {
                money.Property(m => m.Amount).HasColumnName("RemainingAmount").HasPrecision(18, 2);
            });

            builder.Property(p => p.PaymentDate).IsRequired();
        });

        modelBuilder.Entity<Batch>(builder =>
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .HasConversion(batchIdConverter);

            builder.Property(b => b.BatchNumber)
                .HasConversion(batchNumberConverter)
                .HasColumnName("BatchNumber");

            builder.Property(b => b.CreatedDate).IsRequired();
        });
    }
}
