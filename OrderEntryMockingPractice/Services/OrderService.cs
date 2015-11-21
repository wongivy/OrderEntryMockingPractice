using System;
using System.Collections.Generic;
using System.Linq;
using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        private ICustomerRepository _customerRepository;
        private IProductRepository _productRepository;
        private IOrderFulfillmentService _orderFulfillmentService;
        private IEmailService _emailService;
        private ITaxRateService _taxRateService;
        private string _reasonsForInvalidOrder;

        public OrderService(IProductRepository productRepository, IOrderFulfillmentService orderFulfillmentService, IEmailService emailService, ICustomerRepository customerRepository, ITaxRateService taxRateService)
        {
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _orderFulfillmentService = orderFulfillmentService;
            _emailService = emailService;
            _taxRateService = taxRateService;
            _reasonsForInvalidOrder = "";
        }

        public OrderSummary PlaceOrder(Order order)
        {
            List<OrderItem> orderItems = order.OrderItems;
            Customer customer = RetrieveCustomer(order.CustomerId);

            if (OrderIsValid(order, orderItems))
            {
                OrderConfirmation orderConfirmation =_orderFulfillmentService.Fulfill(order);

                IEnumerable<TaxEntry> applicableTaxes = RetrieveTaxEntries(customer).ToList();
                decimal netTotal = CalculateNetTotal(order);
                decimal orderTotal = CalculateOrderTotal(netTotal, applicableTaxes);
                
                OrderSummary orderSummary= new OrderSummary
                {
                    OrderNumber = orderConfirmation.OrderNumber,
                    OrderId = orderConfirmation.OrderId,
                    CustomerId = orderConfirmation.CustomerId,
                    Taxes = applicableTaxes,
                    NetTotal = netTotal,
                    Total = orderTotal,
                };

                _emailService.SendOrderConfirmationEmail(orderSummary.CustomerId, orderSummary.OrderId);
                return orderSummary;
            }
            else
                throw new Exception(_reasonsForInvalidOrder);
        }

        private Customer RetrieveCustomer(int? customerId)
        {
            var customerIdIsValid = customerId == null;
            if (customerIdIsValid)

            return null;
        }

        private decimal CalculateOrderTotal(decimal netTotal, IEnumerable<TaxEntry> applicableTaxes)
        {
            decimal orderTotal = 0;
            var taxes = applicableTaxes.ToList();
            for (int index = 0; index < taxes.Count; index++)
            {
                orderTotal += taxes[index].Rate*netTotal;
            }
            return orderTotal;
        }

        private decimal CalculateNetTotal(Order order)
        {
            decimal netTotal = 0;
            foreach (var orderItem in order.OrderItems)
            {
                netTotal += orderItem.Product.Price*orderItem.Quantity;
            }
            return netTotal;
        }

        private bool OrderIsValid(Order order, List<OrderItem> orderItems)
        {
            if (!orderItemsAreUnique(orderItems))
                _reasonsForInvalidOrder += "The orderItems are not unique by Sku. \n";
            if (!ItemsAreInStock(orderItems))
                _reasonsForInvalidOrder += "Not all products are in stock. \n";
            if (!IsCustomerValid(order.CustomerId))
                _reasonsForInvalidOrder += "The customer is null or cannot be retrieved.";
            return _reasonsForInvalidOrder.Length == 0;
        }

        private bool orderItemsAreUnique(List<OrderItem> orderItems)
        {
            var skus = orderItems.Select(orderItem => orderItem.Product.Sku).ToList();
            return skus.Count == skus.Distinct().Count();
        }

        private bool ItemsAreInStock(List<OrderItem> orderItems)
        {
            foreach (var orderItem in orderItems)
            {
                var productIsNotInstock = !_productRepository.IsInStock(orderItem.Product.Sku);
                if (productIsNotInstock)
                    return false;
            }
            return true;
        }

        private bool IsCustomerValid(int? customerId)
        {
            var customerIdIsValid = customerId != null;
            if (customerIdIsValid)
            {
                var customerIsInCustomerRepository = _customerRepository.Get((int) customerId) != null;
                if (customerIsInCustomerRepository)
                    return true;
            }
            return false;
        }

        private IEnumerable<TaxEntry> RetrieveTaxEntries(Customer customer)
        {
            String customerPostalCode = customer.PostalCode;
            String customerCountry = customer.Country;
            List<TaxEntry> taxes = _taxRateService.GetTaxEntries(customerPostalCode, customerCountry).ToList();
            return taxes;
        } 
    }
}
