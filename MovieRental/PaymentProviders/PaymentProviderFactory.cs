
namespace MovieRental.PaymentProviders
{
    public class PaymentProviderFactory: IPaymentProviderFactory
    {
        private IServiceProvider _serviceProvider;

        public PaymentProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPaymentProvider GetPaymentProvider(string paymentMethod)
        {
            return paymentMethod.ToLower() switch
            {
                "paypal" => _serviceProvider.GetRequiredService<PayPalProvider>(),
                "mbway" => _serviceProvider.GetRequiredService<MbWayProvider>(),
                _ => throw new NotSupportedException($"Payment method '{paymentMethod}' is not supported.")
            };
        }
    }
}