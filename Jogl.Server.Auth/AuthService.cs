using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Jogl.Server.DB;
using Microsoft.Extensions.Configuration;
using Jogl.Server.Data;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Hexarc.Borsh.Serialization;
using Azure.Security.KeyVault.Certificates;
using Azure.Identity;
using System.Security.Cryptography.X509Certificates;

namespace Jogl.Server.Auth
{
    public class AuthService : IAuthService
    {
        const int KEY_SIZE = 64;
        const int ITERATIONS = 350000;
        HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IAuthChallengeService _challengeService;
        public AuthService(IConfiguration configuration, IUserRepository userRepository, IAuthChallengeService challengeService)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _challengeService = challengeService;
        }

        public string GetTokenWithPassword(User user, string password)
        {
            if (user == null)
                return null;

            if (string.IsNullOrEmpty(password))
                return null;

            if (string.IsNullOrEmpty(user.PasswordHash) || user.PasswordSalt == null)
                return null;

            var passwordOk = VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
            if (!passwordOk)
                return null;

            return GetToken(user);
        }


        public string GetToken(string email)
        {
            var user = _userRepository.Get(u => u.Email == email);
            if (user == null)
                return null;

            return GetToken(user);
        }

        public string HashPasword(string password, out byte[] salt)
        {
            salt = RandomNumberGenerator.GetBytes(KEY_SIZE);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                ITERATIONS,
                hashAlgorithm,
                KEY_SIZE);
            return Convert.ToHexString(hash);
        }

        public bool VerifyPassword(string password, string hash, byte[] salt)
        {
            var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, salt, ITERATIONS, hashAlgorithm, KEY_SIZE);
            return hashToCompare.SequenceEqual(Convert.FromHexString(hash));
        }

        public string GetTokenWithSignature(User user, WalletType walletType, string wallet, string signature)
        {
            if (user == null)
                return null;
            if (string.IsNullOrEmpty(wallet) || string.IsNullOrEmpty(signature))
                return null;

            var walletData = user.Wallets.SingleOrDefault(w => w.Address == wallet);
            if (walletData == null)
                return null;

            var signatureOk = VerifySignature(walletType, wallet, signature);
            if (!signatureOk)
                return null;

            return GetToken(user);
        }

        [BorshObject]
        private class Payload
        {
            [BorshPropertyOrder(0)]
            public uint Tag { get; set; }

            [BorshPropertyOrder(1)]
            public string Message { get; set; }

            [BorshPropertyOrder(2), BorshFixedArray(32)]
            public byte[] Nonce { get; set; }

            [BorshPropertyOrder(3)]
            public string Recipient { get; set; }

            [BorshPropertyOrder(4), BorshOptional]
            public string? CallbackURL { get; set; }
        }

        public bool VerifySignature(WalletType walletType, string wallet, string signature)
        {
            var challenge = _challengeService.GetChallenge(wallet);
            var obj = new Payload
            {
                Tag = 2147484061,
                Message = challenge,
                Recipient = "JOGL",
                Nonce = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                CallbackURL = "http://localhost:3000/signin"
            };

            var dataToVerifyBytes = Hexarc.Borsh.BorshSerializer.Serialize(obj);
            using (SHA256 sha = SHA256.Create())
            {
                var publicKeyParam = new Ed25519PublicKeyParameters(ConvertToBytes("J2JHPk7ctoFknQS4BR8WbxScWLcAbNvEAhZ5HQUrKBuJ"));
                var signatureBytes = Convert.FromBase64String(signature);

                dataToVerifyBytes = sha.ComputeHash(dataToVerifyBytes);

                var verifier = new Ed25519Signer();
                verifier.Init(false, publicKeyParam);
                verifier.BlockUpdate(dataToVerifyBytes, 0, dataToVerifyBytes.Length);
                return verifier.VerifySignature(signatureBytes);
            }
        }

        private const string Base58Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        private byte[] ConvertToBytes(string publicKey)
        {
            if (string.IsNullOrEmpty(publicKey))
                throw new ArgumentException("Public key cannot be null or empty");

            // First convert to decimal
            int[] indexes = new int[publicKey.Length];
            for (int i = 0; i < publicKey.Length; i++)
            {
                int digitValue = Base58Alphabet.IndexOf(publicKey[i]);
                if (digitValue == -1)
                    throw new ArgumentException($"Invalid character '{publicKey[i]}' in public key");
                indexes[i] = digitValue;
            }

            // Convert base58 to base256 (bytes)
            int[] encoded = new int[publicKey.Length * 2]; // Over-allocate for safety
            int outputLength = 0;

            for (int i = 0; i < indexes.Length; i++)
            {
                int carry = indexes[i];
                for (int j = 0; j < outputLength; j++)
                {
                    carry += encoded[j] * 58;
                    encoded[j] = carry & 0xff;
                    carry >>= 8;
                }

                while (carry > 0)
                {
                    encoded[outputLength++] = carry & 0xff;
                    carry >>= 8;
                }
            }

            // Handle leading zeros
            for (int i = 0; i < publicKey.Length && publicKey[i] == '1'; i++)
            {
                encoded[outputLength++] = 0;
            }

            // Create final byte array with correct length
            return encoded.Take(outputLength).Reverse().Select(x => (byte)x).ToArray();
        }

        // Helper method to convert bytes to hex string for visualization
        private string BytesToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        private string GetToken(User user)
        {
            var certClient = new CertificateClient(
            new Uri(_configuration["Azure:KeyVault:URL"]),
            new DefaultAzureCredential());

            var cert = certClient.DownloadCertificate(_configuration["JWT:Cert-Name"]);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Sid, user.Id.ToString())
                ]),
                Expires = DateTime.UtcNow.AddMonths(1),
                SigningCredentials = new X509SigningCredentials(new X509Certificate2(cert.Value))
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}