using Jogl.Server.AI;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Mailer
{
    public class Users
    {
        private readonly IUserService _userService;
        private readonly IAIService _aiService;
        private readonly IEmailService _emailService;
        private readonly IUserRepository _userRepository;
        private readonly IEntityScoreRepository _entityScoreRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        private const decimal AUTO_DELETE_THRESHOLD = 85;

        public Users(IUserService userService, IAIService aiService, IEmailService emailService, IUserRepository userRepository, IEntityScoreRepository entityScoreRepository, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _userService = userService;
            _aiService = aiService;
            _emailService = emailService;
            _userRepository = userRepository;
            _entityScoreRepository = entityScoreRepository;
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<Users>();
        }

        [Function("Users")]
        public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer)
        {
            var entityScores = _entityScoreRepository.List(s => s.EntityType == EntityScore.USER && !s.Deleted);
            var users = _userRepository.List(u => !entityScores.Select(s => s.EntityId).Contains(u.Id.ToString()) && !u.Deleted);
            foreach (var user in users)
            {
                //exceptions for orcid users
                if (user.Auth?.IsOrcidUser==true)
                    continue;

                //exceptions for internal users
                if(user.Email.EndsWith("@jogl.io"))
                    continue;

                var score = await _aiService.GetBotScoreAsync(user);
                await _entityScoreRepository.CreateAsync(new EntityScore
                {
                    EntityId = user.Id.ToString(),
                    Score = score,
                    EntityType = EntityScore.USER,
                });

                if (score >= AUTO_DELETE_THRESHOLD)
                {
                    await _userService.DeleteAsync(user.Id.ToString());
                    await _emailService.SendEmailAsync(_configuration["UserDeleteNotificationEmail"], EmailTemplate.ObjectDeleted, new
                    {
                        ENTITY_TYPE = "user",
                        ENTITY_NAME = $"{user.FullName}, id {user.Id}, email {user.Email}",
                        REASON = $"Claude thinks this is a bot ({score}% confidence)",
                    });

                }
            }
        }
    }
}
