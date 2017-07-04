using System.Collections.Generic;
using System.Linq;

using ShoppingCart.EventFeed;

namespace ShoppingCart.ShoppingCart
{
    public class ShoppingCart
    {
        private HashSet<ShoppingCartItem> items = new HashSet<ShoppingCartItem>();

        public ShoppingCart(int userId)
        {
            this.UserId = userId;
        }

        public ShoppingCart(int userId, IEnumerable<ShoppingCartItem> items)
        {
            this.UserId = userId;
            foreach (var item in items)
            {
                this.items.Add(item);
            }
        }

        public int Id { get; }

        public int UserId { get; }

        public IEnumerable<ShoppingCartItem> Items
        {
            get { return items; }
        }

        public void AddItems(IEnumerable<ShoppingCartItem> shoppingCartItems, IEventStore eventStore)
        {
            foreach (var item in shoppingCartItems)
            {
                if (this.items.Add(item))
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
