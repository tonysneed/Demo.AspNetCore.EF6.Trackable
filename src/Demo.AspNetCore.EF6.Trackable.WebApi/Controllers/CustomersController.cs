using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using Demo.AspNetCore.EF6.Trackable.Data.Contexts;
using Demo.AspNetCore.EF6.Trackable.Entities.Shared.Net45.Models;
using Microsoft.AspNet.Mvc;

namespace Demo.AspNetCore.EF6.Trackable.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class CustomersController : Controller
    {
        private readonly NorthwindSlim _dbContext;

        public CustomersController(NorthwindSlim dbContext)
        {
            _dbContext = dbContext;
        }

        // GET api/customers
        [HttpGet]
        public async Task<ObjectResult> GetCustomers()
        {
            IEnumerable<Customer> customers = await _dbContext.Customers
                .ToListAsync();

            return Ok(customers);
        }

        // GET api/customers/ABCD
        [HttpGet("{id}")]
        public async Task<ObjectResult> GetCustomers(string id)
        {
            Customer customer = await _dbContext.Customers
                .SingleOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
            {
                return HttpNotFound(null);
            }

            return Ok(customer);
        }
    }
}
