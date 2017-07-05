using System;

namespace ShoppingCart.ProductCatalog
{
    public interface ICache
    {
        void Add(string key, object value, TimeSpan ttl);
        object Get(string key);
    }
}
