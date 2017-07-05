using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EventStore.ClientAPI;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ShoppingCart.EventFeed
{
    public class EventStoreEventStore : IEventStore
    {
        private readonly string _connectionString;
        private const int MaxPageSize = 4096;

        public EventStoreEventStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Event>> GetEvents(long? firstEventSequenceNumber = null, long? lastEventSequenceNumber = null)
        {
            var start = firstEventSequenceNumber.GetValueOrDefault(0);
            var count = (int)(lastEventSequenceNumber.GetValueOrDefault(MaxPageSize) - start);

            if (count > MaxPageSize)
                count = MaxPageSize;
            else if (count < 0)
                count = 0;

            // Creates a connection to EventStore
            using (var connection = EventStoreConnection.Create(_connectionString))
            {
                await connection.ConnectAsync().ConfigureAwait(false);

                // Reads events from the Event Store
                var result = await connection.ReadStreamEventsForwardAsync(
                    "ShoppingCart",
                    start: start,
                    count: count,
                    resolveLinkTos: false).ConfigureAwait(false);

                var converter = new ExpandoObjectConverter();

                // Accesses the events on the result from the Event Store
                return result.Events
                    .Select(ev => new
                    {
                        Content = JsonConvert.DeserializeObject<ExpandoObject>(Encoding.UTF8.GetString(ev.Event.Data), converter),

                        // Deserializes the metadata part of each event
                        Metadata = JsonConvert.DeserializeObject<EventMetadata>(Encoding.UTF8.GetString(ev.Event.Metadata))
                    })
                    // Maps to events from Event Store Event objects
                    .Select((ev, i) => new Event(i + start, ev.Metadata.OccurredAt, ev.Metadata.EventName, ev.Content));
            }
        }

        public async Task Raise(string eventName, object content)
        {
            using (var connection = EventStoreConnection.Create(_connectionString))
            {
                // Opens the connection to EventStore
                await connection.ConnectAsync().ConfigureAwait(false);
                var contentJson = JsonConvert.SerializeObject(content);

                // Maps OccurredAt and EventName to metadata to be stored along with the event
                var metaDataJson = JsonConvert.SerializeObject(new EventMetadata
                {
                    OccurredAt = DateTimeOffset.Now,
                    EventName = eventName
                });

                // EventData is EventStore’s representation of an event
                var eventData = new EventData(
                    Guid.NewGuid(),
                    "ShoppingCartEvent",
                    isJson: true,
                    data: Encoding.UTF8.GetBytes(contentJson),
                    metadata: Encoding.UTF8.GetBytes(metaDataJson));

                // Writes the event to EventStore
                await connection.AppendToStreamAsync("ShoppingCart", ExpectedVersion.Any, eventData).ConfigureAwait(false);
            }
        }

        private class EventMetadata
        {
            public DateTimeOffset OccurredAt { get; set; }
            public string EventName { get; set; }
        }
    }
}
