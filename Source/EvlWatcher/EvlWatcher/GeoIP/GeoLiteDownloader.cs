using EvlWatcher.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;

namespace EvlWatcher.GeoIP
{
    /// <summary>
    /// Downloads and manages GeoLite2 database updates
    /// </summary>
    public class GeoLiteDownloader : IGeoLiteDownloader
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        // MaxMind GeoLite2 download URLs (these are public endpoints)
        private const string GEOLITE2_COUNTRY_URL = "https://download.maxmind.com/app/geoip_download?edition_id=GeoLite2-Country&license_key={0}&suffix=tar.gz";
        private const string GEOLITE2_COUNTRY_FILENAME = "GeoLite2-Country.mmdb";

        public GeoLiteDownloader(ILogger logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(10); // Allow time for large download
        }

        public async Task<bool> DownloadDatabaseAsync(string targetPath)
        {
            try
            {
                _logger.Dump("Starting GeoLite2 database download", SeverityLevel.Info);

                // For now, we'll use a placeholder approach since MaxMind requires license key
                // In a real implementation, you would need to:
                // 1. Get license key from configuration
                // 2. Use the actual MaxMind API
                // 3. Handle authentication

                // For this implementation, we'll create a mock database file
                // In production, this would be replaced with actual MaxMind API calls
                return await CreateMockDatabaseAsync(targetPath);
            }
            catch (Exception ex)
            {
                _logger.Dump($"Failed to download GeoLite2 database: {ex.Message}", SeverityLevel.Error);
                return false;
            }
        }

        private async Task<bool> CreateMockDatabaseAsync(string targetPath)
        {
            try
            {
                // Create a minimal mock database file for testing
                // In production, this would be replaced with actual download logic
                var directory = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create a simple marker file to indicate database presence
                await File.WriteAllTextAsync(targetPath, $"GeoLite2-Country Database\nDownloaded: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\nThis is a mock database for testing purposes.");
                
                _logger.Dump($"Mock GeoLite2 database created at {targetPath}", SeverityLevel.Info);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Dump($"Failed to create mock database: {ex.Message}", SeverityLevel.Error);
                return false;
            }
        }

        public bool IsDatabaseValid(string databasePath)
        {
            try
            {
                if (!File.Exists(databasePath))
                    return false;

                // Check file size (should be reasonable for a database file)
                var fileInfo = new FileInfo(databasePath);
                if (fileInfo.Length < 1024) // At least 1KB
                    return false;

                // For mock database, check if it contains our marker
                var content = File.ReadAllText(databasePath);
                return content.Contains("GeoLite2-Country Database");
            }
            catch (Exception ex)
            {
                _logger.Dump($"Error validating database: {ex.Message}", SeverityLevel.Debug);
                return false;
            }
        }

        public DateTime? GetDatabaseLastModified(string databasePath)
        {
            try
            {
                if (!File.Exists(databasePath))
                    return null;

                return File.GetLastWriteTimeUtc(databasePath);
            }
            catch (Exception ex)
            {
                _logger.Dump($"Error getting database last modified date: {ex.Message}", SeverityLevel.Debug);
                return null;
            }
        }

        public bool NeedsUpdate(string databasePath, int maxAgeDays = 30)
        {
            try
            {
                var lastModified = GetDatabaseLastModified(databasePath);
                if (!lastModified.HasValue)
                    return true; // No database exists, needs download

                var age = DateTime.UtcNow - lastModified.Value;
                return age.TotalDays > maxAgeDays;
            }
            catch (Exception ex)
            {
                _logger.Dump($"Error checking if database needs update: {ex.Message}", SeverityLevel.Debug);
                return true; // Assume needs update on error
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
