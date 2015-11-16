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
        private IEmailService _emailService;
        private string reasonsForInvalidOrder;

        public OrderService(IProductRepository productRepository, IOrderFulfillmentService orderFulfillmentService, IEmailService emailService)
        {
            _productRepository = productRepository;
            _orderFulfillmentService = orderFulfillmentService;
            _emailService = emailService;
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
                OrderConfirmation orderConfirmation =_orderFulfillmentService.Fulfill(order);

                decimal netTotal = 0;
                TaxEntry taxUSA = new TaxEntry
                {
                    Description = "USA",
                    Rate = (decimal) 0.098
                };

                foreach (var orderItem in order.OrderItems)
                {
                    netTotal += orderItem.Product.Price*orderItem.Quantity;
                }

                OrderSummary orderSummary= new OrderSummary
                {
                    OrderNumber = orderConfirmation.OrderNumber,
                    OrderId = orderConfirmation.OrderId,
                    NetTotal = netTotal,
                    Total = taxUSA.Rate * netTotal,
                    Taxes = new List<TaxEntry>
                    {
                        taxUSA
                    }
                };

                _emailService.SendOrderConfirmationEmail(orderSummary.CustomerId, orderSummary.OrderId);
                return orderSummary;
            }
            else
                throw new Exception(reasonsForInvalidOrder);
        }
    }
}
