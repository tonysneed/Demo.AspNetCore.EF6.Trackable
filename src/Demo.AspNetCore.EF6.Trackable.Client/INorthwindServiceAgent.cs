using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.AspNetCore.EF6.Trackable.Entities.Shared.Net45.Models;

namespace Demo.AspNetCore.EF6.Trackable.Client
{
    public interface INorthwindServiceAgent
    {
        Task<Order> CreateOrder(Order order);
        Task DeleteOrder(Order order);
        Task<IList<Order>> GetCustomerOrders(string customerId);
        Task<IList<Customer>> GetCustomers();
        Task<Order> GetOrder(int orderId);
        Task<Order> UpdateOrder(Order order);
        Task<bool> VerifyOrderDeleted(int orderId);
    }
}