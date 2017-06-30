namespace SpecialOffers.SpecialOffers
{
    public interface ISpecialOffersStore
    {
        SpecialOffer Get(int offerId);

        void Add(SpecialOffer specialOffer);

        void Update(SpecialOffer specialOffer);

        void Save();
    }
}
