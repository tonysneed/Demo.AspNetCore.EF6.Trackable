using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using Demo.AspNetCore.EF6.Trackable.Entities.Shared.Net45.Models;
using TrackableEntities.Client;

namespace Demo.AspNetCore.EF6.Trackable.Client
{
    class Program
    {
        static void Main()
        {
            // TODO: Address for Web API service (replace port number, append .fiddler to localhost)
            const string serviceBaseAddress = "http://localhost.fiddler:" + "5000" + "/";
            var client = new HttpClient { BaseAddress = new Uri(serviceBaseAddress) };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var formatter = new JsonMediaTypeFormatter();

            // Get customers
            Console.WriteLine("Press Enter to get customers");
            Console.ReadLine();
            Console.WriteLine("Customers:");
            IEnumerable<Customer> customers = GetCustomers(client, formatter);
            if (customers == null) return;
            foreach (var c in customers)
                PrintCustomer(c);

            // Get orders for a customer
            Console.WriteLine("\nGet customer orders {CustomerId}:");
            string customerId = Console.ReadLine();
            if (!customers.Any(c => string.Equals(c.CustomerId, customerId, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Invalid customer id: {0}", customerId.ToUpper());
                return;
            }
            IEnumerable<Order> orders = GetCustomerOrders(client, customerId, formatter);
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
            Order order = GetOrder(client, orderId, formatter);
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
            var createdOrder = CreateOrder(client, newOrder, formatter);
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
            var changedOrder = changeTracker.GetChanges().SingleOrDefault();
            var updatedOrder = UpdateOrder(client, changedOrder, formatter);

            // Merge changes
            changeTracker.MergeChanges(updatedOrder);
            Console.WriteLine("Updated order:");
            PrintOrderWithDetails(createdOrder);

            // Delete the order
            Console.WriteLine("\nPress Enter to delete the order");
            Console.ReadLine();
            DeleteOrder(client, createdOrder);

            // Verify order was deleted
            var deleted = VerifyOrderDeleted(client, createdOrder.OrderId);
            Console.WriteLine(deleted ?
                "Order was successfully deleted" :
                "Order was not deleted");

            // Keep console open
            Console.WriteLine("Press any key to exit");
            Console.ReadKey(true);
        }

        private static IEnumerable<Customer> GetCustomers(HttpClient client, MediaTypeFormatter formatter)
        {
            const string request = "api/customers";
            var response = client.GetAsync(request).Result;
            response.EnsureSuccessStatusCode();
            var result = response.Content.ReadAsAsync<IEnumerable<Customer>>(new[] { formatter }).Result;
            return result;
        }

        private static IEnumerable<Order> GetCustomerOrders
            (HttpClient client, string customerId, MediaTypeFormatter formatter)
        {
            string request = "api/orders/" + customerId;
            var response = client.GetAsync(request).Result;
            response.EnsureSuccessStatusCode();
            var result = response.Content.ReadAsAsync<IEnumerable<Order>>(new[] { formatter }).Result;
            return result;
        }

        private static Order GetOrder(HttpClient client, int orderId, MediaTypeFormatter formatter)
        {
            string request = "api/orders/" + orderId;
            var response = client.GetAsync(request).Result;
            response.EnsureSuccessStatusCode();
            var result = response.Content.ReadAsAsync<Order>(new[] { formatter }).Result;
            return result;
        }

        private static Order CreateOrder(HttpClient client, Order order, MediaTypeFormatter formatter)
        {
            string request = "api/orders";
            //var response = client.PostAsync(new Uri(request, UriKind.Relative), order, formatter).Result;
            var response = client.PostAsJsonAsync(new Uri(request, UriKind.Relative), order).Result;
            response.EnsureSuccessStatusCode();
            var result = response.Content.ReadAsAsync<Order>(new[] { formatter }).Result;
            return result;
        }

        private static Order UpdateOrder(HttpClient client, Order order, MediaTypeFormatter formatter)
        {
            string request = "api/orders";
            var response = client.PutAsync(new Uri(request, UriKind.Relative), order, formatter).Result;
            response.EnsureSuccessStatusCode();
            var result = response.Content.ReadAsAsync<Order>().Result;
            return result;
        }

        private static void DeleteOrder(HttpClient client, Order order)
        {
            string request = "api/orders/" + order.OrderId;
            var response = client.DeleteAsync(request);
            response.Result.EnsureSuccessStatusCode();
        }

        private static bool VerifyOrderDeleted(HttpClient client, int orderId)
        {
            string request = "api/orders/" + orderId;
            var response = client.GetAsync(request).Result;
            if (response.IsSuccessStatusCode) return false;
            return true;
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
