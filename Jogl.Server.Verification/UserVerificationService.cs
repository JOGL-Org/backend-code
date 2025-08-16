using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Jogl.Server.Verification
{
    public enum VerificationStatus { OK, Expired, Invalid }
    public class UserVerificationService : IUserVerificationService
    {
        private readonly IUserVerificationCodeRepository _verificationCodeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public UserVerificationService(IUserVerificationCodeRepository verificationCodeRepository, IUserRepository userRepository, IEmailService emailService, IConfiguration configuration)
        {
            _verificationCodeRepository = verificationCodeRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<string> CreateAsync(User user, VerificationAction action, string redirectURL = null, bool notify = false)
        {
            //invalidate existing verification codes
            //var existingVerifications = _verificationCodeRepository.List(c => c.UserEmail == userEmail && c.Action == action);
            //await _verificationCodeRepository.DeleteAsync(existingVerifications);

            //generate code
            var code = GenerateCode(6, false);
            await _verificationCodeRepository.CreateAsync(new UserVerificationCode
            {
                Action = action,
                Code = code,
                RedirectURL = redirectURL,
                CreatedUTC = DateTime.UtcNow,
                UserEmail = user.Email,
                ValidUntilUTC = DateTime.UtcNow.AddDays(1)
            });

            if (notify)
            {
                await _emailService.SendEmailAsync(user.Email, EmailTemplate.UserVerification, new
                {
                    code,
                    //url = _configuration["App:URL"] + $"/confirm?email={HttpUtility.UrlEncode(user.Email)}&verification_code={HttpUtility.UrlEncode(code)}&redirectURL={redirectURL}",
                    LANGUAGE = user.Language
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

        public async Task<VerificationResult> VerifyAsync(string userEmail, VerificationAction action, string code)
        {
            var existingVerification = _verificationCodeRepository.Get(c => c.UserEmail == userEmail && c.Action == action && c.Code == code);
            if (existingVerification == null)
                return new VerificationResult { Status = VerificationStatus.Invalid };

            var user = _userRepository.Get(u => u.Email == userEmail);
            if (user == null)
                return new VerificationResult { Status = VerificationStatus.Invalid };

            await _verificationCodeRepository.DeleteAsync(existingVerification.Id.ToString());
            await _userRepository.SetStatusAsync(user.Id.ToString(), UserStatus.Verified);

            return new VerificationResult { Status = VerificationStatus.OK, RedirectURL = existingVerification.RedirectURL };
        }

        private string GenerateCode(int size = 16, bool allowAlpha = true)
        {
            var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            if (!allowAlpha)
                chars = "1234567890".ToCharArray();

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