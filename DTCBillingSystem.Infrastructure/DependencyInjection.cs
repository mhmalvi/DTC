using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Services;
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

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));

        // Register repositories and infrastructure services
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Register Core services that are required by Infrastructure
        // but not already registered in Core.AddCoreServices
        services.AddScoped<IUserService, DTCBillingSystem.Core.Services.UserService>();
        services.AddScoped<IPasswordHasher, DTCBillingSystem.Core.Services.PasswordHasher>();
        services.AddScoped<IAuditService, DTCBillingSystem.Core.Services.AuditService>();

        return services;
    }
} 