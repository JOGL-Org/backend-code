using Jogl.Server.DB;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier.Discussion
{
    public class UserIndexFunction
    {
        private readonly IUserRepository _userRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IPaperRepository _paperRepository;
        private readonly IResourceRepository _resourceRepository;
        private readonly Search.ISearchService _searchService;
        private readonly ILogger<UserIndexFunction> _logger;

        public UserIndexFunction(IUserRepository userRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, Search.ISearchService searchService, ILogger<UserIndexFunction> logger)
        {
            _userRepository = userRepository;
            _documentRepository = documentRepository;
            _paperRepository = paperRepository;
            _searchService = searchService;
            _resourceRepository = resourceRepository;
            _logger = logger;
        }

        [Function("IndexUsers")]
        public async Task Run([TimerTrigger("0 * * * * *")] TimerInfo myTimer)
        {
            var users = _userRepository.Query(u => true).ToList();
            var documents = _documentRepository.Query(d => true).ToList();
            var papers = _paperRepository.Query(p => true).ToList();
            var resources = _resourceRepository.Query(p => true).ToList();

            await _searchService.IndexUsersAsync(users, documents, papers, resources);
        }
    }
}
