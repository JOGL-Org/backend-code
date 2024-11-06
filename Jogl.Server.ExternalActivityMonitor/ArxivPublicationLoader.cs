using Jogl.Server.Arxiv;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Notifications;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;

namespace Jogl.Server.ExternalActivityMonitor
{
    public class ArxivPublicationLoader
    {
        private readonly IArxivFacade _arxivFacade;
        private readonly INotificationFacade _notificationFacade;
        private readonly IConfiguration _configuration;

        public ArxivPublicationLoader(IArxivFacade arxivFacade, INotificationFacade notificationFacade, IConfiguration configuration)
        {
            _arxivFacade = arxivFacade;
            _notificationFacade = notificationFacade;
            _configuration = configuration;
        }

        [Function("ARXIV-publication-loader")]
        public async Task Run([TimerTrigger("0 0 7 * * *")] TimerInfo myTimer)
        {
            //load new papers from arxiv
            var entries = await _arxivFacade.ListNewPapersAsync(DateTime.UtcNow.AddDays(-2));

            //transform to publication object
            var publications = entries.Select(entry =>
            {
                var id = entry.Id.Replace("http://arxiv.org/abs/", string.Empty);
                var idWithoutVersion = id.Substring(0, id.IndexOf("v"));

                return new Publication
                {
                    Authors = entry.Author.Select(a => a.Name).ToList(),
                    CreatedUTC = DateTime.UtcNow,
                    DOI = entry.Doi?.Text,
                    Journal = entry.JournalRef?.Text,
                    //LicenseURL = entry.
                    Published = entry.Published,
                    ExternalID = idWithoutVersion,
                    //Submitter =entry.
                    ExternalSystem = "ARXIV",
                    ExternalURL = $"https://arxiv.org/abs/{idWithoutVersion}",
                    ExternalFileURL = $"https://arxiv.org/pdf/{idWithoutVersion}",
                    Summary = entry.Summary,
                    Tags = entry.Category.Select(c => c.Term).ToList(),
                    Title = entry.Title
                };
            });


            await _notificationFacade.NotifyLoadedAsync(publications);
        }
    }
}