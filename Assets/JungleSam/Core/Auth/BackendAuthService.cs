using System;

public class BackendAuthService : IAuthService
{
    // TODO: Replace LocalJsonAuthService with this implementation when the online backend is available.
    // This placeholder intentionally performs no HTTP requests in the MVP.

    public AuthResult Register(string username, string password)
    {
        throw new NotImplementedException("Backend auth is not implemented yet.");
    }

    public AuthResult Login(string username, string password)
    {
        throw new NotImplementedException("Backend auth is not implemented yet.");
    }

    public bool UserExists(string username)
    {
        throw new NotImplementedException("Backend auth is not implemented yet.");
    }
}
