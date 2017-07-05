using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShoppingCart.EventFeed
{
    public interface IEventStore
    {
        Task<IEnumerable<Event>> GetEvents(long? firstEventSequenceNumber = null, long? lastEventSequenceNumber = null);

        Task Raise(string eventName, object content);
    }
}
