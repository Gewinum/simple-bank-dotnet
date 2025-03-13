namespace Simplebank.Application.Providers;

using Simplebank.Domain.Interfaces.Providers;
using System;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

public class PasswordsProvider : IPasswordsProvider
{
    // Argon2id configuration parameters (adjust as needed)
    private const int SaltSize = 16;            // 16 bytes salt
    private const int HashSize = 32;            // 32 bytes hash output (256 bits)
    private const int Iterations = 3;           // t = 3
    private const int MemorySize = 65536;       // m = 65536 (in kilobytes, i.e. 64 MB)
    private const int DegreeOfParallelism = 2;  // p = 2
    private const int Argon2Version = 19;       // Version 19

    public string CreateHash(string password)
    {
        // Generate a cryptographically secure random salt
        byte[] saltBytes = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }

        // Initialize Argon2id with the password bytes and the generated salt
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = saltBytes,
            Iterations = Iterations,
            MemorySize = MemorySize,
            DegreeOfParallelism = DegreeOfParallelism
        };

        // Compute the hash
        byte[] hashBytes = argon2.GetBytes(HashSize);

        // Convert salt and hash to Base64 strings
        string saltBase64 = Convert.ToBase64String(saltBytes);
        string hashBase64 = Convert.ToBase64String(hashBytes);

        // Format the string as per the Argon2 reference: 
        // $argon2id$v=19$m=65536,t=3,p=2$<saltBase64>$<hashBase64>
        return $"$argon2id$v={Argon2Version}$m={MemorySize},t={Iterations},p={DegreeOfParallelism}${saltBase64}${hashBase64}";
    }

    public bool VerifyPassword(string password, string hashString)
    {
        // Expected format: $argon2id$v=19$m=65536,t=3,p=2$<saltBase64>$<hashBase64>
        // Split the string by '$'. Remove empty entries.
        var parts = hashString.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5)
        {
            // Invalid hash format
            return false;
        }

        // parts[0] = "argon2id"
        // parts[1] = "v=19"
        // parts[2] = "m=65536,t=3,p=2"
        // parts[3] = salt (Base64), parts[4] = hash (Base64)
        string saltBase64 = parts[3];
        string storedHashBase64 = parts[4];

        byte[] saltBytes;
        try
        {
            saltBytes = Convert.FromBase64String(saltBase64);
        }
        catch
        {
            return false;
        }

        // Re-create the Argon2id instance using the extracted salt and same parameters
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = saltBytes,
            Iterations = Iterations,
            MemorySize = MemorySize,
            DegreeOfParallelism = DegreeOfParallelism
        };

        // Compute the hash for the provided password
        byte[] computedHashBytes = argon2.GetBytes(HashSize);
        string computedHashBase64 = Convert.ToBase64String(computedHashBytes);

        // Compare the computed hash with the stored hash
        return computedHashBase64 == storedHashBase64;
    }
}
