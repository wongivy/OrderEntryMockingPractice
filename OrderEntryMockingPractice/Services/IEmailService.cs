namespace OrderEntryMockingPractice.Services
{
    public interface IEmailService
    {
        void SendOrderConfirmationEmail(int customerId, int orderId);
    }
}