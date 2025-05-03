using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AstroFinance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add infrastructure services here
        // Example:
        // services.AddDbContext<ApplicationDbContext>(options =>
        //     options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        
        // Register repositories
        // services.AddScoped<ICustomerRepository, CustomerRepository>();
        
        // Register other services
        // services.AddTransient<IEmailService, EmailService>();
        
        return services;
    }
}