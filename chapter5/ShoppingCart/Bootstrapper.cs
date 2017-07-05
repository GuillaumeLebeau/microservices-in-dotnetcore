using Nancy;
using Nancy.TinyIoc;

using ShoppingCart.ShoppingCart;

namespace ShoppingCart
{
    using global::ShoppingCart.EventFeed;

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private readonly string _connectionString;
        private readonly string _eventStoreConnectionString;
        private readonly string _eventStoreType;

        public Bootstrapper(string connectionString, string eventStoreconnectionString,  string eventStoreType)
        {
            _connectionString = connectionString;
            _eventStoreConnectionString = eventStoreconnectionString;
            _eventStoreType = eventStoreType;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register<IShoppingCartStore, ShoppingCartStore>(new ShoppingCartStore(_connectionString));

            switch (_eventStoreType)
            {
                case "Sql":
                    container.Register<IEventStore, SqlEventStore>(new SqlEventStore(_eventStoreConnectionString));
                    break;
                case "EventStore":
                    container.Register<IEventStore, EventStoreEventStore>(new EventStoreEventStore(_eventStoreConnectionString));
                    break;
                default:
                    break;
            }
        }
    }
}
