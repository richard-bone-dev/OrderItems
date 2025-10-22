using Api.Application.Abstractions;
using Api.Domain.Core;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure;
using Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api;

public class Program
{
    private static async Task Main(string[] args)
    {
        // --- API / DI Setup ---
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend",
                policy =>
                {
                    policy.WithOrigins(
                            "https://localhost:44362"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

        if (!builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        }
        else
        {
            // Register placeholder — real InMemory will be added in the factory
            //builder.Services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseInMemoryDatabase("Placeholder"));
        }

        var applicationAssembly = typeof(ICommandHandlerAsync<,>).Assembly;

        builder.Services.Scan(scan => scan
            .FromAssemblies(applicationAssembly)
            .AddClasses(c => c.AssignableTo(typeof(ICommandHandlerAsync<,>)))
                .AsSelfWithInterfaces()
                .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IQueryHandlerAsync<,>)))
                .AsSelfWithInterfaces()
                .WithScopedLifetime()
        );

        builder.Services.Scan(scan => scan
            .FromAssemblies(typeof(ICustomerRepository).Assembly, typeof(ApplicationDbContext).Assembly)
                .AddClasses(c => c.InNamespaces("Api.Infrastructure.Repositories"))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
        );

        builder.Services.AddControllersWithViews();
        builder.Services.AddSecurityServices(builder.Configuration, builder.Environment);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SchemaFilter<StronglyTypedIdSchemaFilter>();
        });

        var app = builder.Build();

        if (!app.Environment.IsEnvironment("Testing"))
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (db.Database.IsRelational())
                {
                    db.Database.Migrate();
                    await DataSeeder.SeedAsync(db, app.Environment.EnvironmentName);
                }
            }
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseSecurityPipeline();

        app.UseStaticFiles();
        app.UseRouting();
        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.UseCors("AllowFrontend");

        app.MapControllers();
        app.MapDefaultControllerRoute();
        app.Run();
    }
}

public class StronglyTypedIdSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var baseType = context.Type.BaseType;

        if (baseType?.IsGenericType == true)
        {
            var genericDef = baseType.GetGenericTypeDefinition();

            if (genericDef == typeof(StronglyTypedId<>))
            {
                schema.Type = "string";
                schema.Format = "uuid";
                schema.Example = new Microsoft.OpenApi.Any.OpenApiString(Guid.NewGuid().ToString());
            }
            else if (genericDef == typeof(StronglyTypedIntId<>))
            {
                schema.Type = "integer";
                schema.Format = "int32";
                schema.Example = new Microsoft.OpenApi.Any.OpenApiInteger(new Random().Next(1, 100));
            }
        }
    }
}

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, string environment)
    {
        if (environment == "Testing")
        {
            SeedForTestingAsync(context);
        }
        else
        {
            SeedForNormalAsync(context);
        }
    }

    private static async void SeedForNormalAsync(ApplicationDbContext context)
    {
        // Safe, idempotent seeding for Dev/Prod (runs after Migrate)
        if (!context.ProductTypes.Any())
        {
            //Array.Empty<ProductType>();
            var products = new[] {
                ProductType.Create("None", new Money(null)),
                ProductType.Create("1", new Money(40m)),
                ProductType.Create("2", new Money(70m)),
                ProductType.Create("2a", new Money(80m)),
                ProductType.Create("3", new Money(100m)),
                ProductType.Create("3a", new Money(110m)),
                ProductType.Create("4", new Money(120m)),
                ProductType.Create("8", new Money(190m))
            };
            context.ProductTypes.AddRange(products);
            context.SaveChanges();
        }

        if (!context.Batches.Any())
        {
            var initialBatch = Batch.Create(new BatchNumber(1));
            context.Batches.Add(initialBatch);
            context.SaveChanges();
        }

        var productTypeId = context.ProductTypes.OrderBy(pt => pt.UnitPrice.Amount).First().Id;
        var batch = context.Batches.OrderByDescending(b => b.CreatedAt).FirstOrDefault();

        if (batch == null)
        {
            batch = Batch.Create(new BatchNumber(1));
            context.Batches.Add(batch);
            context.SaveChanges();
        }

        if (!context.Customers.Any(u => u.Name.Value == "None"))
            context.Customers.Add(Customer.Register(new CustomerName("None")));

        if (!context.Customers.Any(u => u.Name.Value == "Admin"))
            context.Customers.Add(Customer.Register(new CustomerName("Admin")));

        if (!context.Customers.Any())
        {
            var customers = new List<Customer>
            {
                CreateCustomer("Tree", [8m, 4m], batch.Id),
                CreateCustomer("DC", [9m, 2m, 8m, 4m, 4m], batch.Id),
                CreateCustomer("MrSherg", [7m, 4m, 6m], batch.Id),
                CreateCustomer("Rozweiler", new [] { 2m, 7m, 12m, 7m, 4m, 6m }, batch.Id),
                CreateCustomer("Kieran", new [] { 17m }, batch.Id),
                CreateCustomer("Linc", new [] { 12m }, batch.Id),
                CreateCustomer("Pullen", new [] { 12m }, batch.Id),
                CreateCustomer("Saffer", new [] { 8m }, batch.Id),
                CreateCustomer("Sean", new [] { 2m, 4m }, batch.Id),
                CreateCustomer("Wiggy", new [] { 4m }, batch.Id),
                CreateCustomer("Tall", new [] { 4m }, batch.Id),
                CreateCustomer("JoeQ", new [] { 4m }, batch.Id),
                CreateCustomer("Just", new [] { 4m }, batch.Id),
                CreateCustomer("Jock", new [] { 4m }, batch.Id),
                CreateCustomer("BoatA", new [] { 3m }, batch.Id),
                CreateCustomer("Parsonage", new [] { 2m }, batch.Id),
                CreateCustomer("Tropical", new [] { 86m, 3.5m }, batch.Id),
                CreateCustomer("Syd", new [] { 40m }, batch.Id),
                CreateCustomer("Aussie", new [] { 69m }, batch.Id),
                CreateCustomer("Stu", new [] { 54.5m }, batch.Id),
                CreateCustomer("Landscaper", new [] { 12m }, batch.Id),
                CreateCustomer("Pill", new [] { 12m }, batch.Id),
                CreateCustomer("Tracey", new [] { 8m }, batch.Id),
                CreateCustomer("Crystal", new [] { 6m }, batch.Id),
                CreateCustomer("Bordeaux", new [] { 4m }, batch.Id),
                CreateCustomer("Aidy", new [] { 4m }, batch.Id),
                CreateCustomer("SamMc", new [] { 12m }, batch.Id)
            };

            // Create Orders for each Customer
            foreach (var customer in customers)
            {
                // Create multiple order details per customer
                var orderDetails = customer.Orders.SelectMany(o => o.OrderDetails).ToList();

                // Example batch creation logic
                var order = Order.Create(
                    new CustomerId(customer.Id.Value),
                    new BatchId(batch.Id.Value),
                    orderDetails
                );

                customer.AddOrder(order);
                context.Customers.Add(customer);
            }

            context.SaveChanges();
        }
    }

    private static Customer CreateCustomer(string name, decimal[] values, BatchId batchId)
    {
        var customer = Customer.Register(new CustomerName(name));

        var orderDetails = values.Select(v => 
            new OrderDetail(
                new ProductTypeId(Guid.NewGuid()),
                new Money(v),
                DateTime.UtcNow.AddDays(-values.ToList().IndexOf(v))
            ))
            .ToList();

        // Use the existing batchId here
        var order = Order.Create(
            new CustomerId(customer.Id),
            batchId,
            orderDetails
        );

        customer.AddOrder(order);
        return customer;
    }

    private static async void SeedForTestingAsync(ApplicationDbContext context)
    {
        // Always start with a clean slate in tests (prevents duplicate “Test User”)
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Minimal, predictable dataset for tests
        //var pt50 = ProductType.Create(50m);
        //var pt100 = ProductType.Create(100m);
        //context.ProductTypes.AddRange(pt50, pt100);

        var testUser = Customer.Register(new CustomerName("Test User"));
        context.Customers.Add(testUser);

        await context.SaveChangesAsync();
    }
}

//public static class DataSeeder
//{
//    public static void Seed(ApplicationDbContext context)
//    {
//        if (!context.ProductTypes.Any())
//        {
//            var products = new List<ProductType>
//            {
//                ProductType.Create(0m),
//                ProductType.Create(40m),
//                ProductType.Create(70m),
//                ProductType.Create(80m),
//                ProductType.Create(100m),
//                ProductType.Create(120m),
//                ProductType.Create(190m)
//            };

//            context.ProductTypes.AddRange(products);
//            context.SaveChanges();
//        }

//        var productTypeId = context.ProductTypes.OrderBy(pt => pt.UnitPrice).First().Id;

//        // only seed once
//        if (!context.Users.Any())
//        {

//            var starting = new Dictionary<string, decimal>
//            {
//                ["None"] = 0m,
//                ["Admin"] = 0m,
//                ["AL"] = 120m,
//                ["TQ"] = 54m,
//                ["SS"] = 54.5m,
//                ["AR"] = 38m + 4m,
//                ["DC"] = 20m - 10m + 8m,
//                ["GB"] = 19m - 13m + 8m,
//                ["SC"] = 15m,
//                ["TC"] = 15m,
//                ["MK"] = 12m,
//                ["KC"] = 12m,
//                ["WB"] = 12m,
//                ["PT"] = 8m,
//                ["AD"] = 8m,
//                ["MD"] = 8m,
//                ["RS"] = 6m + 12m,
//                ["KR"] = 6m,
//                ["SM"] = 4m + 4m,
//                ["MP"] = 4m - 4m,
//                ["JR"] = 4m,
//                ["LI"] = 4m,
//                ["TU"] = 4m,
//                ["AM"] = 4m,
//                ["SI"] = 4m,
//                ["HA"] = 3m,
//                ["AN"] = 2m
//            };

//            var s = starting.Values.Sum();

//            var sorted = starting.OrderByDescending(c => c.Value);

//            foreach (var (code, balance) in sorted)
//            {
//                var customer = User.Register(code);

//                if (balance > 0)
//                {
//                    customer.PlaceOrder(
//                        new CustomerId(customer.Id),
//                        new BatchNumber(1),
//                        productTypeId,
//                        new OrderDetail(new Money(balance * 10), null, null)
//                    );
//                }

//                context.Users.Add(customer);
//            }
//        }

//        context.SaveChanges();
//    }
//}

//public static class DataSeeder1
//{
//    public static async Task SeedAsync(ApplicationDbContext context)
//    {
//        if (await context.Customers.AnyAsync())
//            return;

//        var batch = Batch.Create(new BatchNumber(1));
//        var productTypeId = new ProductTypeId(Guid.NewGuid());

//        // Define your customers with their full order structures
//        var customers = new List<Customer>
//            {
//                CreateCustomer("Tree", [8m, 4m]),
//                CreateCustomer("DC", [9m, 2m, 8m, 4m, 4m]),
//                CreateCustomer("MrSherg", [7m, 4m, 6m]),
//                CreateCustomer("Rozweiler", new [] { 2m, 7m, 12m, 7m, 4m, 6m }),
//                CreateCustomer("Kieran", new [] { 17m }),
//                CreateCustomer("Linc", new [] { 12m }),
//                CreateCustomer("Pullen", new [] { 12m }),
//                CreateCustomer("Saffer", new [] { 8m }),
//                CreateCustomer("Sean", new [] { 2m, 4m }),
//                CreateCustomer("Wiggy", new [] { 4m }),
//                CreateCustomer("Tall", new [] { 4m }),
//                CreateCustomer("JoeQ", new [] { 4m }),
//                CreateCustomer("Just", new [] { 4m }),
//                CreateCustomer("Jock", new [] { 4m }),
//                CreateCustomer("BoatA", new [] { 3m }),
//                CreateCustomer("Parsonage", new [] { 2m }),
//                CreateCustomer("Tropical", new [] { 86m, 3.5m }),
//                CreateCustomer("Syd", new [] { 40m }),
//                CreateCustomer("Aussie", new [] { 69m }),
//                CreateCustomer("Stu", new [] { 54.5m }),
//                CreateCustomer("Landscaper", new [] { 12m }),
//                CreateCustomer("Pill", new [] { 12m }),
//                CreateCustomer("Tracey", new [] { 8m }),
//                CreateCustomer("Crystal", new [] { 6m }),
//                CreateCustomer("Bordeaux", new [] { 4m }),
//                CreateCustomer("Aidy", new [] { 4m }),
//                CreateCustomer("SamMc", new [] { 12m })
//            };

//        // Create Orders for each Customer
//        foreach (var customer in customers)
//        {
//            // Create multiple order details per customer
//            var orderDetails = customer.Orders.SelectMany(o => o.OrderDetails).ToList();

//            // Example batch creation logic
//            var order = Order.Create(
//                new CustomerId(customer.Id),
//                batch.Id,
//                orderDetails
//            );

//            customer.AddOrder(order);
//            context.Customers.Add(customer);
//        }

//        await context.SaveChangesAsync();
//    }

//    private static Customer CreateCustomer(string name, decimal[] values)
//    {
//        var customer = Customer.Register(new UserName(name));

//        // Build a single order with multiple details
//        var orderDetails = values
//            .Select(v => new OrderDetail(
//                new ProductTypeId(Guid.NewGuid()),
//                new Money(v),
//                DateTime.UtcNow.AddDays(-values.ToList().IndexOf(v))
//            ))
//            .ToList();

//        var order = Order.Create(
//            new CustomerId(customer.Id),
//            new BatchId(Guid.NewGuid()),
//            orderDetails
//        );

//        customer.AddOrder(order);
//        return customer;
//    }
//}

