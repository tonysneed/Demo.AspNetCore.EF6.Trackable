using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace Demo.AspNetCore.EF6.Trackable.Data.Contexts
{
    public class NorthwindSlimConfig : DbConfiguration
    {
        public NorthwindSlimConfig()
        {
            SetProviderServices("System.Data.SqlClient", SqlProviderServices.Instance);
        }
    }
}
