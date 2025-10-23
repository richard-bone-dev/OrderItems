using Api.Application.Abstractions;
using Api.Domain.Core;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure;
using Api.Infrastructure.Http;
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

        if (!context.Customers.Any())
        {
            var customers = new List<Customer>
            {
                CreateCustomer("Tree", [8m, 4m, 8m, 4m], [2m], batch.Id),
                CreateCustomer("DC", [9m, 2m, 8m, 4m, 4m], [], batch.Id),
                CreateCustomer("MrSherg", [7m, 4m, 6m], [10m], batch.Id),
                CreateCustomer("Rozweiler", [2m, 7m, 12m, 7m, 4m, 6m, 12m], [], batch.Id),
                CreateCustomer("Kieran", [30m, 2m], [], batch.Id),
                CreateCustomer("Linc", [12m], [], batch.Id),
                CreateCustomer("Pullen", [12m], [], batch.Id),
                CreateCustomer("Saffer", [8m], [], batch.Id),
                CreateCustomer("Sean", [2m, 4m], [], batch.Id),
                CreateCustomer("Wiggy", [4m, 4m], [], batch.Id),
                CreateCustomer("Tall", [4m], [], batch.Id),
                CreateCustomer("JoeQ", [4m], [], batch.Id),
                CreateCustomer("Jock", [4m, 4m], [], batch.Id),
                CreateCustomer("BoatA", [3m], [], batch.Id),
                CreateCustomer("Parsonage", [2m], [], batch.Id),
                CreateCustomer("Tropical", [49.5m], [], batch.Id),
                CreateCustomer("Syd", [40m], [], batch.Id),
                CreateCustomer("Aussie", [71m], [], batch.Id),
                CreateCustomer("Stu", [54.5m], [], batch.Id),
                CreateCustomer("Landscaper", [12m], [], batch.Id),
                CreateCustomer("Pill", [12m], [], batch.Id),
                CreateCustomer("Bordeaux", [4m, 4m], [], batch.Id),
                CreateCustomer("Aidy", [4m], [], batch.Id),
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

    private static Customer CreateCustomer(
        string name,
        decimal[] orderValues,
        decimal[] paymentValues,
        BatchId batchId)
    {
        var customer = Customer.Register(new CustomerName(name));

        var orderDetails = orderValues.Select(v => 
            new OrderDetail(
                new ProductTypeId(Guid.NewGuid()),
                new Money(v),
                DateTime.UtcNow.AddDays(-orderValues.ToList().IndexOf(v))
            ))
            .ToList();

        var order = Order.Create(
            new CustomerId(customer.Id),
            batchId,
            orderDetails
        );

        customer.AddOrder(order);

        var payments = paymentValues.Select(p =>
            Payment.Create(customer.Id, p, DateTime.UtcNow)).ToList();

        payments.ForEach(customer.AddPayment);

        return customer;
    }

    private static async Task SeedForTestingAsync(ApplicationDbContext context)
    {
        // Always start with a clean slate in tests (prevents duplicate Test User)
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