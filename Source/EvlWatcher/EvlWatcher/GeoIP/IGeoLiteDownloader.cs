using System;
using System.Threading.Tasks;

namespace EvlWatcher.GeoIP
{
    /// <summary>
    /// Interface for downloading and updating GeoLite2 database
    /// </summary>
    public interface IGeoLiteDownloader
    {
        /// <summary>
        /// Downloads the latest GeoLite2-Country.mmdb database
        /// </summary>
        /// <param name="targetPath">Path where to save the database file</param>
        /// <returns>True if download was successful</returns>
        Task<bool> DownloadDatabaseAsync(string targetPath);

        /// <summary>
        /// Checks if the database file exists and is valid
        /// </summary>
        /// <param name="databasePath">Path to the database file</param>
        /// <returns>True if database exists and is valid</returns>
        bool IsDatabaseValid(string databasePath);

        /// <summary>
        /// Gets the last modified date of the database file
        /// </summary>
        /// <param name="databasePath">Path to the database file</param>
        /// <returns>Last modified date, or null if file doesn't exist</returns>
        DateTime? GetDatabaseLastModified(string databasePath);

        /// <summary>
        /// Checks if the database needs updating (older than specified days)
        /// </summary>
        /// <param name="databasePath">Path to the database file</param>
        /// <param name="maxAgeDays">Maximum age in days before update is needed</param>
        /// <returns>True if database needs updating</returns>
        bool NeedsUpdate(string databasePath, int maxAgeDays = 30);
    }
}
