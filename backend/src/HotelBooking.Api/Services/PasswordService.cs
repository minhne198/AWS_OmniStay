using System.Security.Cryptography;

namespace HotelBooking.Api.Services;

public sealed class PasswordService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return $"v1:{Iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        var parts = passwordHash.Split(':');
        if (parts is not ["v1", var iterationsValue, var saltValue, var hashValue])
        {
            return false;
        }

        if (!int.TryParse(iterationsValue, out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(saltValue);
        var expectedHash = Convert.FromBase64String(hashValue);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
