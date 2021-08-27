using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.DigitalTwins.Core;

namespace AdtModelUploader
{
    internal sealed class DigitalTwinsRepository
    {
        // https://docs.microsoft.com/en-us/azure/digital-twins/reference-service-limits#functional-limits
        private const int MaximumRequestBodySize = 31000;

        // https://docs.microsoft.com/en-us/azure/digital-twins/reference-service-limits#functional-limits
        private const int MaximumNumberOfModels = 250;

        private readonly DigitalTwinsClient digitalTwinsClient;

        private int numberOfCalls;

        public DigitalTwinsRepository(Uri adtUri, TokenCredential credential)
        {
            this.digitalTwinsClient = new DigitalTwinsClient(adtUri, credential);
        }

        public async Task CreateModelsAsync(List<string> dtdlModels, CancellationToken cancellationToken)
        {
            if (dtdlModels.Count <= MaximumNumberOfModels && GetByteCount(dtdlModels) <= MaximumRequestBodySize)
            {
                await this.InternalCreateModelsAsync(dtdlModels, cancellationToken);
                return;
            }

            await this.CreateModelsAsyncChunked(dtdlModels, cancellationToken);
        }

        private async Task InternalCreateModelsAsync(List<string> dtdlModels,
            CancellationToken cancellationToken)
        {
            try
            {
                this.numberOfCalls++;
                Console.WriteLine($"API Call #{this.numberOfCalls} with {dtdlModels.Count} models");
                await this.digitalTwinsClient.CreateModelsAsync(dtdlModels, cancellationToken);
            }
            catch (RequestFailedException requestFailedException)
            {
                // Ignore 409: Model already exists.
                if (requestFailedException.Status == 409)
                {
                    return;
                }
                
                throw;
            }
        }

        /// <summary>
        /// Returns the number of bytes the request body is. It takes into account brackets and commas needed to create the body.
        /// </summary>
        private static int GetByteCount(IEnumerable<string> strings)
        {
            return Encoding.UTF8.GetByteCount($"[{string.Join(',', strings)}]");
        }

        /// <summary>
        /// Uploads a list of models that is bigger than the allowed request size by using multiple requests.
        /// The request size is maximized until no other models will fit.
        /// </summary>
        private async Task CreateModelsAsyncChunked(List<string> dtdlModels, CancellationToken cancellationToken)
        {
            for (var i = 0; i < dtdlModels.Count;)
            {
                var modelsToUpload = new List<string>();
                for (var x = i; x < dtdlModels.Count; x++)
                {
                    modelsToUpload.Add(dtdlModels[x]);
                    if (GetByteCount(modelsToUpload) > MaximumRequestBodySize || modelsToUpload.Count > MaximumNumberOfModels)
                    {
                        i = x;
                        modelsToUpload.RemoveAt(modelsToUpload.Count - 1);
                        break;
                    }

                    i = x + 1;
                }

                if (modelsToUpload.Count == 0)
                {
                    throw new InvalidOperationException("Single model was larger than maximum request size.");
                }

                await this.InternalCreateModelsAsync(modelsToUpload, cancellationToken);
            }
        }
    }
}