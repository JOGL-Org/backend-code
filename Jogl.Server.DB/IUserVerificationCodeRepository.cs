using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface IUserVerificationCodeRepository : IRepository<UserVerificationCode>
    {
        UserVerificationCode GetForCode(string code);
    }
}