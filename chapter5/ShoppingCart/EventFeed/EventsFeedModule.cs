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
                long? firstEvent = null;
                if (long.TryParse(Request.Query.start.Value, out long firstEventSequenceNumber))
                    firstEvent = firstEventSequenceNumber;

                long? lastEvent = null;
                if (long.TryParse(Request.Query.end.Value, out long lastEventSequenceNumber))
                    lastEvent = lastEventSequenceNumber;

                // Returns the raw list of events.
                // Nancy takes care of serializing the events into the response body.
                return eventStore.GetEvents(firstEvent, lastEvent);
            });
        }
    }
}
