using Nancy;
using Nancy.ModelBinding;
using SpecialOffers.EventFeed;

namespace SpecialOffers.SpecialOffers
{
    public class SpecialOffersModule : NancyModule
    {
        public SpecialOffersModule(ISpecialOffersStore specialOffersStore)
            : base("/specialoffers")
        {
            Post("/", _ =>
            {
                var specialOffer = this.Bind<SpecialOffer>();

                if (specialOffer == null)
                    return HttpStatusCode.BadRequest;

                specialOffersStore.Add(specialOffer);
                specialOffersStore.Save();

                return CreatedResponse(specialOffer);
            });

            Put("/{offerId:int}", parameters =>
            {
                int offerId = parameters.offerId;
                var specialOffer = this.Bind<SpecialOffer>();

                if (specialOffer == null || specialOffer.Id != offerId)
                    return HttpStatusCode.BadRequest;

                var offer = specialOffersStore.Get(offerId);
                if (offer == null)
                    return HttpStatusCode.NotFound;

                specialOffersStore.Update(specialOffer);
                specialOffersStore.Save();

                return HttpStatusCode.NoContent;
            });
        }

        private dynamic CreatedResponse(SpecialOffer newOffer)
        {
            return Negotiate // Negotiate in an entry point to Nancy's fluent API for creating responses.
                .WithStatusCode(HttpStatusCode.Created) // Uses the 201 Created status code for the response
                .WithHeader("Location", $"{Request.Url.SiteBase}/specialoffers/{newOffer.Id}") // Adds a location header to the response because this is expected by HTTP for 201 Created responses.
                .WithModel(newOffer); // Returns the user in the response for convenience.
        }
    }
}