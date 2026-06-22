public static class AuthSession
{
    public static AuthUserData CurrentUser { get; private set; }
    public static bool IsLoggedIn => CurrentUser != null;

    public static void SetUser(AuthUserData user)
    {
        CurrentUser = user;
    }

    public static void Clear()
    {
        CurrentUser = null;
    }
}
