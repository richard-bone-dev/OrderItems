using Api.Application.Abstractions;
using System.Reflection;

namespace Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(c => c.AssignableTo(typeof(ICommandHandlerAsync<,>)))
                .AsSelfWithInterfaces()
                .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IQueryHandlerAsync<,>)))
                .AsSelfWithInterfaces()
                .WithScopedLifetime()
        );

        return services;
    }
}
