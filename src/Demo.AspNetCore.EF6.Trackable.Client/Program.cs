﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using Demo.AspNetCore.EF6.Trackable.Entities.Shared.Net45.Models;
using TrackableEntities.Client;

namespace Demo.AspNetCore.EF6.Trackable.Client
{
    class Program
    {
        static void Main()
        {
            // To debug with Fiddler, append .fiddler to localhost
            const string baseAddress = "http://localhost:" + "5000" + "/";

            // Start
            Console.WriteLine("Press Enter to start");
            Console.ReadLine();
            INorthwindServiceAgent serviceAgent = new NorthwindServiceAgent(baseAddress, new JsonMediaTypeFormatter());

            // Get customers
            Console.WriteLine("Customers:");
            IEnumerable<Customer> customers = serviceAgent.GetCustomers().Result;
            if (customers == null) return;
            foreach (var c in customers)
                PrintCustomer(c);

            // Get orders for a customer
            Console.WriteLine("\nGet customer orders {CustomerId}:");
            string customerId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(customerId)) return;
            if (!customers.Any(c => string.Equals(c.CustomerId, customerId, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Invalid customer id: {0}", customerId.ToUpper());
                return;
            }
            IEnumerable<Order> orders = serviceAgent.GetCustomerOrders(customerId).Result;
            foreach (var o in orders)
                PrintOrder(o);

            // Get an order
            Console.WriteLine("\nGet an order {OrderId}:");
            int orderId = int.Parse(Console.ReadLine());
            if (!orders.Any(o => o.OrderId == orderId))
            {
                Console.WriteLine("Invalid order id: {0}", orderId);
                return;
            }
            Order order = serviceAgent.GetOrder(orderId).Result;
            PrintOrderWithDetails(order);

            // Create a new order
            Console.WriteLine("\nPress Enter to create a new order for {0}",
                customerId.ToUpper());
            Console.ReadLine();

            var newOrder = new Order
            {
                CustomerId = customerId,
                OrderDate = DateTime.Today,
                ShippedDate = DateTime.Today.AddDays(1),
                OrderDetails = new ChangeTrackingCollection<OrderDetail>
                    {
                        new OrderDetail { ProductId = 1, Quantity = 5, UnitPrice = 10 },
                        new OrderDetail { ProductId = 2, Quantity = 10, UnitPrice = 20 },
                        new OrderDetail { ProductId = 4, Quantity = 40, UnitPrice = 40 }
                    }
            };
            Order createdOrder = serviceAgent.CreateOrder(newOrder).Result;
            PrintOrderWithDetails(createdOrder);

            // Update the order
            Console.WriteLine("\nPress Enter to update order details");
            Console.ReadLine();

            // Start change-tracking the order
            var changeTracker = new ChangeTrackingCollection<Order>(createdOrder);

            // Modify order details
            createdOrder.OrderDetails[0].UnitPrice++;
            createdOrder.OrderDetails.RemoveAt(1);
            createdOrder.OrderDetails.Add(new OrderDetail
            {
                OrderId = createdOrder.OrderId,
                ProductId = 3,
                Quantity = 15,
                UnitPrice = 30
            });

            // Submit changes
            Order changedOrder = changeTracker.GetChanges().SingleOrDefault();
            Order updatedOrder = serviceAgent.UpdateOrder(changedOrder).Result;

            // Merge changes
            changeTracker.MergeChanges(updatedOrder);
            Console.WriteLine("Updated order:");
            PrintOrderWithDetails(createdOrder);

            // Delete the order
            Console.WriteLine("\nPress Enter to delete the order");
            Console.ReadLine();
            serviceAgent.DeleteOrder(createdOrder).Wait();

            // Verify order was deleted
            bool deleted = serviceAgent.VerifyOrderDeleted(createdOrder.OrderId).Result;
            Console.WriteLine(deleted ?
                "Order was successfully deleted" :
                "Order was not deleted");

            // Keep console open
            Console.WriteLine("Press any key to exit");
            Console.ReadKey(true);
        }

        private static void PrintCustomer(Customer c)
        {
            Console.WriteLine("{0} {1} {2} {3}",
                c.CustomerId,
                c.CompanyName,
                c.ContactName,
                c.City);
        }

        private static void PrintOrder(Order o)
        {
            Console.WriteLine("{0} {1}",
                o.OrderId,
                o.OrderDate.GetValueOrDefault().ToShortDateString());
        }

        private static void PrintOrderWithDetails(Order o)
        {
            Console.WriteLine("{0} {1}",
                o.OrderId,
                o.OrderDate.GetValueOrDefault().ToShortDateString());
            foreach (var od in o.OrderDetails)
            {
                Console.WriteLine("\t{0} {1} {2} {3}",
                    od.OrderDetailId,
                    od.Product.ProductName,
                    od.Quantity,
                    od.UnitPrice.ToString("c"));
            }
        }
    }
}
