using System;

using Nancy;
using Nancy.Bootstrapper;

namespace LoyaltyProgram
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        /// <summary>
        /// Nancy internal configuration
        /// </summary>
        protected override Func<ITypeCatalog, NancyInternalConfiguration> InternalConfiguration
            // Remove all default status-code handlers so they don't alter the responses.
            => NancyInternalConfiguration.WithOverrides(builder => builder.StatusCodeHandlers.Clear());
    }
}
