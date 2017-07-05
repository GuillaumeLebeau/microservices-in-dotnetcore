using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Polly;
using ShoppingCart.ShoppingCart;

namespace ShoppingCart.ProductCatalog
{
    public class ProductCatalogClient : IProductCatalogClient
    {
        // URL for the fake Product Catalog microservice
        private readonly string _productCatalogBaseUrl; // = "http://private-05cc8-chapter2productcataloguemicroservice.apiary-mock.com";

        private const string getProductPathTemplate = "/products?productIds=[{0}]";

        private readonly ICache _cache;

        // Use Polly's fluent API to set up a retry policy with an exponential back-off
        private static Policy exponentialRetryPolicy = Policy.Handle<Exception>()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)));

        public ProductCatalogClient(ICache cache, string productCatalogUrl)
        {
            _cache = cache;
            _productCatalogBaseUrl = productCatalogUrl;
        }

        public Task<IEnumerable<ShoppingCartItem>> GetShoppingCartItems(int[] productCatalogIds) =>
            // Wraps calls to the Product Catalog microservice in the retry policy
            exponentialRetryPolicy.ExecuteAsync(() => GetItemsFromCatalogService(productCatalogIds));

        private async Task<IEnumerable<ShoppingCartItem>> GetItemsFromCatalogService(int[] productCatalogIds)
        {
            var response = await RequestProductFromProductCatalog(productCatalogIds).ConfigureAwait(false);
            return await ConvertToShoppingCartItems(response).ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> RequestProductFromProductCatalog(int[] productCatalogIds)
        {
            // Adds the product IDs as a query string parameter to the path of the /products endpoint
            var productsResource = string.Format(getProductPathTemplate, string.Join(",", productCatalogIds));

            // Tries to retrieve a valid response from the cache
            var response = _cache.Get(productsResource) as HttpResponseMessage;

            // Only makes the HTTP request if there’s no response in the cache
            if (response == null)
            {
                // Creates a client for making the HTTP GET request
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_productCatalogBaseUrl);

                    // Tells HttpClient to perform HTTP GET asynchronously
                    response = await httpClient.GetAsync(productsResource).ConfigureAwait(false);

                    AddToCache(productsResource, response);
                }
            }

            return response;
        }

        private void AddToCache(string resource, HttpResponseMessage response)
        {
            // Reads the cache-control header from the response
            var cacheHeader = response.Headers.FirstOrDefault(h => h.Key.Equals("cache-control", StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(cacheHeader.Key))
                return;

            // Parses the cache-control value and extracts maxage from it
            var maxAge = CacheControlHeaderValue.Parse(cacheHeader.Value.FirstOrDefault()).MaxAge;
            if (maxAge.HasValue)
            {
                // Adds the response to the cache if it has a max-age value
                _cache.Add(key: resource, value: response, ttl: maxAge.Value);
            }
        }

        private static async Task<IEnumerable<ShoppingCartItem>> ConvertToShoppingCartItems(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            // Uses Json.NET to deserialize the JSON from the Product Catalog microservice
            var products = JsonConvert.DeserializeObject<List<ProductCatalogProduct>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

            // Creates a ShoppingCartItem for each product in the response
            return products.Select( p => new ShoppingCartItem(int.Parse(p.ProductId), p.ProductName, p.ProductDescription, p.Price));
        }

        // Use a private class to represent the product data
        private class ProductCatalogProduct
        {
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductDescription { get; set; }
            public Money Price { get; set; }
        }
    }
}
