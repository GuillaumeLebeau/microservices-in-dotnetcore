using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nancy.Responses.Negotiation;
using System.IO;
using YamlDotNet.Serialization;
using Nancy;

namespace LoyaltyProgram
{
    public class YamlBodyDeserializer : IBodyDeserializer
    {
        /// <summary>
        /// Whether the deserializer can deserialize the content type
        /// </summary>
        /// <param name="mediaRange">Content type to deserialize</param>
        /// <param name="context">Current <see cref="T:Nancy.ModelBinding.BindingContext" />.</param>
        /// <returns>
        /// True if supported, false otherwise
        /// </returns>
        public bool CanDeserialize(MediaRange mediaRange, BindingContext context)
            // Tells Nancy which content types this deserializer can handle
            => mediaRange.Subtype.ToString().EndsWith("yaml");

        /// <summary>
        /// Deserialize the request body to a model
        /// </summary>
        /// <param name="mediaRange">Content type to deserialize</param>
        /// <param name="bodyStream">Request body stream</param>
        /// <param name="context">Current <see cref="T:Nancy.ModelBinding.BindingContext" />.</param>
        /// <returns>
        /// Model instance
        /// </returns>
        public object Deserialize(MediaRange mediaRange, Stream bodyStream, BindingContext context)
        {

            var yamlDeserializer = new Deserializer();
            var reader = new StreamReader(bodyStream);

            // Tries to deserialize the request body to the type needed by the application code
            return yamlDeserializer.Deserialize(reader, context.DestinationType);
        }
    }

    public class YamlBodySerializer : IResponseProcessor
    {
        /// <summary>
        /// Gets a set of mappings that map a given extension (such as .json)
        /// to a media range that can be sent to the client in a vary header.
        /// </summary>
        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
        {
            get
            {
                // Tells Nancy which file extensions can be handled by this response processor.
                // You don't use this feature.
                yield return new Tuple<string, MediaRange>("yaml", new MediaRange("application/yaml"));
            }
        }

        /// <summary>
        /// Determines whether the processor can handle a given content type and model.
        /// </summary>
        /// <param name="requestedMediaRange">Content type requested by the client.</param>
        /// <param name="model">The model for the given media range.</param>
        /// <param name="context">The nancy context.</param>
        /// <returns>
        /// A <see cref="T:Nancy.Responses.Negotiation.ProcessorMatch" /> result that determines the priority of the processor.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            // Tells Nancy that this processor can handle accept header values that end with "yaml"
            return requestedMediaRange.Subtype.ToString().EndsWith("yaml")
                ? new ProcessorMatch { ModelResult = MatchResult.DontCare, RequestedContentTypeResult = MatchResult.NonExactMatch }
                : ProcessorMatch.None;
        }

        /// <summary>
        /// Process the response.
        /// </summary>
        /// <param name="requestedMediaRange">Content type requested by the client.</param>
        /// <param name="model">The model for the given media range.</param>
        /// <param name="context">The nancy context.</param>
        /// <returns>
        /// A <see cref="T:Nancy.Response" /> instance.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Response Process(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            return new Response
            {
                // Sets up a function that writes the response body to a stream
                Contents = stream =>
                {
                    var yamlSerializer = new Serializer();
                    var streamWriter = new StreamWriter(stream);

                    // Writes the YAML serialized object to the stream Nancy uses for the response body
                    yamlSerializer.Serialize(streamWriter, model);
                    streamWriter.Flush();
                },
                ContentType = "application/yaml"
            };
        }
    }
}
