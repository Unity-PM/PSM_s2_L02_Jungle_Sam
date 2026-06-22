public interface IAuthService
{
    AuthResult Register(string username, string password);
    AuthResult Login(string username, string password);
    bool UserExists(string username);
}
