using System.Collections.Generic;

namespace OrderEntryMockingPractice.Models
{
    public class Order
    {
        public int? CustomerId { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }
}