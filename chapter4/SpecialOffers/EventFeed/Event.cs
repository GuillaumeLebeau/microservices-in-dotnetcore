using System;

namespace SpecialOffers.EventFeed
{
    public struct Event
    {
        public Event(long sequenceNumber, DateTimeOffset occuredAt, string name, object content)
        {
            this.SequenceNumber = sequenceNumber;
            this.OccuredAt = occuredAt;
            this.Name = name;
            this.Content = content;
        }

        public long SequenceNumber { get; }
        public DateTimeOffset OccuredAt { get; }
        public string Name { get; }
        public object Content { get; }
    }
}
