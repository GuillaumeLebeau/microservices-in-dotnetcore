using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using Dapper;

namespace ShoppingCart.ShoppingCart
{
    public class ShoppingCartStore : IShoppingCartStore
    {
        private readonly string _connectionString;

        public ShoppingCartStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<ShoppingCart> Get(int userId)
        {
            using (var conn = await GetOpenConnectionAsync())
            {
                var readItemsSql = $@"
SELECT item.ShoppingCartId AS ShoppingCartId,
       item.ProductCatalogId AS ProductCatalogId,
       item.ProductName AS ProductName,
       item.ProductDescription AS Description,
       item.Amount,
       item.Currency
  FROM shopcart.ShoppingCartItems AS item
       INNER JOIN shopcart.ShoppingCart AS cart ON item.ShoppingCartId = cart.ID
 WHERE cart.UserId = @UserId
";
                var items = await conn.QueryAsync<ShoppingCartItem>(readItemsSql, new { UserId = userId });
                return new ShoppingCart(userId, items);
            }
        }

        public async Task Save(ShoppingCart shoppingCart)
        {
            using (var conn = await GetOpenConnectionAsync())
            using (var tx = conn.BeginTransaction())
            {
                var deleteAllForShoppingCartSql = $@"
DELETE item
  FROM shopcart.ShoppingCartItems AS item
       INNER JOIN shopcart.ShoppingCart AS cart ON item.ShoppingCartId = cart.ID
       AND cart.UserId = @UserId
";
                await conn.ExecuteAsync(deleteAllForShoppingCartSql, new { UserId = shoppingCart.UserId }, tx).ConfigureAwait(false);

                var addAllForShoppingCartSql = $@"
INSERT INTO shopcart.ShoppingCartItems (ShoppingCartId, ProductCatalogId, ProductName, ProductDescription, Amount, Currency)
VALUES (@ShoppingCartId, @ProductCatalogId, @ProductName, v@ProductDescription, @Amount, @Currency)";
                await conn.ExecuteAsync(addAllForShoppingCartSql, shoppingCart.Items.Select(new { }), tx).ConfigureAwait(false);
            }
        }

        private async Task<IDbConnection> GetOpenConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            return connection;
        }
    }
}
