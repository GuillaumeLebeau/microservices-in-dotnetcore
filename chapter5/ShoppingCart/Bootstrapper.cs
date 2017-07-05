using Nancy;
using Nancy.TinyIoc;

using ShoppingCart.EventFeed;
using ShoppingCart.ShoppingCart;

namespace ShoppingCart
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private readonly string _connectionString;

        public Bootstrapper(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register<IShoppingCartStore, ShoppingCartStore>(new ShoppingCartStore(_connectionString));
            container.Register<IEventStore, EventStore>(new EventStore(_connectionString));
        }
    }
}
