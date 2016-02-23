using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Demo.AspNetCore.EF6.Trackable.Entities.Shared.Net45.Models;

namespace Demo.AspNetCore.EF6.Trackable.Client
{
    public class NorthwindServiceAgent : INorthwindServiceAgent
    {
        private readonly HttpClient _client;
        private readonly MediaTypeFormatter _formatter;

        public NorthwindServiceAgent(string baseAddress, MediaTypeFormatter formatter)
        {
            _client = new HttpClient { BaseAddress = new Uri(baseAddress) };
            _formatter = formatter;
        }

        public async Task<IList<Customer>> GetCustomers()
        {
            string request = "api/customers";
            var response = await _client.GetAsync(request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<IList<Customer>>();
            return result;
        }

        public async Task<IList<Order>> GetCustomerOrders(string customerId)
        {
            string request = "api/orders/" + customerId;
            var response = await _client.GetAsync(request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<IList<Order>>();
            return result;
        }

        public async Task<Order> GetOrder(int orderId)
        {
            string request = "api/orders/" + orderId;
            var response = await _client.GetAsync(request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<Order>();
            return result;
        }

        public async Task<Order> CreateOrder(Order order)
        {
            string request = "api/orders";
            var response = await _client.PostAsync(new Uri(request, UriKind.Relative), order, _formatter);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<Order>();
            return result;
        }

        public async Task<Order> UpdateOrder(Order order)
        {
            string request = "api/orders";
            var response = await _client.PutAsync(new Uri(request, UriKind.Relative), order, _formatter);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<Order>();
            return result;
        }

        public async Task DeleteOrder(Order order)
        {
            string request = "api/orders/" + order.OrderId;
            var response = await _client.DeleteAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> VerifyOrderDeleted(int orderId)
        {
            string request = "api/orders/" + orderId;
            var response = await _client.GetAsync(request);
            if (response.IsSuccessStatusCode) return false;
            return true;
        }
    }
}
