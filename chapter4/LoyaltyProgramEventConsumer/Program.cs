using System;

namespace LoyaltyProgramEventConsumer
{
    class Program
    {
        private EventSubscriber subscriber;

        public static void Main(string[] args) => new Program().Main();

        public void Main()
        {
            var specialOfferUrl = Environment.GetEnvironmentVariable("SpecialOffersUrl");
            Console.WriteLine(specialOfferUrl);

            this.subscriber = new EventSubscriber(specialOfferUrl);
            this.subscriber.Start();

            Console.ReadLine();

            this.subscriber.Stop();
        }
    }
}