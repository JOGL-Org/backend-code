using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Jogl.Server.DB;
using Microsoft.Extensions.Configuration;
using Jogl.Server.Data;

namespace Jogl.Server.Auth
{
    public class AuthService : IAuthService
    {
        const int KEY_SIZE = 64;
        const int ITERATIONS = 350000;
        HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        public AuthService(IConfiguration configuration, IUserRepository userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
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

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Sid, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMonths(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public string GetToken(string email)
        {
            var user = _userRepository.GetForEmail(email);
            if (user == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Sid, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMonths(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
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
    }
}