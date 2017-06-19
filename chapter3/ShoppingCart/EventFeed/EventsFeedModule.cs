using Nancy;

namespace ShoppingCart.EventFeed
{
    public class EventsFeedModule : NancyModule
    {
        public EventsFeedModule(IEventStore eventStore)
            : base("/events")
        {
            Get("/", _ =>
            {
                // Reads the start and end values from a query string parameter
                if (!long.TryParse(this.Request.Query.start.Value, out long firstEventSequenceNumber))
                    firstEventSequenceNumber = 0;

                if (!long.TryParse(this.Request.Query.end.Value, out long lastEventSequenceNumber))
                    lastEventSequenceNumber = long.MaxValue;

                // Returns the raw list of events.
                // Nancy takes care of serializing the events into the response body.
                return eventStore.GetEvents(firstEventSequenceNumber, lastEventSequenceNumber);
            });
        }
    }
}
