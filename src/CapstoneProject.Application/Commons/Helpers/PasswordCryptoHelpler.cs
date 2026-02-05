using System.Security.Cryptography;
using System.Text;

namespace CapstoneProject.Application.Common.Helpers;

public static class PasswordCryptoHelper
{
    /// <summary>
    /// Derives a 32-byte key from the provided string using SHA256
    /// </summary>
    private static byte[] DeriveKey(string key)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
    }

    public static string Encrypt(string plainText, string encryptKey)
    {
        if (string.IsNullOrEmpty(encryptKey))
        {
            throw new ArgumentException("Encryption key cannot be null or empty", nameof(encryptKey));
        }

        using var aes = Aes.Create();
        // Derive a 32-byte key from the provided string (AES-256)
        aes.Key = DeriveKey(encryptKey);
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string cipherText, string encryptKey)
    {
        if (string.IsNullOrEmpty(encryptKey))
        {
            throw new ArgumentException("Encryption key cannot be null or empty", nameof(encryptKey));
        }

        var fullCipher = Convert.FromBase64String(cipherText);
        using var aes = Aes.Create();
        // Derive a 32-byte key from the provided string (AES-256)
        aes.Key = DeriveKey(encryptKey);

        var iv = new byte[aes.BlockSize / 8];
        var cipher = new byte[fullCipher.Length - iv.Length];

        Array.Copy(fullCipher, iv, iv.Length);
        Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(cipher);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}