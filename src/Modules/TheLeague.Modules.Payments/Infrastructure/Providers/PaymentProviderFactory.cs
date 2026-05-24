using Microsoft.Extensions.DependencyInjection;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Payments.Infrastructure.Providers;

public class PaymentProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPaymentProvider GetProvider(PaymentMethod method)
    {
        // For now, all electronic methods use the mock provider.
        // In production, this would resolve Stripe, PayPal, GoCardless providers.
        return method switch
        {
            PaymentMethod.Stripe => _serviceProvider.GetRequiredService<MockPaymentProvider>(),
            PaymentMethod.PayPal => _serviceProvider.GetRequiredService<MockPaymentProvider>(),
            PaymentMethod.GoCardless => _serviceProvider.GetRequiredService<MockPaymentProvider>(),
            PaymentMethod.BankTransfer => _serviceProvider.GetRequiredService<MockPaymentProvider>(),
            PaymentMethod.Cash => _serviceProvider.GetRequiredService<MockPaymentProvider>(),
            PaymentMethod.Cheque => _serviceProvider.GetRequiredService<MockPaymentProvider>(),
            _ => _serviceProvider.GetRequiredService<MockPaymentProvider>()
        };
    }
}
