using System.Collections.Generic;
using System.Linq;
using SpecialOffers.EventFeed;

namespace SpecialOffers.SpecialOffers
{
    public class SpecialOffersStore : ISpecialOffersStore
    {
        private static readonly IDictionary<int, SpecialOffer> _database = new Dictionary<int, SpecialOffer>();
        private readonly IEventStore _eventStore;

        public SpecialOffersStore(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public void Add(SpecialOffer specialOffer)
        {
            var offerId = _database.Keys.Any() ? _database.Keys.Max() + 1 : 1;
            specialOffer.Id = offerId;
            _database.Add(offerId, specialOffer);
            _eventStore.Raise("NewSpecialOffer", specialOffer);
        }

        public SpecialOffer Get(int offerId)
        {
            _database.TryGetValue(offerId, out SpecialOffer offer);
            return offer;
        }

        public void Save()
        {
            // Nothing needed. Saving would be needed with a real DB
        }

        public void Update(SpecialOffer specialOffer)
        {
            _database[specialOffer.Id] = specialOffer;
            _eventStore.Raise("UpdatedSpecialOffer", specialOffer);
        }
    }
}
