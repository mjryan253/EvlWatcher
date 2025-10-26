using EvlWatcher.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EvlWatcher.GeoIP
{
    /// <summary>
    /// Manages GeoIP database downloads and updates
    /// </summary>
    public class GeoIPDatabaseManager : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IGeoLiteDownloader _downloader;
        private readonly Timer _updateTimer;
        private readonly string _databasePath;
        private readonly int _updateIntervalHours;
        private readonly int _maxAgeDays;

        public GeoIPDatabaseManager(ILogger logger, IGeoLiteDownloader downloader, string databasePath, int updateIntervalHours = 24, int maxAgeDays = 30)
        {
            _logger = logger;
            _downloader = downloader;
            _databasePath = databasePath;
            _updateIntervalHours = updateIntervalHours;
            _maxAgeDays = maxAgeDays;

            // Set up periodic update timer
            _updateTimer = new Timer(CheckForUpdates, null, TimeSpan.Zero, TimeSpan.FromHours(_updateIntervalHours));
        }

        public bool IsDatabaseAvailable => _downloader.IsDatabaseValid(_databasePath);

        public string DatabasePath => _databasePath;

        private async void CheckForUpdates(object state)
        {
            try
            {
                await CheckAndUpdateDatabaseAsync();
            }
            catch (Exception ex)
            {
                _logger.Dump($"Error in database update check: {ex.Message}", SeverityLevel.Error);
            }
        }

        public async Task<bool> CheckAndUpdateDatabaseAsync()
        {
            try
            {
                _logger.Dump("Checking GeoIP database for updates", SeverityLevel.Debug);

                if (!_downloader.NeedsUpdate(_databasePath, _maxAgeDays))
                {
                    _logger.Dump("GeoIP database is up to date", SeverityLevel.Debug);
                    return true;
                }

                _logger.Dump("GeoIP database needs updating", SeverityLevel.Info);

                // Create backup of existing database if it exists
                if (File.Exists(_databasePath))
                {
                    var backupPath = _databasePath + ".backup";
                    File.Copy(_databasePath, backupPath, true);
                    _logger.Dump($"Created backup of existing database at {backupPath}", SeverityLevel.Debug);
                }

                // Download new database
                bool downloadSuccess = await _downloader.DownloadDatabaseAsync(_databasePath);

                if (downloadSuccess && _downloader.IsDatabaseValid(_databasePath))
                {
                    _logger.Dump("GeoIP database updated successfully", SeverityLevel.Info);
                    return true;
                }
                else
                {
                    _logger.Dump("Failed to update GeoIP database", SeverityLevel.Warning);
                    
                    // Restore backup if download failed
                    var backupPath = _databasePath + ".backup";
                    if (File.Exists(backupPath))
                    {
                        File.Copy(backupPath, _databasePath, true);
                        _logger.Dump("Restored database from backup", SeverityLevel.Info);
                    }
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Dump($"Error updating GeoIP database: {ex.Message}", SeverityLevel.Error);
                return false;
            }
        }

        public async Task<bool> ForceUpdateAsync()
        {
            try
            {
                _logger.Dump("Force updating GeoIP database", SeverityLevel.Info);
                return await _downloader.DownloadDatabaseAsync(_databasePath);
            }
            catch (Exception ex)
            {
                _logger.Dump($"Error in force update: {ex.Message}", SeverityLevel.Error);
                return false;
            }
        }

        public void Dispose()
        {
            _updateTimer?.Dispose();
            (_downloader as IDisposable)?.Dispose();
        }
    }
}
