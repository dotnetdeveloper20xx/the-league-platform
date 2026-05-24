using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheLeague.Modules.Payments.Infrastructure;
using TheLeague.Modules.Payments.Infrastructure.Persistence;
using TheLeague.Modules.Payments.Infrastructure.Providers;
using TheLeague.Shared.Contracts;

namespace TheLeague.Modules.Payments;

public class PaymentsModule : IModule
{
    public string Name => "Payments";

    public void RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "payments")));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PaymentsModule).Assembly));

        // Register payment infrastructure
        services.AddSingleton<MockPaymentProvider>();
        services.AddScoped<PaymentProviderFactory>();
        services.AddSingleton(new PlatformFeeCalculator(
            feePercentage: configuration.GetValue<decimal>("Payments:PlatformFeePercentage", 1.5m),
            minimumFee: configuration.GetValue<decimal>("Payments:MinimumFee", 0.30m)));
    }

    public void UseModule(IApplicationBuilder app)
    {
        // No middleware needed for this module
    }
}
