using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
            IEmailService emailService = MockRepository.GenerateMock<IEmailService>();

            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();

            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            OrderService orderService = new OrderService(productRepository, orderFulfillmentService, emailService);

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
            IEmailService emailService = MockRepository.GenerateMock<IEmailService>();

            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();
            orderFulfillmentService.Stub(or => orderFulfillmentService.Fulfill(Arg<Order>.Is.Anything)).Return(new OrderConfirmation
            {
                OrderNumber = "1"
            });

            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            OrderService orderService = new OrderService(productRepository, orderFulfillmentService, emailService);
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
            IEmailService emailService = MockRepository.GenerateMock<IEmailService>();

            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();
            orderFulfillmentService.Stub(or => orderFulfillmentService.Fulfill(Arg<Order>.Is.Anything)).Return(new OrderConfirmation
            {
                OrderNumber = "1"
            });

            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            productRepository.Stub(pr => pr.IsInStock("apple")).Return(true);
            productRepository.Stub(pr => pr.IsInStock("apple2")).Return(true);

            OrderService orderService = new OrderService(productRepository, orderFulfillmentService, emailService);

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
            IEmailService emailService = MockRepository.GenerateMock<IEmailService>();

            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();

            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(false);

            OrderService orderService = new OrderService(productRepository, orderFulfillmentService, emailService);

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
            IEmailService emailService = MockRepository.GenerateMock<IEmailService>();

            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();

            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(false);

            OrderService orderService = new OrderService(productRepository, orderFulfillmentService, emailService);

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
            IEmailService emailService = MockRepository.GenerateMock<IEmailService>();

            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();
            orderFulfillmentService.Stub(or => orderFulfillmentService.Fulfill(Arg<Order>.Is.Anything)).Return(new OrderConfirmation
            {
                OrderNumber = "1"
            });

            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);

            OrderService orderService = new OrderService(productRepository, orderFulfillmentService, emailService);

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
        public void PlaceOrder_OrderIsValid_OrderSubmittedToOrderFulfillmentService()
        {
            // Arrange
            IEmailService emailService = MockRepository.GenerateMock<IEmailService>();

            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();
            orderFulfillmentService.Stub(or => orderFulfillmentService.Fulfill(Arg<Order>.Is.Anything)).Return(new OrderConfirmation
            {
                OrderNumber = "1"
            });

            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);
            OrderService orderService = new OrderService(productRepository, orderFulfillmentService, emailService);

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
        public void PlaceOrder_OrderIsValid_OrderSummaryContainsFulfillmentConfirmationNumber()
        {
            // Arrange
            IEmailService emailService = MockRepository.GenerateMock<IEmailService>();

            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();
            orderFulfillmentService.Stub(or => orderFulfillmentService.Fulfill(Arg<Order>.Is.Anything)).Return(new OrderConfirmation
            {
                OrderNumber = "1"
            });

            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);
            OrderService orderService = new OrderService(productRepository, orderFulfillmentService, emailService);

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
            var result = orderService.PlaceOrder(order);

            // Assert  
            Assert.That(result.OrderNumber == "1");
        }

        [Test]
        public void PlaceOrder_OrderIsValid_OrderSummaryContainsOrderId()
        {
            // Arrange
            IEmailService emailService = MockRepository.GenerateMock<IEmailService>();

            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();
            orderFulfillmentService.Stub(or => orderFulfillmentService.Fulfill(Arg<Order>.Is.Anything)).Return(new OrderConfirmation
            {
                OrderNumber = "1",
                OrderId = 1
            });

            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);
            OrderService orderService = new OrderService(productRepository, orderFulfillmentService, emailService);

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
            var result = orderService.PlaceOrder(order);

            // Assert  
            Assert.That(result.OrderId == 1);
        }


        [Test]
        public void PlaceOrder_OrderIsValid_OrderSummaryContainsApplicableTaxes()
        {
            // Arrange
            IEmailService emailService = MockRepository.GenerateMock<IEmailService>();

            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();
            orderFulfillmentService.Stub(or => orderFulfillmentService.Fulfill(Arg<Order>.Is.Anything)).Return(new OrderConfirmation
            {
                OrderNumber = "1",
                OrderId = 1
            });

            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);
            OrderService orderService = new OrderService(productRepository, orderFulfillmentService, emailService);

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
                    }
                }
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
            IEmailService emailService = MockRepository.GenerateMock<IEmailService>();

            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();
            orderFulfillmentService.Stub(or => orderFulfillmentService.Fulfill(Arg<Order>.Is.Anything)).Return(new OrderConfirmation
            {
                OrderNumber = "1",
                OrderId = 1
            });

            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);
            OrderService orderService = new OrderService(productRepository, orderFulfillmentService, emailService);

            var order = new Order
            {
                OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    Product = new Product
                    {
                        Sku = "apple",
                        Price = (decimal) 2.00
                    },
                    Quantity = 2
                },
                new OrderItem
                {
                    Product = new Product
                    {
                        Sku = "apple2",
                        Price = (decimal) 2.50
                    },
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
            IEmailService emailService = MockRepository.GenerateMock<IEmailService>();
            emailService.Stub(x => x.SendOrderConfirmationEmail(Arg<int>.Is.Equal(123), Arg<int>.Is.Equal(1)));

            IOrderFulfillmentService orderFulfillmentService = MockRepository.GenerateMock<IOrderFulfillmentService>();
            orderFulfillmentService.Stub(or => orderFulfillmentService.Fulfill(Arg<Order>.Is.Anything)).Return(new OrderConfirmation
            {
                OrderNumber = "1",
                OrderId = 1,
                CustomerId = 123
            });

            IProductRepository productRepository = MockRepository.GenerateMock<IProductRepository>();
            productRepository.Stub(pr => pr.IsInStock(Arg<string>.Is.Anything)).Return(true);
            OrderService orderService = new OrderService(productRepository, orderFulfillmentService, emailService);

            var order = new Order
            {
                OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    Product = new Product
                    {
                        Sku = "apple",
                        Price = (decimal) 2.00
                    },
                    Quantity = 2
                },
                new OrderItem
                {
                    Product = new Product
                    {
                        Sku = "apple2",
                        Price = (decimal) 2.50
                    },
                    Quantity = 1
                }
                }
            };

            // Act  
            var result = orderService.PlaceOrder(order);
            
            // Assert  
            emailService.AssertWasCalled(x => x.SendOrderConfirmationEmail(result.CustomerId, result.OrderId));
        }


    }
}
