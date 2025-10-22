using System.Security.Cryptography;
using System.Text;

namespace IkeaDocuScan.Shared.Configuration;

/// <summary>
/// Helper class for encrypting and decrypting configuration values using Windows DPAPI
/// Data Protection API (DPAPI) - encrypts data using Windows credentials
/// Only the same user account on the same machine can decrypt
/// </summary>
public static class DpapiConfigurationHelper
{
    /// <summary>
    /// Encrypt plain text using DPAPI LocalMachine scope
    /// </summary>
    /// <param name="plainText">Text to encrypt</param>
    /// <returns>Base64-encoded encrypted string</returns>
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentNullException(nameof(plainText));

        var data = Encoding.UTF8.GetBytes(plainText);
        var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.LocalMachine);
        return Convert.ToBase64String(encrypted);
    }

    /// <summary>
    /// Decrypt DPAPI encrypted text
    /// </summary>
    /// <param name="encryptedText">Base64-encoded encrypted string</param>
    /// <returns>Decrypted plain text</returns>
    public static string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            throw new ArgumentNullException(nameof(encryptedText));

        try
        {
            var data = Convert.FromBase64String(encryptedText);
            var decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException(
                "Cannot decrypt configuration. Ensure the encrypted file was created on this machine with the same user account.",
                ex);
        }
    }

    /// <summary>
    /// Check if a string appears to be encrypted (is valid Base64)
    /// </summary>
    public static bool IsEncrypted(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            Convert.FromBase64String(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
