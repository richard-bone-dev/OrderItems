using Api.Application.Abstractions;
using Api.Domain.Core;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Persistence;
using Api.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Scrutor;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

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

        var applicationAssembly = typeof(ICommandHandler<,>).Assembly;

        builder.Services.Scan(scan => scan
            .FromAssemblies(applicationAssembly)
            .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
                .AsSelfWithInterfaces()
                .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
                .AsSelfWithInterfaces()
                .WithScopedLifetime()
        );

        builder.Services.Scan(scan => scan
            .FromAssemblies(typeof(IUserRepository).Assembly, typeof(ApplicationDbContext).Assembly)
                .AddClasses(c => c.InNamespaces("Api.Infrastructure.Repositories"))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
        );

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
                    DataSeeder.Seed(db, app.Environment.EnvironmentName);
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
            //Array.Empty<ProductType>();
            var products = new[] {
                ProductType.Create("None", new Money(0m)),
                ProductType.Create("1", new Money(40m)),
                ProductType.Create("2", new Money(70m)),
                ProductType.Create("2a", new Money(80m)),
                ProductType.Create("3", new Money(100m)),
                ProductType.Create("3a", new Money(120m)),
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
        var batch = context.Batches.OrderByDescending(b => b.CreatedAt).First();

        if (!context.Users.Any(u => u.Name.Value == "None"))
            context.Users.Add(User.Register(new UserName("None")));

        if (!context.Users.Any(u => u.Name.Value == "Admin"))
            context.Users.Add(User.Register(new UserName("Admin")));

        if (!context.Users.Any())
        {
            var starting = new Dictionary<string, decimal>
            {
                ["Aussie"] = 69m,
                ["Syd"] = 69m - 10m,
                ["Tropical"] = 62m,
                ["Stu"] = 54.5m,
                ["Rossweiler"] = (37.5m) + (4m - 4m) + 17m + 12m + (10m - 5m) - 24m + 12m,
                ["MrSherg"] = 19m + (4m - 4m),
                ["Tree"] = 16m,
                ["Saffer"] = 15m,
                ["Landscaper"] = 12m,
                ["SamDan"] = 12m,
                ["Pill"] = 12m,
                ["DanM"] = 10m,
                ["Sean"] = (23m - 15m),
                ["BoatMK"] = 8m,
                ["Linc"] = 8m,
                ["Jock"] = (8m - 5m) + 4m,
                ["BoatAnt"] = 6m,
                ["Crystal"] = 6m,
                ["Bordeaux"] = 4m,
                ["Lara"] = 4m,
                ["Aidy"] = 4m,
                ["Harry"] = 4m
                //["Rossweiler-1"] = 19m + (12m - 12m) + 17m + (17m - 8.5m) + 12m + 12m + (-30 - 4m) + 12m + 12m - 50m + 12m + 17m,
            };

            var sorted = starting.OrderByDescending(c => c.Value);
            var amt = starting.Values.Sum();

            foreach (var (code, balance) in sorted)
            {
                var user = User.Register(new UserName(code));

                if (balance > 0)
                {
                    var orderDetail = new OrderDetail(productTypeId, new Money(balance), DateTime.UtcNow);

                    var order = Order.Create(new UserId(user.Id), batch.Id, orderDetail);

                    user.AddOrder(order);
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
        //var pt50 = ProductType.Create(50m);
        //var pt100 = ProductType.Create(100m);
        //context.ProductTypes.AddRange(pt50, pt100);

        var testUser = User.Register(new UserName("Test User"));
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