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

        private const string readItemsSql = @"
SELECT cart.ID AS Id,
       cart.UserId
  FROM shopCart.ShoppingCart as cart

SELECT item.ProductCatalogId AS ProductCatalogId,
       item.ProductName AS ProductName,
       item.ProductDescription AS Description,
       item.Currency,
       item.Amount
  FROM shopcart.ShoppingCartItems AS item
       INNER JOIN shopcart.ShoppingCart AS cart ON item.ShoppingCartId = cart.ID
 WHERE cart.UserId = @UserId
";

        private const string deleteAllForShoppingCartSql = @"
DELETE item
  FROM shopcart.ShoppingCartItems AS item
       INNER JOIN shopcart.ShoppingCart AS cart ON item.ShoppingCartId = cart.ID
       AND cart.UserId = @UserId
";

        private const string deleteShoppingCartForUserSql = @"
DELETE FROM shopcart.ShoppingCart
WHERE UserId = @UserId
";

        private const string addShoppingCartForUserSql = @"
INSERT INTO shopcart.ShoppingCart (UserId)
OUTPUT Inserted.ID
VALUES (@UserId)
";

        private const string addAllForShoppingCartSql = @"
INSERT INTO shopcart.ShoppingCartItems (ShoppingCartId, ProductCatalogId, ProductName, ProductDescription, Amount, Currency)
VALUES (@ShoppingCartId, @ProductCatalogId, @ProductName, @ProductDescription, @Amount, @Currency)
";

        public ShoppingCartStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<ShoppingCart> Get(long userId)
        {
            using (var conn = await GetOpenConnectionAsync().ConfigureAwait(false))
            {
                using (var multi = await conn.QueryMultipleAsync(readItemsSql, new { UserId = userId }).ConfigureAwait(false))
                {
                    var shoppingCart = await multi.ReadSingleOrDefaultAsync<ShoppingCart>().ConfigureAwait(false);
                    var items = multi.Read<ShoppingCartItem, Money, ShoppingCartItem>((item, money) =>
                    {
                        item.Price = money;
                        return item;
                    },
                    splitOn: "Currency");

                    return new ShoppingCart((shoppingCart?.Id).GetValueOrDefault(0), userId, items);
                }
            }
        }

        public async Task Save(ShoppingCart shoppingCart)
        {
            using (var conn = await GetOpenConnectionAsync().ConfigureAwait(false))
            using (var tx = conn.BeginTransaction())
            {
                await conn.ExecuteAsync(deleteAllForShoppingCartSql, new { UserId = shoppingCart.UserId }, tx).ConfigureAwait(false);
                await conn.ExecuteAsync(deleteShoppingCartForUserSql, new { UserId = shoppingCart.UserId }, tx).ConfigureAwait(false);

                shoppingCart.Id = await conn.ExecuteScalarAsync<int>(addShoppingCartForUserSql, new { UserId = shoppingCart.UserId }, tx).ConfigureAwait(false);
                await conn.ExecuteAsync(addAllForShoppingCartSql, shoppingCart.Items.Select(i => new {
                    ShoppingCartId = shoppingCart.Id,
                    i.ProductCatalogId,
                    i.ProductName,
                    ProductDescription = i.Description,
                    Amount = i.Price.Amount,
                    Currency = i.Price.Currency
                }), tx).ConfigureAwait(false);

                tx.Commit();
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
