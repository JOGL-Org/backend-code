namespace Jogl.Server.Cryptography
{
    public interface ICryptographyService
    {
        string ComputeHash(string data);
        string GenerateCode(int size = 16, bool allowAlpha = true);
    }
}
