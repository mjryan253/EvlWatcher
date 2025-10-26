using System;
using System.IO;
using System.Net;
using EvlWatcher.Logging;
using MaxMind.GeoIP2;

namespace EvlWatcher.GeoIP
{
    /// <summary>
    /// MaxMind GeoLite2 implementation of IP geolocation service
    /// </summary>
    public class MaxMindGeoIPService : IGeoIPService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly GeoIPDatabaseManager _databaseManager;
        private DatabaseReader _databaseReader;
        private bool _isInitialized = false;

        public MaxMindGeoIPService(ILogger logger, GeoIPDatabaseManager databaseManager = null)
        {
            _logger = logger;
            
            // Create database manager if not provided
            if (databaseManager == null)
            {
                var executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var directory = Path.GetDirectoryName(executablePath);
                var databasePath = Path.Combine(directory, "GeoLite2-Country.mmdb");
                var downloader = new GeoLiteDownloader(logger);
                _databaseManager = new GeoIPDatabaseManager(logger, downloader, databasePath);
            }
            else
            {
                _databaseManager = databaseManager;
            }
            
            InitializeDatabase();
        }

        public bool IsAvailable => _isInitialized && _databaseReader != null;

        public string GetCountryCode(IPAddress ipAddress)
        {
            if (!IsAvailable)
                return null;

            try
            {
                var countryInfo = GetCountryInfo(ipAddress);
                return countryInfo?.CountryCode;
            }
            catch (Exception ex)
            {
                _logger.Dump($"Failed to get country code for {ipAddress}: {ex.Message}", SeverityLevel.Debug);
                return null;
            }
        }

        public CountryInfo GetCountryInfo(IPAddress ipAddress)
        {
            if (!IsAvailable)
                return null;

            try
            {
                var response = _databaseReader.Country(ipAddress);
                return new CountryInfo
                {
                    CountryCode = response.Country.IsoCode,
                    CountryName = response.Country.Name
                };
            }
            catch (Exception ex)
            {
                _logger.Dump($"Failed to get country info for {ipAddress}: {ex.Message}", SeverityLevel.Debug);
                return null;
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                var databasePath = _databaseManager.DatabasePath;

                if (!_databaseManager.IsDatabaseAvailable)
                {
                    _logger.Dump("GeoLite2-Country.mmdb database file not found. Attempting to download...", SeverityLevel.Info);
                    
                    // Try to download the database
                    var downloadTask = _databaseManager.CheckAndUpdateDatabaseAsync();
                    downloadTask.Wait(TimeSpan.FromMinutes(5)); // Wait up to 5 minutes for download
                    
                    if (!_databaseManager.IsDatabaseAvailable)
                    {
                        _logger.Dump("Failed to download GeoLite2 database. Country blocking will be disabled.", SeverityLevel.Warning);
                        _logger.Dump($"Expected database location: {databasePath}", SeverityLevel.Info);
                        _logger.Dump("Please download GeoLite2-Country.mmdb from https://dev.maxmind.com/geoip/geolite2-free-geolocation-data", SeverityLevel.Info);
                        return;
                    }
                }

                _databaseReader = new DatabaseReader(databasePath);
                _isInitialized = true;
                _logger.Dump("MaxMind GeoLite2 database loaded successfully", SeverityLevel.Info);
            }
            catch (Exception ex)
            {
                _logger.Dump($"Failed to initialize MaxMind GeoLite2 database: {ex.Message}", SeverityLevel.Warning);
                _logger.Dump("Country blocking will be disabled", SeverityLevel.Warning);
            }
        }

        public void Dispose()
        {
            _databaseReader?.Dispose();
            _databaseManager?.Dispose();
        }
    }
}
