public class AuthResult
{
    public bool success;
    public string message;
    public AuthUserData user;

    public static AuthResult Success(AuthUserData user, string message)
    {
        return new AuthResult
        {
            success = true,
            message = message,
            user = user
        };
    }

    public static AuthResult Error(string message)
    {
        return new AuthResult
        {
            success = false,
            message = message,
            user = null
        };
    }
}
