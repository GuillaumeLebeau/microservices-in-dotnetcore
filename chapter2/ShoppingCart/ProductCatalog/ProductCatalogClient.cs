using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ShoppingCart.ShoppingCart;

namespace ShoppingCart.ProductCatalog
{
    public class ProductCatalogClient : IProductCatalogClient
    {
        private static string productCatalogBaseUrl = @"http://private-05cc8-chapter2productcataloguemicroservice.apiary-mock.com";
        private static string getProductPathTemplate = "/products?productIds=[{0}]";

        public Task<IEnumerable<ShoppingCartItem>> GetShoppingCartItems(int[] productCatalogIds)
        {
            throw new NotImplementedException();
        }

        private async Task<IEnumerable<ShoppingCartItem>> GetItemsFromCatalogService(int[] productCatalogIds)
        {
            var response = await RequestProductFromProductCatalog(productCatalogIds).ConfigureAwait(false);
            return await ConvertToShoppingCartItems(response).ConfigureAwait(false);
        }

        private static async Task<HttpResponseMessage> RequestProductFromProductCatalog(int[] productCatalogIds)
        {
            var productsResource = string.Format(getProductPathTemplate, string.Join(",", productCatalogIds));
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(productCatalogBaseUrl);
                return await httpClient.GetAsync(productsResource).ConfigureAwait(false);
            }
        }

        private static async Task<IEnumerable<ShoppingCartItem>> ConvertToShoppingCartItems(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var products = JsonConvert.DeserializeObject<List<ProductCatalogProduct>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            return products.Select( p => new ShoppingCartItem(int.Parse(p.ProductId), p.ProductName, p.ProductDescription, p.Price));
        }

        private class ProductCatalogProduct
        {
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductDescription { get; set; }
            public Money Price { get; set; }
        }
    }
}