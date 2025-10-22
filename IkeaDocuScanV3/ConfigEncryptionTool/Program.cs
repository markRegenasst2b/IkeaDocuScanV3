using IkeaDocuScan.Shared.Configuration;
using System.Text.Json;

namespace ConfigEncryptionTool;

/// <summary>
/// Command-line tool for creating encrypted configuration files
/// Run this on the production server to create secrets.encrypted.json
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     IkeaDocuScan Configuration Encryption Tool                    ║");
        Console.WriteLine("║     Creates DPAPI-encrypted configuration file                    ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        try
        {
            // Check if running on Windows
            if (!OperatingSystem.IsWindows())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: This tool requires Windows (DPAPI is Windows-only)");
                Console.ResetColor();
                return;
            }

            // Gather configuration
            Console.WriteLine("Enter configuration values:");
            Console.WriteLine("---------------------------");
            Console.WriteLine();

            // Database Configuration
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("DATABASE CONFIGURATION:");
            Console.ResetColor();

            Console.Write("SQL Server (e.g., localhost or PROD-SQL-01): ");
            var server = Console.ReadLine() ?? "localhost";

            Console.Write("Database Name (default: IkeaDocuScan): ");
            var database = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(database))
                database = "IkeaDocuScan";

            Console.Write("Use Windows Authentication? (y/n, default: y): ");
            var useWindowsAuth = Console.ReadLine()?.ToLower() != "n";

            string connectionString;
            if (useWindowsAuth)
            {
                connectionString = $"Server={server};Database={database};Integrated Security=true;TrustServerCertificate=True;";
                Console.WriteLine($"✓ Using Windows Authentication");
            }
            else
            {
                Console.Write("Username: ");
                var username = Console.ReadLine();

                Console.Write("Password: ");
                var password = ReadPassword();

                connectionString = $"Server={server};Database={database};User Id={username};Password={password};TrustServerCertificate=True;";
                Console.WriteLine("✓ Using SQL Authentication");
            }

            Console.WriteLine();

            // Application Configuration
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("APPLICATION CONFIGURATION:");
            Console.ResetColor();

            Console.Write("Scanned Files Path (e.g., C:\\ScannedDocuments or \\\\FileServer\\Share): ");
            var filesPath = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(filesPath))
            {
                filesPath = "C:\\ScannedDocuments";
            }

            Console.WriteLine();

            // Encrypt connection string
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Encrypting connection string...");
            Console.ResetColor();

            var encryptedConnectionString = DpapiConfigurationHelper.Encrypt(connectionString);

            // Create configuration object
            var config = new
            {
                ConnectionStrings = new
                {
                    DefaultConnection = encryptedConnectionString
                },
                IkeaDocuScan = new
                {
                    ScannedFilesPath = filesPath
                }
            };

            // Serialize to JSON
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(config, options);

            // Save to file
            var outputFileName = "secrets.encrypted.json";
            File.WriteAllText(outputFileName, json);

            // Success message
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    ✓ SUCCESS                                       ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"Configuration encrypted and saved to: {Path.GetFullPath(outputFileName)}");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("NEXT STEPS:");
            Console.ResetColor();
            Console.WriteLine("1. Copy 'secrets.encrypted.json' to your application directory");
            Console.WriteLine("   Example: C:\\inetpub\\IkeaDocuScan\\");
            Console.WriteLine();
            Console.WriteLine("2. Ensure the IIS Application Pool identity has read access to the file");
            Console.WriteLine();
            Console.WriteLine("3. The encrypted file can ONLY be decrypted on THIS machine");
            Console.WriteLine("   with the SAME user account that encrypted it.");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("IMPORTANT SECURITY NOTES:");
            Console.ResetColor();
            Console.WriteLine("• Do NOT commit secrets.encrypted.json to source control");
            Console.WriteLine("• Create a backup of this file as part of server backups");
            Console.WriteLine("• If you change machines, you must re-run this tool");
            Console.WriteLine();

            // Verify decryption
            Console.Write("Verify encryption (test decrypt)? (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                try
                {
                    var decrypted = DpapiConfigurationHelper.Decrypt(encryptedConnectionString);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ Decryption test successful!");
                    Console.ResetColor();
                    Console.WriteLine("Decrypted connection string:");
                    Console.WriteLine(MaskConnectionString(decrypted));
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"✗ Decryption test failed: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════════════");
            Console.WriteLine("ERROR:");
            Console.WriteLine(ex.Message);
            Console.WriteLine("═══════════════════════════════════════════════════════════════════");
            Console.ResetColor();
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    /// <summary>
    /// Read password without showing on screen
    /// </summary>
    static string ReadPassword()
    {
        var password = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password.Substring(0, password.Length - 1);
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password;
    }

    /// <summary>
    /// Mask password in connection string for display
    /// </summary>
    static string MaskConnectionString(string connectionString)
    {
        if (connectionString.Contains("Password=", StringComparison.OrdinalIgnoreCase))
        {
            var parts = connectionString.Split(';');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Trim().StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
                {
                    parts[i] = "Password=********";
                }
            }
            return string.Join(";", parts);
        }
        return connectionString;
    }
}
