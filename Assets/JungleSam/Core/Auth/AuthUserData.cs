using System;

[Serializable]
public class AuthUserData
{
    public string userId;
    public string username;
    public string passwordHash;
    public string passwordSalt;
    public string createdAtUtc;
    public string lastLoginUtc;
}
