namespace OrderEntryMockingPractice.Services
{
    public interface IProductRepository
    {
        bool IsInStock(string productSku);
    }
}