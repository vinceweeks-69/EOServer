using Autofac;
using EO.Persistence;
using InventoryServiceLayer.Interface;

namespace InventoryServiceLayer.Implementation
{
    public class InventoryServiceLayerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<InventoryManager>().As<IInventoryManager>();
            builder.Register(c => new InventoryManager(c.Resolve<IEOPersistence>()));
        }
    }
}
