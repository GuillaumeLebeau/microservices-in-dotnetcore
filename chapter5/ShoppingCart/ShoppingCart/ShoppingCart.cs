using System.Collections.Generic;
using System.Linq;

using ShoppingCart.EventFeed;

namespace ShoppingCart.ShoppingCart
{
    public class ShoppingCart
    {
        private readonly HashSet<ShoppingCartItem> items = new HashSet<ShoppingCartItem>();

        public ShoppingCart(int id, long userId)
        {
            Id = id;
            UserId = userId;
        }

        public ShoppingCart(int id, long userId, IEnumerable<ShoppingCartItem> items)
        {
            Id = id;
            UserId = userId;

            foreach (var item in items)
            {
                this.items.Add(item);
            }
        }

        public int Id { get; set; }

        public long UserId { get; }

        public IEnumerable<ShoppingCartItem> Items
        {
            get { return items; }
        }

        public void AddItems(IEnumerable<ShoppingCartItem> shoppingCartItems, IEventStore eventStore)
        {
            foreach (var item in shoppingCartItems)
            {
                if (items.Add(item))
                {
                    // Raises an event through the eventStore for each item added
                    eventStore.Raise("ShoppingCartItemAdded", new { UserId, item });
                }
            }
        }

        public void RemoveItems(int[] productCatalogIds, IEventStore eventStore)
        {
            foreach (var item in items.Where(i => productCatalogIds.Contains(i.ProductCatalogId)).ToList())
            {
                if (items.Remove(item))
                {
                    // Raises an event through the eventStore for each item removed
                    eventStore.Raise("ShoppingCartItemRemoved", new { UserId, item });
                }
            }
        }
    }
}
