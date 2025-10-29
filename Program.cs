using Api.Application.Abstractions;
using Api.Domain.Core;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure;
using Api.Infrastructure.Http;
using Api.Infrastructure.Persistence;
using Api.Infrastructure.Services;
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
            // Register placeholder  real InMemory will be added in the factory
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

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IReportingService, ReportingService>();

        builder.Services.AddSingleton<IApiErrorResponseFactory, ApiErrorResponseFactory>();

        builder.Services.AddControllersWithViews(options =>
        {
            options.Filters.Add<ExceptionMappingFilter>();
        })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var factory = context.HttpContext.RequestServices.GetRequiredService<IApiErrorResponseFactory>();
                    return factory.Create(context);
                };
            });
        builder.Services.AddSecurityServices(builder.Configuration, builder.Environment);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SchemaFilter<StronglyTypedIdSchemaFilter>();
        });

        var app = builder.Build();

        app.UseCors("AllowFrontend");

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

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

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
            await SeedForTestingAsync(context);
        }
        else
        {
            await SeedForNormalAsync(context);
        }
    }

    private static async Task SeedForNormalAsync(ApplicationDbContext context)
    {
        if (!context.ProductTypes.Any())
        {
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

        var productTypeNone = new ProductTypeId(context.ProductTypes.First(pt => pt.Name == "None").Id);
        var productType4 = new ProductTypeId(context.ProductTypes.First(pt => pt.Name == "1").Id);
        var productType7 = new ProductTypeId(context.ProductTypes.First(pt => pt.Name == "2").Id);
        var productType8 = new ProductTypeId(context.ProductTypes.First(pt => pt.Name == "2a").Id);
        var productType10 = new ProductTypeId(context.ProductTypes.First(pt => pt.Name == "3").Id);
        var productType11 = new ProductTypeId(context.ProductTypes.First(pt => pt.Name == "3a").Id);
        var productType12 = new ProductTypeId(context.ProductTypes.First(pt => pt.Name == "4").Id);
        var productType19 = new ProductTypeId(context.ProductTypes.First(pt => pt.Name == "8").Id);

        if (!context.Customers.Any())
        {
            var customers = new List<Customer>
            {
                CreateCustomerOrder("Tree", batch.Id, 0m, [
                    OrderDetail.Create(productType4, new Money(4), new DateTime(2025, 10, 24)),
                    OrderDetail.Create(productType8, new Money(8), new DateTime(2025, 10, 25)),
                    OrderDetail.Create(productType4, new Money(4), new DateTime(2025, 10, 26)),
                    OrderDetail.Create(productType4, new Money(4), new DateTime(2025, 10, 27))
                ], 2m),
                CreateCustomerOrder("DC", batch.Id, 27m, [
                    OrderDetail.Create(productType8, new Money(8), new DateTime(2025, 10, 25))
                ], 8m),
                CreateCustomerOrder("MrSherg", batch.Id, 10m, [
                    OrderDetail.Create(productType4, new Money(4), new DateTime(2025, 10, 24)),
                    OrderDetail.Create(productType4, new Money(4), new DateTime(2025, 10, 25)),
                    OrderDetail.Create(productType4, new Money(4), new DateTime(2025, 10, 27))
                ]),
                CreateCustomerOrder("Rossweiler", batch.Id, 0m, [
                    OrderDetail.Create(productType12, new Money(12), new DateTime(2025, 10, 24)),
                    OrderDetail.Create(productType12, new Money(12), new DateTime(2025, 10, 26)),
                    OrderDetail.Create(productType12, new Money(12), new DateTime(2025, 10, 28))
                ]),
                CreateCustomerOrder("SillyBollocks", batch.Id, 0m, [
                    OrderDetail.Create(productType4, new Money(4), new DateTime(2025, 10, 25))
                ]),
                CreateCustomerOrder("Cannon", batch.Id, 0m, [
                    OrderDetail.Create(productType4, new Money(4), new DateTime(2025, 10, 25)),
                    OrderDetail.Create(productType4, new Money(4), new DateTime(2025, 10, 26))
                ]),
                CreateCustomerOrder("Tropical", batch.Id, 49.5m, [
                    OrderDetail.Create(productType8, new Money(8), new DateTime(2025, 10, 23)),
                    OrderDetail.Create(productType10, new Money(10), new DateTime(2025, 10, 26)),
                    OrderDetail.Create(productType8, new Money(8), new DateTime(2025, 10, 26)),
                    OrderDetail.Create(productType8, new Money(8), new DateTime(2025, 10, 27))
                ]),
                CreateCustomerOrder("Jock", batch.Id, 0m, [
                    OrderDetail.Create(productType4, new Money(4), new DateTime(2025, 10, 24))
                ]),
                CreateCustomerOrder("GB", batch.Id, 8m, [
                    OrderDetail.Create(productType8, new Money(8), new DateTime(2025, 10, 25))
                ], 5m),
                CreateCustomerOrder("BoatA", batch.Id, 3m, [
                    OrderDetail.Create(productTypeNone, new Money(5m), new DateTime(2025, 10, 24)),
                    OrderDetail.Create(productType4, new Money(4), new DateTime(2025, 10, 25))
                ]),
                CreateCustomerOrder("Pill", batch.Id, 12m, [
                    OrderDetail.Create(productType4, new Money(4), new DateTime(2025, 10, 24))
                ]),
                CreateCustomerOrder("Sean", batch.Id, 6m, [
                    OrderDetail.Create(productType4, new Money(4), new DateTime(2025, 10, 24))
                ]),
                CreateCustomerOrder("PT", batch.Id, 0m, [
                    OrderDetail.Create(productType4, new Money(4), new DateTime(2025, 10, 28))
                ]),
                CreateCustomerOrder("Linc", batch.Id, 12m, []),
                CreateCustomerOrder("Pullen", batch.Id, 12m, [], 4m),
                CreateCustomerOrder("Syd", batch.Id, 40m, []),
                CreateCustomerOrder("Aussie", batch.Id, 70m, []),
                CreateCustomerOrder("Stu", batch.Id, 54.5m, []),
                CreateCustomerOrder("Landscaper", batch.Id, 12m, []),
                CreateCustomerOrder("Bordeaux", batch.Id, 4m, []),
                CreateCustomerOrder("Aidy", batch.Id, 4m, []),
                //CreateCustomer("Tracey", [8m], [], batch.Id),
                //CreateCustomer("Crystal", [6m], [], batch.Id),
                //CreateCustomer("SamMc", [12m], batch.Id),
                //CreateCustomer("Just", [0m], [], batch.Id)
            };

            var sorted = customers.OrderByDescending(c => c.Balance.Amount);
            var amt = customers.Sum(c => c.Balance.Amount.HasValue ? c.Balance.Amount.Value : 0);

            customers.ForEach(c => context.Customers.Add(c));

            await context.SaveChangesAsync();
        }
    }

    private static Customer CreateCustomerOrder(
        string name,
        BatchId batchId,
        decimal balance,
        List<OrderDetail> orderDetails,
        decimal? paymentValue = null)
    {
        var customer = Customer.Register(new CustomerName(name));

        customer.AddOrder(
            CreateOrderWithDetail(
                new CustomerId(customer.Id),
                batchId,
                balance,
                orderDetails));

        if (paymentValue != null) CreatePayment(customer, paymentValue, DateTime.UtcNow);

        return customer;
    }

    private static ProductTypeId ProductTypeNone => new ProductTypeId(new Guid("82B22333-799F-4322-AA5E-86AB3C551A6E"));

    private static Order CreateOrderWithDetail(
        CustomerId customerId,
        BatchId batchId,
        decimal balance,
        List<OrderDetail> orderDetails)
    {
        var details = new List<OrderDetail> { OrderDetail.Create(ProductTypeNone, new Money(balance), DateTime.MinValue) };
        details.AddRange(orderDetails);

        return Order.Create(
            new CustomerId(customerId),
            batchId,
            details
        );
    }

    private static void CreatePayment(Customer customer, decimal? paymentValue, DateTime? date = null)
    {
        if (date == null) date = DateTime.UtcNow;

        customer.AddPayment(Payment.Create(customer.Id, paymentValue.Value, date));
    }

    private static async Task SeedForTestingAsync(ApplicationDbContext context)
    {
        // Always start with a clean slate in tests (prevents duplicate Test Customer)
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Minimal, predictable dataset for tests
        //var pt50 = ProductType.Create(50m);
        //var pt100 = ProductType.Create(100m);
        //context.ProductTypes.AddRange(pt50, pt100);

        var testCustomer = Customer.Register(new CustomerName("Test Customer"));
        context.Customers.Add(testCustomer);

        await context.SaveChangesAsync();
    }
}

//private static Customer CreateCustomerOrder(
//    string name,
//    decimal[] orderValues,
//    decimal[] paymentValues,
//    BatchId batchId)
//{
//    var customer = Customer.Register(new CustomerName(name));

//    customer.AddOrder(
//        CreateOrderWithDetails(
//            new CustomerId(customer.Id),
//            orderValues,
//            batchId));

//    CreatePayments(customer, paymentValues);

//    return customer;
//}

//private static Customer CreateCustomerOrder(
//    string name,
//    decimal[] orderValues,
//    BatchId batchId)
//{
//    var customer = Customer.Register(new CustomerName(name));

//    customer.AddOrder(
//        CreateOrderWithDetail(
//            new CustomerId(customer.Id),
//            batchId,
//            orderValues,
//            batchId));

//    return customer;
//}

//private static Order CreateOrderWithDetails(
//    CustomerId customerId,
//    decimal[] orderValues,
//    BatchId batchId)
//{
//    var orderDetails = orderValues.Select(v =>
//        new OrderDetail(
//            new ProductTypeId(Guid.NewGuid()),
//            new Money(v),
//            DateTime.UtcNow.AddDays(-orderValues.ToList().IndexOf(v))
//        ))
//        .ToList();

//    return Order.Create(
//        new CustomerId(customerId),
//        batchId,
//        orderDetails
//    );
//}

//private static void CreatePayments(Customer customer, decimal[] paymentValues)
//{
//    var payments = paymentValues.Select(
//                p => Payment.Create(customer.Id, p, DateTime.UtcNow))
//                .ToList();

//    payments.ForEach(customer.AddPayment);
//}

//private static void CreatePayment(Customer customer, decimal paidAmount)
//{
//    customer.AddPayment(Payment.Create(customer.Id, paidAmount));
//}

//private static void CreatePayments(Customer customer, decimal[] paymentValues)
//{
//    var payments = paymentValues.Select(
//                p => Payment.Create(customer.Id, p, DateTime.UtcNow))
//                .ToList();

//    payments.ForEach(customer.AddPayment);
//}