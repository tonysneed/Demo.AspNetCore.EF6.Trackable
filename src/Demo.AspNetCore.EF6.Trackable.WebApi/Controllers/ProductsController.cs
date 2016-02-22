using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using Demo.AspNetCore.EF6.Trackable.Data.Contexts;
using Microsoft.AspNet.Mvc;
using System.Linq;

namespace Demo.AspNetCore.EF6.Trackable.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        private readonly NorthwindSlim _dbContext;

        public ProductsController(NorthwindSlim dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ObjectResult> Get()
        {
            var products = await _dbContext.Products
                .OrderBy(e => e.ProductName)
                .ToListAsync();
            return Ok(products);
        }
    }
}
