using System.Threading.Tasks;

namespace ShoppingCart.ShoppingCart
{
    public interface IShoppingCartStore
    {
         Task<ShoppingCart> Get(long userId);

         Task Save(ShoppingCart shoppingCart);
    }
}
