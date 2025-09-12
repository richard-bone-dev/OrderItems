using Api.Application.Interfaces;
using Api.Application.Services;
using Api.Domain.Core;
using Api.Domain.Entities;
using Api.Domain.Events;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api;

public class Program
{
    private static void Main(string[] args)
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

        // Repositories & Services
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        builder.Services.AddScoped<IOrderPaymentService, OrderPaymentService>();

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
        builder.Services.AddScoped<IProductTypeRepository, ProductTypeRepository>();
        builder.Services.AddScoped<IOrderPaymentService, OrderPaymentService>();

        builder.Services.AddSingleton<IBatchAssignmentService, BatchAssignmentService>();

        // Controllers
        builder.Services.AddControllers();
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
                    //DataSeeder.Seed(db, app.Environment.EnvironmentName);
                }
            }
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.UseCors("AllowFrontend");

        app.MapControllers();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
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
    public static void Seed(ApplicationDbContext context, string environment)
    {
        if (environment == "Testing")
        {
            SeedForTesting(context);
        }
        else
        {
            SeedForNormal(context);
        }
    }

    private static void SeedForNormal(ApplicationDbContext context)
    {
        // Safe, idempotent seeding for Dev/Prod (runs after Migrate)
        if (!context.ProductTypes.Any())
        {
            var products = new[]
            {
                ProductType.Create(0m),
                ProductType.Create(40m),
                ProductType.Create(70m),
                ProductType.Create(80m),
                ProductType.Create(100m),
                ProductType.Create(120m),
                ProductType.Create(190m)
            };
            context.ProductTypes.AddRange(products);
            context.SaveChanges();
        }

        var productTypeId = context.ProductTypes.OrderBy(pt => pt.UnitPrice).First().Id;

        if (!context.Users.Any(u => u.Name == "None"))
            context.Users.Add(User.Register("None"));

        if (!context.Users.Any(u => u.Name == "Admin"))
            context.Users.Add(User.Register("Admin"));

        if (!context.Users.Any())
        {
            var starting = new Dictionary<string, decimal>
            {
                ["None"] = 0m,
                ["Admin"] = 0m,
                ["AL"] = 120m + 10m + 7m - 30m,
                ["TQ"] = 54m,
                ["SS"] = 54.5m - 4m,
                ["AR"] = 38m + 4m + 4m + 4m,
                ["DC"] = 20m - 10m + 8m,
                ["GB"] = 19m - 13m + 8m,
                ["SC"] = 15m,
                ["TC"] = 15m,
                ["MK"] = 12m,
                ["KC"] = 12m,
                ["WB"] = 12m,
                ["PT"] = 8m - 8m + 4m,
                ["AD"] = 8m - 8m + 4m,
                ["MD"] = 8m - 8m + 4m + 4m,
                ["RS"] = 12m + 4m + 12m - 25m + 12m + 12m,
                ["KR"] = 6m,
                ["SM"] = 4m + 4m,
                ["MP"] = 4m - 4m + 4m,
                ["DK"] = 4m + 4m - 8m + 19m - 20m,
                ["JR"] = 4m,
                ["LI"] = 4m,
                ["TU"] = 4m,
                ["AM"] = 4m,
                ["SI"] = 4m,
                ["HA"] = 3m,
                ["AN"] = 2m + 4m
            };

            var sorted = starting.OrderByDescending(c => c.Value);

            foreach (var (code, balance) in sorted)
            {
                var user = User.Register(code);

                if (balance > 0)
                {
                    user.PlaceOrder(
                        new UserId(user.Id),
                        new BatchNumber(1),
                        productTypeId,
                        new OrderDetail(new Money(balance * 10), null, null)
                    );
                }

                context.Users.Add(user);
            }
        }

        context.SaveChanges();
    }

    private static void SeedForTesting(ApplicationDbContext context)
    {
        // Always start with a clean slate in tests (prevents duplicate “Test User”)
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Minimal, predictable dataset for tests
        var pt50 = ProductType.Create(50m);
        var pt100 = ProductType.Create(100m);
        context.ProductTypes.AddRange(pt50, pt100);

        var testUser = User.Register("Test User");
        context.Users.Add(testUser);

        context.SaveChanges();
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
//                var user = User.Register(code);

//                if (balance > 0)
//                {
//                    user.PlaceOrder(
//                        new UserId(user.Id),
//                        new BatchNumber(1),
//                        productTypeId,
//                        new OrderDetail(new Money(balance * 10), null, null)
//                    );
//                }

//                context.Users.Add(user);
//            }
//        }

//        context.SaveChanges();
//    }
//}