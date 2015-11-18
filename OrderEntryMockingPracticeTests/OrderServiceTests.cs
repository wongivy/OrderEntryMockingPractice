﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;
using Rhino.Mocks;

namespace OrderEntryMockingPracticeTests
{
    public class OrderServiceTests
    {
        private ICustomerRepository _customerRepository;
        private IEmailService _emailService;
        private IOrderFulfillmentService _orderFulfillmentService;
        private IProductRepository _productRepository;
        private ITaxRateService _taxRateService;

        [SetUp]
        public void PlaceOrder_SetUp()
        {
            _customerRepository = MockRepository.GenerateMock<ICustomerRepository>();
            _emailService = MockRepository.GenerateMock<IEmailService>();
            _orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();
            _productRepository = MockRepository.GenerateMock<IProductRepository>();
        }

        [Test]
        public void PlaceOrder__NotUniqueByProductSKU__DoesThrowException()
        {
            // Arrange
            var orderService = new OrderService(_productRepository, _orderFulfillmentService, _emailService, _customerRepository, _taxRateService);

            var order = new Order
            {
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Product = new Product {Sku = "apple"} },
                    new OrderItem { Product = new Product {Sku = "apple"} },
                }
            };
            
            // Act // Assert   
            var exception = Assert.Throws<Exception>(() => orderService.PlaceOrder(order));
            Assert.That(exception.Message, Is.StringContaining("The orderItems are not unique by Sku."));
        }

        [Test]
        public void PlaceOrder__UniqueByProductSKU__DoesNotThrowException()
        {
            // Arrange
            _customerRepository.Stub(c => c.Get(Arg<int>.Is.Anything)).Return(new Customer());
            _productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);

            _orderFulfillmentService.Expect(service => service.Fulfill(Arg<Order>.Is.Anything)).Return(new OrderConfirmation());

            var orderService = new OrderService(_productRepository, _orderFulfillmentService, _emailService, _customerRepository, _taxRateService);

            var order = new Order
            {
                CustomerId = 1,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Product = new Product { Sku = "apple" } },
                    new OrderItem { Product = new Product { Sku = "apple2" } }
                }
            };

            // Act 
            var result = orderService.PlaceOrder(order);

            // Assert   
            _productRepository.VerifyAllExpectations();
        }

        [Test]
        public void PlaceOrder_AllProductsInStock_DoesNotThrowException()
        {
            // Arrange
            _customerRepository.Stub(c => c.Get(Arg<int>.Is.Anything)).Return(new Customer());
            _orderFulfillmentService.Stub(or => _orderFulfillmentService.Fulfill(Arg<Order>.Is.Anything)).Return(new OrderConfirmation());

            _productRepository.Expect(pr => pr.IsInStock("apple")).Return(true);
            _productRepository.Expect(pr => pr.IsInStock("apple2")).Return(true);

            var orderService = new OrderService(_productRepository, _orderFulfillmentService, _emailService, _customerRepository, _taxRateService);

            var order = new Order
            {
                CustomerId = 1,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Product = new Product { Sku = "apple" } },
                    new OrderItem { Product = new Product { Sku = "apple2" } }
                }
            };

            // Act 
            var result = orderService.PlaceOrder(order);

            // Assert   
            _productRepository.VerifyAllExpectations();
        }

        [Test]
        public void PlaceOrder_AllProductsNotInStock_DoesThrowException()
        {
            // Arrange
            _productRepository.Expect(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(false);

            var orderService = new OrderService(_productRepository, _orderFulfillmentService, _emailService, _customerRepository, _taxRateService);

            var order = new Order
            {
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Product = new Product { Sku = "apple" } },
                    new OrderItem { Product = new Product { Sku = "apple2" } }
                }
            };

            // Act // Assert   
            var exception = Assert.Throws<Exception>(() => orderService.PlaceOrder(order));
            Assert.That(exception.Message, Is.StringContaining("Not all products are in stock."));
        }

        [Test]
        public void PlaceOrder_OrderIsNotValid_ThrowsExceptionWithListOfWhyNotValid()
        {
            // Arrange
            _productRepository.Expect(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(false);

            var orderService = new OrderService(_productRepository, _orderFulfillmentService, _emailService, _customerRepository, _taxRateService);

            var order = new Order
            {
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Product = new Product { Sku = "apple" } },
                    new OrderItem { Product = new Product { Sku = "apple2" } }
                }
            };

            // Act // Assert   
            Assert.Throws<Exception>(() => orderService.PlaceOrder(order));
        }

        [Test]
        public void PlaceOrder_OrderIsValid_ReturnOrderSummary()
        {
            // Arrange
            _customerRepository.Stub(c => c.Get(Arg<int>.Is.Anything)).Return(new Customer());
            _productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);
            _orderFulfillmentService.Stub(or => _orderFulfillmentService.Fulfill(Arg<Order>.Is.Anything)).Return(new OrderConfirmation());

            var orderService = new OrderService(_productRepository, _orderFulfillmentService, _emailService, _customerRepository, _taxRateService);

            var order = new Order
            {
                CustomerId = 1,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Product = new Product { Sku = "apple" } },
                    new OrderItem { Product = new Product { Sku = "apple2" } }
                }
            };

            // Act // Assert   
            var actual = orderService.PlaceOrder(order);
            Assert.IsInstanceOf<OrderSummary>(actual);
        }

        [Test]
        public void PlaceOrder_OrderIsValid_OrderSubmittedToOrderFulfillmentService()
        {
            // Arrange
            _customerRepository.Stub(c => c.Get(Arg<int>.Is.Anything)).Return(new Customer());
            _productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);

            var order = new Order
            {
                CustomerId = 1,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Product = new Product { Sku = "apple" } },
                    new OrderItem { Product = new Product { Sku = "apple2" } }
                }
            };

            _orderFulfillmentService.Expect(or => or.Fulfill(order)).Return(new OrderConfirmation());

            var orderService = new OrderService(_productRepository, _orderFulfillmentService, _emailService, _customerRepository, _taxRateService);

            // Act  
            var result = orderService.PlaceOrder(order);

            // Assert  
            _orderFulfillmentService.VerifyAllExpectations();
        }

        [Test]
        public void PlaceOrder_OrderIsValid_OrderSummaryContainsFulfillmentConfirmationNumber()
        {
            // Arrange
            var expectedConfirmationNumber = "1";
            _customerRepository.Stub(c => c.Get(Arg<int>.Is.Anything)).Return(new Customer());
            _productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);

            var order = new Order
            {
                CustomerId = 1,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Product = new Product { Sku = "apple" } },
                    new OrderItem { Product = new Product { Sku = "apple2" } },
                }
            };

            _orderFulfillmentService.Stub(or => or.Fulfill(order)).Return(new OrderConfirmation { OrderNumber = "1" });

            var orderService = new OrderService(_productRepository, _orderFulfillmentService, _emailService, _customerRepository, _taxRateService);

            // Act  
            var result = orderService.PlaceOrder(order);

            // Assert  
            Assert.That(result.OrderNumber, Is.EqualTo(expectedConfirmationNumber));
        }

        [Test]
        public void PlaceOrder_OrderIsValid_OrderSummaryContainsOrderId()
        {
            // Arrange
            var expectedOrderId = 1;
            _customerRepository.Stub(c => c.Get(Arg<int>.Is.Anything)).Return(new Customer());
            _productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);

            var order = new Order
            {
                CustomerId = 1,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Product = new Product { Sku = "apple" } },
                    new OrderItem { Product = new Product { Sku = "apple2" } },
                }
            };

            _orderFulfillmentService.Stub(or => or.Fulfill(order)).Return(new OrderConfirmation{OrderNumber = "1", OrderId = 1, });

            var orderService = new OrderService(_productRepository, _orderFulfillmentService, _emailService, _customerRepository, _taxRateService);


            // Act  
            var result = orderService.PlaceOrder(order);

            // Assert  
            Assert.That(result.OrderId, Is.EqualTo(expectedOrderId), "OrderId");
        }

        [Test]
        public void PlaceOrder_OrderIsValid_OrderSummaryContainsApplicableTaxes()
        {
            // Arrange
            _customerRepository.Stub(c => c.Get(Arg<int>.Is.Anything)).Return(new Customer());
            _productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);
            _orderFulfillmentService.Stub(or => _orderFulfillmentService.Fulfill(Arg<Order>.Is.Anything))
                .Return(new OrderConfirmation());

            var orderService = new OrderService(_productRepository, _orderFulfillmentService, _emailService, _customerRepository, _taxRateService);

            var order = new Order
            {
                CustomerId = 1,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Product = new Product { Sku = "apple" } },
                    new OrderItem { Product = new Product { Sku = "apple2" } }
                }
            };

            // Act  
            var result = orderService.PlaceOrder(order);

            // Assert  
            Assert.That(result.Taxes.ToList().Exists(tax => tax.Description == "USA"));
        }

        [Test]
        public void PlaceOrder_OrderIsValid_OrderSummaryContainsNetTotal()
        {
            // Arrange
            _customerRepository.Stub(c => c.Get(Arg<int>.Is.Anything)).Return(new Customer());
            _orderFulfillmentService.Stub(or => or.Fulfill(Arg<Order>.Is.Anything)).Return(new OrderConfirmation());
            _productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);

            var orderService = new OrderService(_productRepository, _orderFulfillmentService, _emailService, _customerRepository, _taxRateService);

            var order = new Order
            {
                CustomerId = 1,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Product = new Product { Sku = "apple", Price = (decimal) 2.00},
                        Quantity = 2
                    },
                    new OrderItem
                    {
                        Product = new Product { Sku = "apple2", Price = (decimal) 2.50 },
                        Quantity = 1
                    }
                }
            };

            // Act  
            var result = orderService.PlaceOrder(order);

            // Assert  
            Assert.AreEqual(6.50, result.NetTotal);
        }

        [Test]
        public void PlaceOrder_OrderIsValid_SendConfirmationEmail()
        {
            // Arrange
            var expectedCustomerId = 1;
            var expectedOrderId = 1;
            var order = new Order
            {
                CustomerId = 1,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Product = new Product { Sku = "apple", Price = (decimal) 2.00},
                        Quantity = 2
                    },
                    new OrderItem
                    {
                        Product = new Product { Sku = "apple2", Price = (decimal) 2.50 },
                        Quantity = 1
                    }
                }
            };

            _customerRepository.Stub(c => c.Get(Arg<int>.Is.Anything)).Return(new Customer());
            _productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);
            _orderFulfillmentService.Stub(or => or.Fulfill(order)).Return(new OrderConfirmation {OrderId = 1,CustomerId = 1});

            _emailService.Expect(x => x.SendOrderConfirmationEmail(expectedCustomerId, expectedOrderId));

            var orderService = new OrderService(_productRepository, _orderFulfillmentService, _emailService, _customerRepository, _taxRateService);

            // Act  
            var result = orderService.PlaceOrder(order);

            // Assert  
            _emailService.VerifyAllExpectations();
        }

        [Test]
        public void PlaceOrder_CustomerNotInCustomerRepository_DoesThrowException()
        {
            // Arrange
            _customerRepository.Stub(c => c.Get(Arg<int>.Is.Anything)).Return(null);
            _productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);

            var orderService = new OrderService(_productRepository, _orderFulfillmentService, _emailService, _customerRepository, _taxRateService);

            var order = new Order
            {
                CustomerId = 1,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Product = new Product { Sku = "apple", Price = (decimal) 2.00},
                        Quantity = 2
                    },
                    new OrderItem
                    {
                        Product = new Product { Sku = "apple2", Price = (decimal) 2.50 },
                        Quantity = 1
                    }
                }
            };
            // Act  // Assert  
            var exception = Assert.Throws<Exception>(() => orderService.PlaceOrder(order));
            Assert.That(exception.Message, Is.StringContaining("The customer is null or cannot be retrieved."));
        }

        [Test]
        public void PlaceOrder_CustomerIsInCustomerRepository_DoesNotThrowException()
        {
            // Arrange
            var expectedCustomerId = 1;

            _productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);
            _orderFulfillmentService.Stub(or => or.Fulfill(Arg<Order>.Is.Anything)).Return(new OrderConfirmation());

            _customerRepository.Expect(c => c.Get(expectedCustomerId)).Return(new Customer {CustomerId = expectedCustomerId});

            var orderService = new OrderService(_productRepository, _orderFulfillmentService, _emailService, _customerRepository, _taxRateService);

            var order = new Order
            {
                CustomerId = 1,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Product = new Product { Sku = "apple", Price = (decimal) 2.00},
                        Quantity = 2
                    },
                    new OrderItem
                    {
                        Product = new Product { Sku = "apple2", Price = (decimal) 2.50 },
                        Quantity = 1
                    }
                }
            };

            // Act  
            var result = orderService.PlaceOrder(order);

            // Assert  
            _customerRepository.VerifyAllExpectations();
        }

    }
}