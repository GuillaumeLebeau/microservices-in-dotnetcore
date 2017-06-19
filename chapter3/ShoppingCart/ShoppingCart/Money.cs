namespace ShoppingCart.ShoppingCart
{
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
