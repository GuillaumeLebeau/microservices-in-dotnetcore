using Microsoft.Extensions.Configuration;

using Nancy;
using Nancy.TinyIoc;

using ShoppingCart.ShoppingCart;

namespace ShoppingCart
{
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
        }
    }
}
