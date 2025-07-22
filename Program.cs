using Api.Application;
using Api.Domain;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;

internal class Program
{
    private static void Main(string[] args)
    {
        // --- API / DI Setup ---
        var builder = WebApplication.CreateBuilder(args);

        // EF Core
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Repositories & Services
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IBatchAssignmentService, BatchAssignmentService>();
        builder.Services.AddScoped<UserService>();

        // Controllers
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
            DataSeeder.Seed(db);
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}

public static class DataSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        // only seed once
        if (context.Users.Any())
            return;

        var startingBalances = new Dictionary<string, decimal>
        {
            ["RS"] = 70m + 50m + 70m + 40m,
            ["TQ"] = 450m,
            ["KC"] = 120m + 20m + 40m,
            ["AR"] = 150m + (40m - 40m) + 100m + ((7 * 40m) - 20m),
            ["JR"] = 40m,
            ["GB"] = 390m,
            ["SS"] = 420m,
            ["DC"] = 50m + 40m + 40m,
            ["AL"] = 600m + 20m - 250m,
            ["DP"] = 0m,
            ["WB"] = 20m + 20m,
            ["AD"] = 0m,
            ["DK"] = 10m,
            ["MK"] = 70m,
            ["PT"] = 30m + (20m + 20m),
            ["JN"] = 30m,
            ["AN"] = 40m + 40m,
            ["MD"] = 0m,
            ["SN"] = 40m,
            ["TC"] = 40m,
        };

        int userId = 1;
        foreach (var (code, balance) in startingBalances)
        {
            // register a new User
            var user = User.Register(code);

            // seed a single Order representing starting balance
            user.PlaceOrder(
                new BatchNumber(1),
                null,
                new Money(balance)
            );

            context.Users.Add(user);
            userId++;
        }

        context.SaveChanges();
    }
}