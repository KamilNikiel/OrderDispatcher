using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace OrderDispatcher.Core.Database
{
    public class OrderDispatcherDbContext : SagaDbContext
    {
        public OrderDispatcherDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 
            
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDispatcherDbContext).Assembly);
        }

        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get { yield return new OrderDispatcherSagaStateMap(); }
        }
    }
}