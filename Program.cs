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
            ["RS"] = 70m,
            ["TQ"] = 650m,
            ["KC"] = 120m,
            ["AR"] = 370m,
            ["JR"] = 40m,
            ["GB"] = 390m,
            ["SS"] = 380m,
            ["DC"] = 110m,
            ["AL"] = 370m,
            ["DP"] = 0m,
            ["WB"] = 0m,
            ["AD"] = 40m,
            ["DK"] = 0m,
            ["MK"] = 80m,
            ["PT"] = 0m,
            ["JN"] = 30m,
            ["AN"] = 50m
        };

        int userId = 1;
        foreach (var (code, balance) in startingBalances)
        {
            // register a new User
            var user = User.Register();

            // seed a single Order representing starting balance
            user.PlaceOrder(
                new BatchNumber(1),
                1m,
                new Money(balance)
            );

            context.Users.Add(user);
            userId++;
        }

        context.SaveChanges();
    }
}