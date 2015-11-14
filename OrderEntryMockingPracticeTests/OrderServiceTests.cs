using System;
using System.Collections.Generic;
using NUnit.Framework;
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;
using Rhino.Mocks;

namespace OrderEntryMockingPracticeTests
{
    public class OrderServiceTests
    {
        [Test]
        public void PlaceOrder__NotUniqueByProductSKU__ThrowException()
        {
            // Arrange
            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();

            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            OrderService orderService = new OrderService(productRepository, orderFulfillmentService);

            var order = new Order
            {
                OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    Product = new Product { Sku = "apple"}
                },
                new OrderItem
                {
                    Product = new Product
                    {
                        Sku = "apple"
                    }}
                }
            };

            // Act // Assert   
            Assert.Throws<Exception>(() => orderService.PlaceOrder(order)).Message.Contains("The product is not in stock");
        }

        [Test]
        public void PlaceOrder__UniqueByProductSKU__DoesNotThrowException()
        {
            // Arrange
            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();

            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            OrderService orderService = new OrderService(productRepository, orderFulfillmentService);
            productRepository.Stub(pr => pr.IsInStock("apple")).Return(true);
            productRepository.Stub(pr => pr.IsInStock("apple2")).Return(true);

            var order = new Order
            {
                OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    Product = new Product { Sku = "apple"}
                },
                new OrderItem
                {
                    Product = new Product
                    {
                        Sku = "apple2"
                    }}
                }
            };

            // Act // Assert   
            Assert.DoesNotThrow(() => orderService.PlaceOrder(order));
        }

        [Test]
        public void PlaceOrder_AllProductsInStock_DoesNotThrowException()
        {
            // Arrange
            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();

            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            productRepository.Stub(pr => pr.IsInStock("apple")).Return(true);
            productRepository.Stub(pr => pr.IsInStock("apple2")).Return(true);

            OrderService orderService = new OrderService(productRepository, orderFulfillmentService);

            var order = new Order
            {
                OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    Product = new Product
                    {
                        Sku = "apple"
                    }
                },
                new OrderItem
                {
                    Product = new Product
                    {
                        Sku = "apple2"
                    }}
                }
            };

            // Act // Assert   
            Assert.DoesNotThrow(() => orderService.PlaceOrder(order));
        }

        [Test]
        public void PlaceOrder_AllProductsNotInStock_DoesThrowException()
        {
            // Arrange
            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();

            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(false);
            
            OrderService orderService = new OrderService(productRepository, orderFulfillmentService);

            var order = new Order
            {
                OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    Product = new Product
                    {
                        Sku = "apple"
                    }
                },
                new OrderItem
                {
                    Product = new Product
                    {
                        Sku = "apple2"
                    }}
                }
            };

            // Act // Assert   
            Assert.Throws<Exception>(() => orderService.PlaceOrder(order)).Message.Contains("The OrderItems are not unqiue by Sku.");
        }

        [Test]
        public void PlaceOrder_OrderIsNotValid_ThrowsExceptionWithListOfWhyNotValid()
        {
            // Arrange
            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();

            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(false);
            
            OrderService orderService = new OrderService(productRepository, orderFulfillmentService);

            var order = new Order
            {
                OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    Product = new Product
                    {
                        Sku = "apple"
                    }
                },
                new OrderItem
                {
                    Product = new Product
                    {
                        Sku = "apple2"
                    }}
                }
            };

            // Act // Assert   
            Assert.Throws<Exception>(() => orderService.PlaceOrder(order));
        }

        [Test]
        public void PlaceOrder_OrderIsValid_ReturnOrderSummary()
        {
            // Arrange
            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();

            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);
            
            OrderService orderService = new OrderService(productRepository, orderFulfillmentService);

            var order = new Order
            {
                OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    Product = new Product
                    {
                        Sku = "apple"
                    }
                },
                new OrderItem
                {
                    Product = new Product
                    {
                        Sku = "apple2"
                    }}
                }
            };

            // Act // Assert   
            var actual = orderService.PlaceOrder(order);
            Assert.IsInstanceOf<OrderSummary>(actual);
        }

        [Test]
        public void PlaceOrder_OrderIsValid_SubmittedToOrderFulfillmentService()
        {
            // Arrange
            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();
            
            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);
            OrderService orderService = new OrderService(productRepository, orderFulfillmentService);

            var order = new Order
            {
                OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    Product = new Product
                    {
                        Sku = "apple"
                    }
                },
                new OrderItem
                {
                    Product = new Product
                    {
                        Sku = "apple2"
                    }}
                }
            };

            // Act  
            orderService.PlaceOrder(order);

            // Assert  
            orderFulfillmentService.AssertWasCalled(x => x.Fulfill(order));
        }

        [Test]
        public void PlaceOrder_OrderSubmittedToOrderFulfillmentService_ContainsFulfillmentConfirmationNumber()
        {
            // Arrange
            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();
            
            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);
            OrderService orderService = new OrderService(productRepository, orderFulfillmentService);

            var order = new Order
            {
                OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    Product = new Product
                    {
                        Sku = "apple"
                    }
                },
                new OrderItem
                {
                    Product = new Product
                    {
                        Sku = "apple2"
                    }}
                }
            };

            // Act  
            orderService.PlaceOrder(order);

            // Assert  
            orderFulfillmentService.AssertWasCalled(x => x.Fulfill(order));
        }
    }
}
