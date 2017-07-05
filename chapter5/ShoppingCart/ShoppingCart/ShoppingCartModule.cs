namespace ShoppingCart.ShoppingCart
{
    using global::ShoppingCart.EventFeed;
    using global::ShoppingCart.ProductCatalog;
    using Nancy;
    using Nancy.ModelBinding;

    public class ShoppingCartModule : NancyModule
    {
        public ShoppingCartModule(IShoppingCartStore shoppingCartStore,
            IProductCatalogClient productCatalog,
            IEventStore eventStore)
            : base("/shoppingcart")
        {
            Get("/{userid:long}", parameters =>
            {
                var userId = (long)parameters.userid;
                return shoppingCartStore.Get(userId);
            });

            Post("/{userid:long}/items", async parameters =>
            {
                var productcatalogIds = this.Bind<int[]>();
                var userId = (long)parameters.userid;

                var shoppingCart = await shoppingCartStore.Get(userId).ConfigureAwait(false);
                var shoppingCartItems = await productCatalog.GetShoppingCartItems(productcatalogIds).ConfigureAwait(false);
                shoppingCart.AddItems(shoppingCartItems, eventStore);
                await shoppingCartStore.Save(shoppingCart).ConfigureAwait(false);

                return shoppingCart;
            });

            Delete("/{userid:long}/items", async parameters =>
            {
                var productcatalogIds = this.Bind<int[]>();
                var userId = (long)parameters.userid;

                var shoppingCart = await shoppingCartStore.Get(userId).ConfigureAwait(false);
                shoppingCart.RemoveItems(productcatalogIds, eventStore);
                await shoppingCartStore.Save(shoppingCart).ConfigureAwait(false);

                return shoppingCart;
            });
        }
    }
}
