using Azure.Messaging.ServiceBus;
using Jogl.Server.Arxiv;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Jogl.Server.ExternalActivityMonitor
{
    public class PublicationStorageProcessor
    {
        private readonly IPublicationRepository _publicationRepository;
        private readonly IConfiguration _configuration;

        public PublicationStorageProcessor(IPublicationRepository publicationRepository, IConfiguration configuration)
        {
            _publicationRepository = publicationRepository;
            _configuration = configuration;
        }

        [Function("publication-storage-processor")]
        public async Task RunCommentsAsync(
        [ServiceBusTrigger("publication-loaded", "storage", Connection = "ConnectionString")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
        {
            var publication = JsonSerializer.Deserialize<Publication>(message.Body.ToString());
            await _publicationRepository.UpsertAsync(publication, p => p.ExternalID);
            await messageActions.CompleteMessageAsync(message);
        }
    }
}