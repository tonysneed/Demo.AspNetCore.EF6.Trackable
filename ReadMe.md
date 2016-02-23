# Using Trackable Entities and EF 6 with ASP.NET Core 1.0

*NOTE: If you have not done so already, install the Visual Studio 2015 extension 
for Trackable Entities by selecting **Extensions and Updates** from the **Tools** menu 
and searching online for **Trackable Entities**.*

*You shoud also install the latest version of **MS SQL Express** and create a new database 
called **NorthwindSlim**, then run the SQL script in* http://bit.ly/northwindslim.

## Part A: Set Up Trackable Entities

1. Start with a new C# web app using ASP.NET 5 (Core 1.0)
    - Select the Web API template (preview)
    - Append **.WebApi** to the web project name.

2. Right-click on the **src** folder in the Solution Explorer and select **Add New Project**.
    - From the *Trackable* category select **Trackable Entities Class Library**.
    - For the name enter the solution name (for example, Demo.AspNetCore.EF6.Trackable)
    - When the Entities Selection dialog appears, first select **Shared**, then select **.Net 4.5**
        + You may also select other kinds of entities.
    - You will need to remove the project from the solution, then use **File Explorer** to manually move it to the **src** folder
        + After moving the project, you should right-click **src** in the Solution Explorer and select 
          **Add Existing Project** to add the *.Entities.Shared.Net45* project.
    - Update the package references for the entities project by opening the *.csproj* file in a text editor
      and adding "..\" to the path for each package reference.
    - Update the TE package for EF 6 to the latest version: `install-package TrackableEntities.EF.6`

3. Right-click the *Entities* project and select **Add New Item**, then from the **Data** category 
   select **ADO.NET Entity Data Model** and provide the name *NorthwindSlim*.
    - Select *Code First from Database*
    - Add a new *Data Connection* for *.\sqlexpress* and select the NorthwindSlim database.
    - Select the following tables:
        + Category, Customer, Order, OrderDetail, Product
    - Move the entity classes into the Models folder and refactor the namespace to include .Models
   
4. Add a new project to the solution with .Data appended to the name
    - For example: Demo.AspNetCore.EF6.Trackable.Data
    - Re-target the project to .NET 4.5.1.
    - Remove the project from the solution and use File Manager to manually move it to the **src** folder.
    - Right-click the **src** folder in the Solution Explorer, select **Add Existing Project** and add the .Data project
    - Install the TE packages Client and EF 6
    - Add a reference from the .Data project to the .Entities project
    - Move the Contexts folder, with the NorthwindSlim.cs file, from .Entities to .Data,
      then delete the Contexts folder from the .Entities project.
    - Fix the namespace for the NortwhindSlim.cs file in the .Data project and re-compile.
    - Uninstall the following packages from the .Entities project: TrackableEntities.EF.6, EntityFramework
    
5. Add a `NorthwindSlimDatabaseInitializer` class to the Contexts folder of the .Data project.
    - Extend `DropCreateDatabaseIfModelChanges<NorthwindSlim>`
    - Override the `Seed` method to add sample data for Categories, Products, Customers and Orders.
    - Update the call to `Database.SetInitializer` in the static ctor of *Northwind.cs* to 
      pass a new `NorthwindSlimDatabaseInitializer` instead of a `NullDatabaseInitializer`.
    - Update the `NorthwindSlim` ctor to accept a `string connection` parameter and pass it to `base`.
    - Add a public `NorthwindSlimConfig` class to the Contexts folder of the .Data project.
        + Extend `DbConfiguration`
        + Add a ctor in which you call `SetProviderServices`, passing "System.Data.SqlClient" and `SqlProviderServices.Instance`.
        + Add a `DbConfigurationType` attribute to `NorthwindSlim`, passing `typeof(NorthwindSlimConfig)`.
        
## Part B: Set Up ASP.NET Core Web API for TE and EF 6

1. Edit the *project.json* in the .WebApi project.
    - Remove "dnxcore50" from "frameworks".
    - Add "EntityFramework" to "dependencies", specifying the latest version for EF 6.x.
    
2. Add a "Data" section to appsettings.json with a connection string
    - Here we specify LocalDb, but SQL Express or full is OK too.

    ```json
    "Data": {
      "NorthwindSlim": {
        "ConnectionString": "Data Source=(localdb)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\NorthwindSlim.mdf;Integrated Security=True; MultipleActiveResultSets=True"
      }
    }
    ```

3. Update the `Startup` ctor to set the "DataDirectory" for the current `AppDomain`.
    - Add an "App_Data" directory to the .WebApi project.

    ```csharp
    public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
    {
        // Set up configuration sources.
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables();
        Configuration = builder.Build();

        // Set up data directory
        string appRoot = appEnv.ApplicationBasePath;
        AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(appRoot, "App_Data"));
    }
    ```

4. Register `NorthwindSlim` with DI system by supplying a new instance of `NorthwindSlim`.
    - Add references from the .WebApi project to both the .Data and .Entities projects.
    - Add "TrackableEntities.EF.6" and "TrackableEntities.Client" to "dependencies" in project.json.
    - Add the following code to the `ConfigureServices` method in `Startup`

    ```csharp
    services.AddScoped(provider =>
    {
        var connectionString = Configuration["Data:NorthwindSlim:ConnectionString"];
        return new NorthwindSlim(connectionString);
    });
    ```

5. Rename `ValuesController` to `ProductsController`.
    - Remove all methods except the first `Get` method.
    - Add a ctor that accepts a `dbContext` parameter of type `NorthwindSlim` 
      and initialized a `_dbContext` field.
    - Refactor the `Get` method to asynchronously return products sorted by name.
    
    ```csharp
    // GET: api/products
    [HttpGet]
    public async Task<ObjectResult> Get()
    {
        var products = await _dbContext.Products
            .OrderBy(e => e.ProductName)
            .ToListAsync();
        return Ok(products);
    }
    ```
    
6. Add a `CustomersController` to the .WebApi project.
    - Remove all the methods
    - Add a ctor that accepts a `dbContext` parameter of type `NorthwindSlim` 
      and initialized a `_dbContext` field.
    - Add two Get methods for retrieving all customers and a specific customer by id.
    
    ```csharp
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
    public async Task<ObjectResult> GetCustomer(string id)
    {
        Customer customer = await _dbContext.Customers
            .SingleOrDefaultAsync(c => c.CustomerId == id);

        if (customer == null)
        {
            return HttpNotFound(null);
        }

        return Ok(customer);
    }
    ```

7. Test the controller by running the app and submitting some requests.
    - Add a dependency for "Microsoft.AspNet.Diagnostics" to project.json.
    - Call `app.UseWelcomePage()` at the end in the `Startup.Configure` method.
    - Update launchUrl in launchSettings.json to "".
    - Press **Ctrl+F5** to start the .WebApi project.
    - You can also execute `dnx web` from a command line.
    - Use Postman or Fiddler.
    - The database should be created automatically
    
    ```
    GET: http://localhost:5000/api/products
    GET: http://localhost:5000/api/customers
    GET: http://localhost:5000/api/customers/ALFKI
    ```

8. Add an Orders controller with actions for GET, POST, PATCH and DELETE
    - Add an assembly reference to *System.Data*.
        + It will be added to "frameworkAssemblies" in project.json.
        
    ```csharp
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
    ```

9. Add a .Client console project to the **src** directory.
    - Set the target framework to .NET 4.5.1.
    - Add the following packages:
        + Microsoft.AspNet.WebApi.Client
        + TrackableEntities.Common
        + TrackableEntities.Client
    - Add a reference to the .Entities project.

10. Add an INorthwindServiceAgent interface.

    ```csharp
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
    ```

11. Implement INorthwindServiceAgent, passing a base address and 
    MediaTypeFormatter to the ctor.
    - Initialize an HttpClient using the base address
    - Submit HTTP requests to the server
    - Specify the media type formatter with POST and PUT requests

