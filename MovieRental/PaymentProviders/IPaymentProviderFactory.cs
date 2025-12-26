
namespace MovieRental.PaymentProviders
{
    public interface IPaymentProviderFactory
    {
        IPaymentProvider GetPaymentProvider(string paymentMethod);
    }
}