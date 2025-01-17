using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Infrastructure.Data;
using DTCBillingSystem.Infrastructure.Repositories;
using DTCBillingSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DTCBillingSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
        }

        // Register database context
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));

        // Register repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register infrastructure-specific services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }
} 