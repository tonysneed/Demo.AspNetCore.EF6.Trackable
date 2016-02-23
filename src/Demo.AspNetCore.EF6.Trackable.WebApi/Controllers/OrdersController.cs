using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using Demo.AspNetCore.EF6.Trackable.Data.Contexts;
using Demo.AspNetCore.EF6.Trackable.Entities.Shared.Net45.Models;
using Microsoft.AspNet.Mvc;
using TrackableEntities;
using TrackableEntities.Common;
using TrackableEntities.EF6;

namespace Demo.AspNetCore.EF6.Trackable.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        private readonly NorthwindSlim _dbContext;

        public OrdersController(NorthwindSlim dbContext)
        {
            _dbContext = dbContext;
        }

        // GET api/orders?customerId=ABCD
        [HttpGet("{customerId}")]
        public async Task<ObjectResult> GetOrders(string customerId)
        {
            IEnumerable<Order> orders = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include("OrderDetails.Product")
                .Where(o => o.CustomerId == customerId)
                .ToListAsync();

            return Ok(orders);
        }

        // GET api/orders/5
        [HttpGet("{id:int}")]
        public async Task<ObjectResult> GetOrders(int id)
        {
            Order order = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include("OrderDetails.Product")
                .SingleOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound(null);
            }

            return Ok(order);
        }

        // POST api/Order
        [HttpPost]
        public async Task<ObjectResult> PostOrder([FromBody]Order order)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            order.TrackingState = TrackingState.Added;
            _dbContext.ApplyChanges(order);

            await _dbContext.SaveChangesAsync();

            await _dbContext.LoadRelatedEntitiesAsync(order);
            order.AcceptChanges();
            return CreatedAtRoute(new { id = order.OrderId }, order);
        }

        // PUT api/Order
        [HttpPut]
        public async Task<ObjectResult> PutOrder([FromBody]Order order)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _dbContext.ApplyChanges(order);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_dbContext.Orders.Any(o => o.OrderId == order.OrderId))
                {
                    return HttpBadRequest(order.OrderId);
                }
                throw;
            }

            await _dbContext.LoadRelatedEntitiesAsync(order);
            order.AcceptChanges();
            return Ok(order);
        }

        // DELETE api/Order/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            Order order = await _dbContext.Orders
                .Include(o => o.OrderDetails)
                .SingleOrDefaultAsync(o => o.OrderId == id);
            if (order == null)
            {
                return Ok();
            }

            order.TrackingState = TrackingState.Deleted;
            _dbContext.ApplyChanges(order);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_dbContext.Orders.Any(o => o.OrderId == order.OrderId))
                {
                    return HttpBadRequest(id);
                }
                throw;
            }

            return Ok();
        }
    }
}
