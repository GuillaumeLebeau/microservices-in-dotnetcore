using System.Collections.Generic;
using System.Linq;

using Nancy;

namespace ProductCatalog
{
    public class ProductsModule : NancyModule
    {
        public ProductsModule(IProductStore productStore)
            : base("/products")
        {
            Get("/", _ =>
            {
                string productIdsString = Request.Query.productIds;
                var productIds = ParseProductIdsFromQueryString(productIdsString);
                var products = productStore.GetProductsByIds(productIds);

                return Negotiate
                    .WithModel(products)
                    .WithHeader("cache-control", "max-age=86400");
            });
        }

        private static IEnumerable<int> ParseProductIdsFromQueryString(string productIdsString)
        {
            return productIdsString.Split(',').Select(s => s.Replace("[", "").Replace("]", "")).Select(int.Parse);
        }
    }

    public interface IProductStore
    {
        IEnumerable<ProductCatalogProduct> GetProductsByIds(IEnumerable<int> productIds);
    }

    public class StaticProductStore : IProductStore
    {
        public IEnumerable<ProductCatalogProduct> GetProductsByIds(IEnumerable<int> productIds)
        {
            return productIds.Select(id => new ProductCatalogProduct(id, "foo" + id.ToString(), "bar", new Money("EUR", 40.3M * id)));
        }
    }

    public class ProductCatalogProduct
    {
        public ProductCatalogProduct(int productId, string productName, string description, Money price)
        {
            ProductId = productId.ToString();
            ProductName = productName;
            ProductDescription = description;
            Price = price;
        }

        public string ProductId { get; }
        public string ProductName { get; }
        public string ProductDescription { get; }
        public Money Price { get; }
    }

    public class Money
    {
        public Money(string currency, decimal amount)
        {
            this.Currency = currency;
            this.Amount = amount;
        }

        public string Currency { get; }

        public decimal Amount { get; }
    }
}
