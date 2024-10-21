using RestSharp;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Services
{
    public class CaptchaVerificationService : IVerificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CaptchaVerificationService> _logger;

        public CaptchaVerificationService(IConfiguration configuration, ILogger<CaptchaVerificationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> VerifyAsync(string token, string action)
        {
            decimal riskScoreThreshold = decimal.Parse(_configuration["Google:Captcha:Score"]);
            var client = new RestClient($"https://recaptchaenterprise.googleapis.com/v1/projects/{_configuration["Google:ProjectID"]}/assessments?key={_configuration["Google:APIKey"]}");
            var request = new RestRequest("/");
            var referer = _configuration["App:URL"];
            request.AddHeader("Referer", referer);
            request.AddJsonBody(new
            {
                @event = new
                {
                    token,
                    expectedAction = action,
                    siteKey = _configuration["Google:Captcha:Token"],
                }
            });

            var response = await client.ExecutePostAsync<RecaptchaResponse>(request);
            return response.Data.RiskAnalysis.Score > riskScoreThreshold;
        }

        public class AccountDefenderAssessment
        {
            [JsonPropertyName("labels")]
            public List<object> Labels { get; set; }
        }

        public class Event
        {
            [JsonPropertyName("token")]
            public string Token { get; set; }

            [JsonPropertyName("siteKey")]
            public string SiteKey { get; set; }

            [JsonPropertyName("userAgent")]
            public string UserAgent { get; set; }

            [JsonPropertyName("userIpAddress")]
            public string UserIpAddress { get; set; }

            [JsonPropertyName("expectedAction")]
            public string ExpectedAction { get; set; }

            [JsonPropertyName("hashedAccountId")]
            public string HashedAccountId { get; set; }

            [JsonPropertyName("express")]
            public bool Express { get; set; }

            [JsonPropertyName("requestedUri")]
            public string RequestedUri { get; set; }

            [JsonPropertyName("wafTokenAssessment")]
            public bool WafTokenAssessment { get; set; }

            [JsonPropertyName("ja3")]
            public string Ja3 { get; set; }

            [JsonPropertyName("headers")]
            public List<object> Headers { get; set; }

            [JsonPropertyName("firewallPolicyEvaluation")]
            public bool FirewallPolicyEvaluation { get; set; }

            [JsonPropertyName("fraudPrevention")]
            public string FraudPrevention { get; set; }
        }

        public class RiskAnalysis
        {
            [JsonPropertyName("score")]
            public decimal Score { get; set; }

            [JsonPropertyName("reasons")]
            public List<object> Reasons { get; set; }

            [JsonPropertyName("extendedVerdictReasons")]
            public List<object> ExtendedVerdictReasons { get; set; }

            [JsonPropertyName("challenge")]
            public string Challenge { get; set; }
        }

        public class RecaptchaResponse
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("event")]
            public Event Event { get; set; }

            [JsonPropertyName("riskAnalysis")]
            public RiskAnalysis RiskAnalysis { get; set; }

            [JsonPropertyName("tokenProperties")]
            public TokenProperties TokenProperties { get; set; }

            [JsonPropertyName("accountDefenderAssessment")]
            public AccountDefenderAssessment AccountDefenderAssessment { get; set; }
        }

        public class TokenProperties
        {
            [JsonPropertyName("valid")]
            public bool Valid { get; set; }

            [JsonPropertyName("invalidReason")]
            public string InvalidReason { get; set; }

            [JsonPropertyName("hostname")]
            public string Hostname { get; set; }

            [JsonPropertyName("androidPackageName")]
            public string AndroidPackageName { get; set; }

            [JsonPropertyName("iosBundleId")]
            public string IosBundleId { get; set; }

            [JsonPropertyName("action")]
            public string Action { get; set; }

            [JsonPropertyName("createTime")]
            public DateTime CreateTime { get; set; }
        }
    }
}