namespace ShoppingCart.ShoppingCart
{
    public class ShoppingCartItem
    {
        public ShoppingCartItem()
        {
        }

        public ShoppingCartItem(int productCatalogId, string productName, string description, Money price)
        {
            this.ProductCatalogId = productCatalogId;
            this.ProductName = productName;
            this.Description = description;
            this.Price = price;
        }

        public int ProductCatalogId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public Money Price { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var that = obj as ShoppingCartItem;
            return this.ProductCatalogId.Equals(that.ProductCatalogId);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return this.ProductCatalogId.GetHashCode();
        }
    }
}
