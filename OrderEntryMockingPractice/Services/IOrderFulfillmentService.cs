using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public interface IOrderFulfillmentService
    {
        OrderConfirmation Fulfill(Order order);
    }
}