using Microsoft.EntityFrameworkCore;

internal class Program_
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddScoped<CreateOrderHandler>();
        builder.Services.AddScoped<CreatePaymentHandler>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // auto-migrate & seed single product
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
            DataSeeder.Seed(db);
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AdHoc Jobs API V1");
            });
        }

        // 1) Create a new order (with editable Total)
        app.MapPost("/api/orders", async (CreateOrderCommand cmd, CreateOrderHandler h) =>
        {
            var id = await h.HandleAsync(cmd);
            return Results.Created($"/api/orders/{id}", new { OrderId = id });
        })
        .WithName("CreateOrder")
        .WithTags("Orders");

        // 2) Record a payment
        app.MapPost("/api/payments", async (CreatePaymentCommand cmd, CreatePaymentHandler h) =>
        {
            var id = await h.HandleAsync(cmd);
            return Results.Created($"/api/payments/{id}", new { PaymentId = id });
        })
        .WithName("CreatePayment")
        .WithTags("Payments");

        // 3) Get a customer's balance
        app.MapGet("/api/customers/{customerName}/balance", async (string customerName, AppDbContext db) =>
        {
            var cust = await db.Customers.FirstOrDefaultAsync(c => c.Name == customerName);
            if (cust is null) return Results.NotFound();

            var totalOrders = await db.Orders
                .Where(o => o.CustomerId == cust.Id)
                .SumAsync(o => o.Total);

            var totalPayments = await db.Payments
                .Where(p => p.CustomerId == cust.Id)
                .SumAsync(p => p.Amount);

            return Results.Ok(new
            {
                CustomerName = customerName,
                TotalOrders = totalOrders,
                TotalPayments = totalPayments,
                Balance = totalOrders - totalPayments
            });
        }).WithName("GetBalance").WithTags("Customers");

        // 4.1 Create Batch
        app.MapPost("/api/batches", async (AppDbContext db) =>
        {
            var batch = new Batch       `;
            db.Batches.Add(batch);
            await db.SaveChangesAsync();
            return Results.Created($"/api/batches/{batch.Id}", batch);
        })
        .WithName("CreateBatch")
        .WithTags("Batches");

        // 4.2 Add Order to Batch
        app.MapPost("/api/batches/{batchId:int}/orders", async (
            int batchId,
            AddOrderCommand cmd,
            AppDbContext db) =>
        {
            var batch = await db.Batches.FindAsync(batchId);
            if (batch is null) return Results.NotFound($"Batch {batchId} not found");

            var order = new Order
            {
                BatchId = batchId,
                CustomerId = cmd.UserId,
                //ProductId = cmd.ProductId,
                Quantity = cmd.Quantity
            };
            db.Orders.Add(order);
            await db.SaveChangesAsync();
            return Results.Created($"/api/orders/{order.Id}", order);
        })
        .WithName("AddOrderToBatch")
        .WithTags("Batches");

        // 4.3 Get Orders in Batch
        app.MapGet("/api/batches/{batchId:int}/orders", async (
            int batchId,
            AppDbContext db) =>
        {
            var orders = await db.Orders
                .Where(o => o.BatchId == batchId)
                .ToListAsync();
            return orders.Any() ? Results.Ok(orders) : Results.NotFound();
        }).WithName("GetOrdersInBatch").WithTags("Batches");

        app.Run();
    }
}


// 5. DTOs / Commands
public record AddOrderCommand(
    int UserId,
    int ProductId,
    int Quantity = 1,
    string? Note = null
);


public record CreateOrderCommand(
    string CustomerName,
    int Quantity,
    decimal Total          // now editable by client
);

public class CreateOrderHandler
{
    private readonly AppDbContext _db;
    private const int _fixedProductId = 1;
    public CreateOrderHandler(AppDbContext db) => _db = db;

    public async Task<int> HandleAsync(CreateOrderCommand cmd)
    {
        var cust = await _db.Customers
                   .FirstOrDefaultAsync(c => c.Name == cmd.CustomerName)
               ?? _db.Customers.Add(new Customer { Name = cmd.CustomerName }).Entity;

        var prod = await _db.Products.FindAsync(_fixedProductId)
                   ?? throw new ArgumentException("Product not found");

        var order = new Order
        {
            Customer = cust,
            Quantity = cmd.Quantity,
            UnitPrice = prod.Price,
            Total = cmd.Total,
            OrderDate = DateTime.UtcNow
        };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        return order.Id;
    }
}

public record CreatePaymentCommand(string CustomerName, decimal Amount);
public class CreatePaymentHandler
{
    private readonly AppDbContext _db;
    public CreatePaymentHandler(AppDbContext db) => _db = db;

    public async Task<int> HandleAsync(CreatePaymentCommand cmd)
    {
        var cust = await _db.Customers
                   .FirstOrDefaultAsync(c => c.Name == cmd.CustomerName)
               ?? _db.Customers.Add(new Customer { Name = cmd.CustomerName }).Entity;

        var pay = new Payment
        {
            Customer = cust,
            Amount = cmd.Amount,
            PaymentDate = DateTime.UtcNow
        };
        _db.Payments.Add(pay);
        await _db.SaveChangesAsync();
        return pay.Id;
    }
}

public static class DataSeeder
{
    private const string Widget = "Widget";

    public static void Seed(AppDbContext db)
    {
        if (!db.Batches.Any())
        {
            db.Batches.Add(new Batch
            {
                Quantity = 35,
                CreatedDate = DateTime.UtcNow
            });

            db.SaveChanges();
        }

        // 1) Seed the single Product
        if (!db.Products.Any())
        {
            db.Products.Add(new Product
            {
                Name = Widget,
                Price = 40m
            });
            db.SaveChanges();
        }

        // 2) Seed customers + one “starting balance” Order each (if no orders exist)
        if (!db.Orders.Any())
        {
            var startingBalances = new Dictionary<string, decimal>
            {
                ["RS"] = 110m,
                ["TQ"] = 370m,
                ["KC"] = 220m,
                ["AR"] = 310m,
                ["JR"] = 40m,
                ["GB"] = 250m,
                ["SS"] = 300m,
                ["DC"] = 140m,
                ["AL"] = 280m,
                ["DP"] = 20m,
                ["WB"] = 40m
            };

            var product = db.Products.First(p => p.Name == Widget);

            foreach (var (code, balance) in startingBalances)
            {
                // find or create customer
                var customer = db.Customers
                    .FirstOrDefault(c => c.Name == code)
                    ?? db.Customers.Add(new Customer { Name = code }).Entity;

                // create one “seed” order with Total = starting balance,
                // and Quantity = 1 as a placeholder
                db.Orders.Add(new Order
                {
                    Customer = customer,
                    Quantity = 1,
                    BatchId = 1,
                    UnitPrice = product.Price,
                    Total = balance,
                    OrderDate = DateTime.UtcNow
                });
            }

            db.SaveChanges();
        }
    }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions opts) : base(opts) { }
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Batch>()
            .HasMany(b => b.Orders)
            .WithOne(o => o.Batch)
            .HasForeignKey(o => o.BatchId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .Property(o => o.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.Total)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Product>()
            .Property(o => o.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payment>()
            .Property(o => o.Amount)
            .HasPrecision(18, 2);
    }
}

public class Batch
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<Order> Orders { get; } = new();
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
}

public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int BatchId { get; set; }
    public Batch Batch { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }   // now editable
    public DateTime OrderDate { get; set; }

    // balance is derived at query time (not stored here)
}

public class Payment
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
}
