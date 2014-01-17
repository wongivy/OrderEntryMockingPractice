using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public interface ICustomerRepository
    {
        Customer Get(int customerId);
    }
}