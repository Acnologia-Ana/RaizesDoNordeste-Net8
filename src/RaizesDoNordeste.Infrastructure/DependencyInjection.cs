using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaizesDoNordeste.Application.Interfaces;
using RaizesDoNordeste.Infrastructure.Persistence;
using RaizesDoNordeste.Infrastructure.Services;

namespace RaizesDoNordeste.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' nao configurada.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<
            ICatalogoService,
            CatalogoService>();

        services.AddScoped<
            IAuthService,
            AuthService>();

        return services;
    }
}
