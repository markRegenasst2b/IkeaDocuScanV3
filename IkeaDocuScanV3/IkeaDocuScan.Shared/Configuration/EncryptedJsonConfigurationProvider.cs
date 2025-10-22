using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Security.Cryptography;

namespace IkeaDocuScan.Shared.Configuration;

/// <summary>
/// Custom configuration provider that automatically decrypts DPAPI-encrypted values
/// </summary>
public class EncryptedJsonConfigurationProvider : JsonConfigurationProvider
{
    private readonly List<string> _keysToDecrypt;

    public EncryptedJsonConfigurationProvider(
        EncryptedJsonConfigurationSource source,
        List<string> keysToDecrypt) : base(source)
    {
        _keysToDecrypt = keysToDecrypt ?? new List<string>();
    }

    public override void Load(Stream stream)
    {
        base.Load(stream);

        // Decrypt specific keys
        foreach (var key in _keysToDecrypt)
        {
            if (Data.TryGetValue(key, out var encrypted) && !string.IsNullOrEmpty(encrypted))
            {
                try
                {
                    Data[key] = DpapiConfigurationHelper.Decrypt(encrypted);
                }
                catch (CryptographicException)
                {
                    // Log warning but don't fail - might be running on different machine
                    // or value might not be encrypted
                    Console.WriteLine($"Warning: Could not decrypt configuration key: {key}");
                }
            }
        }
    }
}

/// <summary>
/// Configuration source for encrypted JSON files
/// </summary>
public class EncryptedJsonConfigurationSource : JsonConfigurationSource
{
    public List<string> KeysToDecrypt { get; set; } = new();

    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        EnsureDefaults(builder);
        return new EncryptedJsonConfigurationProvider(this, KeysToDecrypt);
    }
}

/// <summary>
/// Extension methods for adding encrypted JSON configuration
/// </summary>
public static class EncryptedJsonConfigurationExtensions
{
    /// <summary>
    /// Add encrypted JSON configuration file
    /// </summary>
    public static IConfigurationBuilder AddEncryptedJsonFile(
        this IConfigurationBuilder builder,
        string path,
        bool optional = false,
        bool reloadOnChange = false,
        params string[] keysToDecrypt)
    {
        return builder.Add(new EncryptedJsonConfigurationSource
        {
            Path = path,
            Optional = optional,
            ReloadOnChange = reloadOnChange,
            KeysToDecrypt = keysToDecrypt?.ToList() ?? new List<string>
            {
                "ConnectionStrings:DefaultConnection"  // Default key to decrypt
            }
        });
    }
}
