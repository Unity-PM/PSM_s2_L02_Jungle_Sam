using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class LocalJsonAuthService : IAuthService
{
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;
    private const int Pbkdf2Iterations = 100000;

    private readonly bool _enableDebugLogs;

    public LocalJsonAuthService(bool enableDebugLogs = false)
    {
        _enableDebugLogs = enableDebugLogs;
    }

    public string UsersDatabasePath => Path.Combine(GetDataRootPath(), "UsersDatabase.json");

    public AuthResult Register(string username, string password)
    {
        username = NormalizeInput(username);
        password ??= string.Empty;

        if (string.IsNullOrWhiteSpace(username))
            return AuthResult.Error("Nazwa użytkownika nie może być pusta.");

        if (string.IsNullOrWhiteSpace(password))
            return AuthResult.Error("Hasło nie może być puste.");

        UsersDatabase database = LoadDatabase();

        if (ContainsUser(database, username))
            return AuthResult.Error("Użytkownik o tej nazwie już istnieje.");

        byte[] salt = GenerateSalt();
        string nowUtc = DateTime.UtcNow.ToString("o");
        AuthUserData user = new AuthUserData
        {
            userId = Guid.NewGuid().ToString("N"),
            username = username,
            passwordSalt = Convert.ToBase64String(salt),
            passwordHash = HashPassword(password, salt),
            createdAtUtc = nowUtc,
            lastLoginUtc = nowUtc
        };

        database.users.Add(user);
        SaveDatabase(database);
        AuthSession.SetUser(user);

        Log($"Registered user '{username}'. Database: {UsersDatabasePath}");
        return AuthResult.Success(user, "Rejestracja zakończona.");
    }

    public AuthResult Login(string username, string password)
    {
        username = NormalizeInput(username);
        password ??= string.Empty;

        if (string.IsNullOrWhiteSpace(username))
            return AuthResult.Error("Nazwa użytkownika nie może być pusta.");

        if (string.IsNullOrWhiteSpace(password))
            return AuthResult.Error("Hasło nie może być puste.");

        UsersDatabase database = LoadDatabase();
        AuthUserData user = FindUser(database, username);

        if (user == null)
            return AuthResult.Error("Nie znaleziono użytkownika.");

        if (!VerifyPassword(password, user))
            return AuthResult.Error("Błędne hasło.");

        user.lastLoginUtc = DateTime.UtcNow.ToString("o");
        SaveDatabase(database);
        AuthSession.SetUser(user);

        Log($"Logged in user '{username}'. Database: {UsersDatabasePath}");
        return AuthResult.Success(user, "Logowanie zakończone.");
    }

    public bool UserExists(string username)
    {
        username = NormalizeInput(username);

        if (string.IsNullOrWhiteSpace(username))
            return false;

        return ContainsUser(LoadDatabase(), username);
    }

    private UsersDatabase LoadDatabase()
    {
        EnsureDataFolderExists();

        if (!File.Exists(UsersDatabasePath))
        {
            UsersDatabase emptyDatabase = new UsersDatabase();
            SaveDatabase(emptyDatabase);
            return emptyDatabase;
        }

        try
        {
            string json = File.ReadAllText(UsersDatabasePath);
            UsersDatabase database = JsonUtility.FromJson<UsersDatabase>(json);

            if (database == null)
                database = new UsersDatabase();

            database.users ??= new System.Collections.Generic.List<AuthUserData>();
            database.users.RemoveAll(user => user == null);
            return database;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not read users database. A new empty database will be used. Path: {UsersDatabasePath}\n{exception}");
            return new UsersDatabase();
        }
    }

    private void SaveDatabase(UsersDatabase database)
    {
        EnsureDataFolderExists();

        database ??= new UsersDatabase();
        database.users ??= new System.Collections.Generic.List<AuthUserData>();

        string json = JsonUtility.ToJson(database, true);
        File.WriteAllText(UsersDatabasePath, json);
    }

    private static string GetDataRootPath()
    {
        return Path.Combine(Application.persistentDataPath, "JungleSam");
    }

    private static void EnsureDataFolderExists()
    {
        Directory.CreateDirectory(GetDataRootPath());
    }

    private static AuthUserData FindUser(UsersDatabase database, string username)
    {
        if (database?.users == null)
            return null;

        return database.users.FirstOrDefault(user =>
            user != null &&
            string.Equals(user.username, username, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsUser(UsersDatabase database, string username)
    {
        return FindUser(database, username) != null;
    }

    private static byte[] GenerateSalt()
    {
        byte[] salt = new byte[SaltSizeBytes];

        using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(salt);
        }

        return salt;
    }

    private static string HashPassword(string password, byte[] salt)
    {
        using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256))
        {
            return Convert.ToBase64String(pbkdf2.GetBytes(HashSizeBytes));
        }
    }

    private static bool VerifyPassword(string password, AuthUserData user)
    {
        if (user == null ||
            string.IsNullOrWhiteSpace(user.passwordSalt) ||
            string.IsNullOrWhiteSpace(user.passwordHash))
        {
            return false;
        }

        try
        {
            byte[] salt = Convert.FromBase64String(user.passwordSalt);
            string attemptedHash = HashPassword(password, salt);
            return SlowEquals(attemptedHash, user.passwordHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool SlowEquals(string a, string b)
    {
        if (a == null || b == null)
            return false;

        byte[] aBytes = Convert.FromBase64String(a);
        byte[] bBytes = Convert.FromBase64String(b);
        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }

    private static string NormalizeInput(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private void Log(string message)
    {
        if (_enableDebugLogs)
            Debug.Log($"[LocalJsonAuthService] {message}");
    }
}
