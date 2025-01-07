namespace Jogl.Server.Cryptography
{
    public interface IHashService
    {
        string ComputeHash(string data);
    }
}
