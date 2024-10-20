﻿using Jogl.Server.Data;

namespace Jogl.Server.Auth
{
    public interface IAuthService
    {
        string GetToken(User user, string password);
        string GetToken(string email);
        string HashPasword(string password, out byte[] salt);
        bool VerifyPassword(string password, string hash, byte[] salt);
    }
}