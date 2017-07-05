using Nancy;
using Nancy.TinyIoc;

using ShoppingCart.ShoppingCart;

namespace ShoppingCart
{
    using global::ShoppingCart.EventFeed;
    using global::ShoppingCart.ProductCatalog;
    using Microsoft.Extensions.Configuration;

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private readonly IConfigurationRoot _configuration;

        public Bootstrapper(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register<IShoppingCartStore, ShoppingCartStore>(new ShoppingCartStore(_configuration["ConnectionString"]));
            container.Register<IProductCatalogClient, ProductCatalogClient>(new ProductCatalogClient(container.Resolve<ICache>(), _configuration["ProductCatalogUrl"]));

            switch (_configuration["EventStoreType"])
            {
                case "Sql":
                    container.Register<IEventStore, SqlEventStore>(new SqlEventStore(_configuration["EventStoreConnectionString"]));
                    break;
                case "EventStore":
                    container.Register<IEventStore, EventStoreEventStore>(new EventStoreEventStore(_configuration["EventStoreConnectionString"]));
                    break;
                default:
                    break;
            }
        }
    }
}
