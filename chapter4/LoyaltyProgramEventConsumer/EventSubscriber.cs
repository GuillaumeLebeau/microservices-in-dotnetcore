using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace LoyaltyProgramEventConsumer
{
    public class EventSubscriber
    {
        private readonly string _specialOffersHost;
        private long _start = 0;
        private readonly int _chunkSize = 100;
        private readonly Timer _timer;

        public EventSubscriber(string specialOffersHost)
        {
            _specialOffersHost = specialOffersHost;

            // Sets up the timer and the callback
            _timer = new Timer(SubscriptionCycleCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Start()
        {
            _timer.Change(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(-1));
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private async void SubscriptionCycleCallback(object state)
        {
            // Awaits the HTTP GET to the event feed
            var response = await ReadEvents().ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.OK)
                HandleEvents(await response.Content.ReadAsStringAsync());

            Start();
        }

        private async Task<HttpResponseMessage> ReadEvents()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(_specialOffersHost);
                
                // Awaits getting new events
                // Uses query parameters to limit the number of events read
                var response = await httpClient.GetAsync($"/events/?start={_start}&end={_start + _chunkSize}").ConfigureAwait(false);
                return response;
            }
        }

        private void HandleEvents(string content)
        {
            var events = JsonConvert.DeserializeObject<IEnumerable<SpecialOfferEvent>>(content);
            foreach (var ev in events)
            {
                // Treats the content property as a dynamic object
                dynamic eventData = ev.Content;

                // Keeps tracks of the highest event number handled
                _start = Math.Max(_start, ev.SequenceNumber + 1);
            }
        }
    }

    public struct SpecialOfferEvent
    {
        public long SequenceNumber { get; set; }
        public string Name { get; set; }
        public object Content { get; set; }
    }
}
