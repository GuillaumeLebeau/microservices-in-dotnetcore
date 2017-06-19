using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ShoppingCart.ShoppingCart;

namespace ShoppingCart.ProductCatalog
{
    public class ProductCatalogClient : IProductCatalogClient
    {
        private static string productCatalogBaseUrl = @"http://private-05cc8-chapter2productcataloguemicroservice.apiary-mock.com";
        private static string getProductPathTemplate = "/products?productIds=[{0}]";

        public Task<IEnumerable<ShoppingCartItem>> GetShoppingCartItems(int[] productCatalogueIds)
        {
            throw new NotImplementedException();
        }

        private static async Task<HttpResponseMessage> RequestProductFromProductCatalogue(int[] productCatalogueIds)
        {
            var productsResource = string.Format(getProductPathTemplate, string.Join(",", productCatalogueIds));
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(productCatalogBaseUrl);
                return await httpClient.GetAsync(productsResource).ConfigureAwait(false);
            }
        }
    }
}