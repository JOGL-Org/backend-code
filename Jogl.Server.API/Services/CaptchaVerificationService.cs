using Recaptcha.Verify.Net;

namespace Jogl.Server.API.Services
{
    public class CaptchaVerificationService : IVerificationService
    {
        private readonly IRecaptchaService _recaptchaService;
        private readonly ILogger<CaptchaVerificationService> _logger;

        public CaptchaVerificationService(IRecaptchaService recaptchaService, ILogger<CaptchaVerificationService> logger)
        {
            _recaptchaService = recaptchaService;
            _logger = logger;
        }

        public async Task<bool> VerifyAsync(string token, string action)
        {
            var checkResult = await _recaptchaService.VerifyAndCheckAsync(token, action);

            if (!checkResult.Success)
            {
                if (!checkResult.Response.Success)
                    throw new Exception(string.Join(", ", checkResult.Response.ErrorCodes));

                if (!checkResult.ScoreSatisfies)
                    return false;

                return false;
            }

            return true;
        }
    }
}
