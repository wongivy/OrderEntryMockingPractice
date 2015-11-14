using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        private IProductRepository _productRepository;
        private IOrderFulfillmentService _orderFulfillmentService;
        private string reasonsForInvalidOrder;

        public OrderService(IProductRepository ProductRepository, IOrderFulfillmentService orderFulfillmentService)
        {
            _productRepository = ProductRepository;
            _orderFulfillmentService = orderFulfillmentService;
            reasonsForInvalidOrder = "";
        }

        public OrderSummary PlaceOrder(Order order)
        {
            var skus = order.OrderItems.Select(orderItem => orderItem.Product.Sku).ToList();
            if (skus.Count() != skus.Distinct().Count())
                reasonsForInvalidOrder += "The OrderItems are not unqiue by Sku. ";

            foreach (var orderItem in order.OrderItems)
            {
                var productIsNotInstock = !_productRepository.IsInStock(orderItem.Product.Sku);
                if(productIsNotInstock)
                    reasonsForInvalidOrder += "The product is not in stock";
            }

            var OrderIsValid = reasonsForInvalidOrder.Length == 0;
            if (OrderIsValid)
            {
                _orderFulfillmentService.Fulfill(order);
                return new OrderSummary();
            }
            else
                throw new Exception(reasonsForInvalidOrder);
        }
    }
}
