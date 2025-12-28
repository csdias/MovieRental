
namespace MovieRental.PaymentProviders
{
    public class PaymentProviderFactory: IPaymentProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public PaymentProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPaymentProvider GetPaymentProvider(string paymentMethod)
        {
            // solution using keyed services
            return _serviceProvider.GetRequiredKeyedService<IPaymentProvider>(paymentMethod);

            // solution using switch case
            // return paymentMethod.ToLower() switch
            // {
            //     "paypal" => _serviceProvider.GetRequiredService<PayPalProvider>(),
            //     "mbway" => _serviceProvider.GetRequiredService<MbWayProvider>(),
            //     _ => throw new NotSupportedException($"Payment method '{paymentMethod}' is not supported.")
            // };
        }
    }
}