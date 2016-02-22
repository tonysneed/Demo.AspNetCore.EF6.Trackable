using System.Data.Entity;
using Demo.AspNetCore.EF6.Trackable.Entities.Shared.Net45.Models;

namespace Demo.AspNetCore.EF6.Trackable.Data.Contexts
{
    [DbConfigurationType(typeof(NorthwindSlimConfig))]
    public partial class NorthwindSlim : DbContext
    {
        static NorthwindSlim()
        {
            Database.SetInitializer(new NorthwindSlimDatabaseInitializer());
        }

        public NorthwindSlim(string connection)
            : base(connection)
        {
            Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>()
                .Property(e => e.CustomerId)
                .IsFixedLength();

            modelBuilder.Entity<Order>()
                .Property(e => e.CustomerId)
                .IsFixedLength();

            modelBuilder.Entity<Order>()
                .Property(e => e.Freight)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Order>()
                .HasMany(e => e.OrderDetails)
                .WithRequired(e => e.Order)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrderDetail>()
                .Property(e => e.UnitPrice)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Product>()
                .Property(e => e.UnitPrice)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Product>()
                .Property(e => e.RowVersion)
                .IsFixedLength();

            modelBuilder.Entity<Product>()
                .HasMany(e => e.OrderDetails)
                .WithRequired(e => e.Product)
                .WillCascadeOnDelete(false);

            ModelCreating(modelBuilder);
        }

        partial void ModelCreating(DbModelBuilder modelBuilder);
    }
}
