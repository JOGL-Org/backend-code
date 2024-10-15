using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Jogl.Server.Business
{
    public enum VerificationStatus { OK, Expired, Invalid }
    public class UserVerificationService : BaseService, IUserVerificationService
    {
        private readonly IUserVerificationCodeRepository _verificationCodeRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public UserVerificationService(IUserVerificationCodeRepository verificationCodeRepository, IEmailService emailService, IConfiguration configuration, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _verificationCodeRepository = verificationCodeRepository;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<string> CreateAsync(User user, VerificationAction action, bool notify)
        {
            //invalidate existing verification codes
            //var existingVerifications = _verificationCodeRepository.List(c => c.UserEmail == userEmail && c.Action == action);
            //await _verificationCodeRepository.DeleteAsync(existingVerifications);

            //generate code
            var code = GenerateCode();
            await _verificationCodeRepository.CreateAsync(new UserVerificationCode
            {
                Action = action,
                Code = code,
                CreatedUTC = DateTime.UtcNow,
                UserEmail = user.Email,
                ValidUntilUTC = DateTime.UtcNow.AddDays(1)
            });

            if (notify)
            {
                await _emailService.SendEmailAsync(user.Email, EmailTemplate.UserVerification, new
                {
                    first_name = user.FirstName,
                    url = _configuration["App:URL"] + $"/confirm?email={HttpUtility.UrlEncode(user.Email)}&verification_code={HttpUtility.UrlEncode(code)}",
                });
            }

            return code;
        }

        public VerificationStatus GetVerificationStatus(string userEmail, VerificationAction action, string code)
        {
            var existingVerification = _verificationCodeRepository.Get(c => c.UserEmail == userEmail && c.Action == action && c.Code == code);
            if (existingVerification == null || existingVerification.Deleted)
                return VerificationStatus.Invalid;

            return VerificationStatus.OK;
        }

        public async Task<VerificationStatus> VerifyAsync(string userEmail, VerificationAction action, string code)
        {
            var existingVerification = _verificationCodeRepository.Get(c => c.UserEmail == userEmail && c.Action == action && c.Code == code);
            if (existingVerification == null )
                return VerificationStatus.Invalid;

            var user = _userRepository.Get(u => u.Email == userEmail);
            if (user == null || user.Status != UserStatus.Pending)
                return VerificationStatus.Invalid;

            await _verificationCodeRepository.DeleteAsync(existingVerification.Id.ToString());
            await _userRepository.SetVerifiedAsync(user.Id.ToString());

            return VerificationStatus.OK;
        }

        private string GenerateCode(int size = 16)
        {
            var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[4 * size];
            using (var crypto = RandomNumberGenerator.Create())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }
    }
}