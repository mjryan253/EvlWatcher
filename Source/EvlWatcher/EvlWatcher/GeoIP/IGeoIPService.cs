using System.Net;

namespace EvlWatcher.GeoIP
{
    /// <summary>
    /// Interface for IP geolocation services
    /// </summary>
    public interface IGeoIPService
    {
        /// <summary>
        /// Gets the country code for the specified IP address
        /// </summary>
        /// <param name="ipAddress">The IP address to lookup</param>
        /// <returns>ISO 3166-1 alpha-2 country code (e.g., "US", "CN", "RU") or null if not found</returns>
        string GetCountryCode(IPAddress ipAddress);

        /// <summary>
        /// Gets detailed country information for the specified IP address
        /// </summary>
        /// <param name="ipAddress">The IP address to lookup</param>
        /// <returns>CountryInfo object with country details or null if not found</returns>
        CountryInfo GetCountryInfo(IPAddress ipAddress);

        /// <summary>
        /// Indicates whether the GeoIP service is available and ready to use
        /// </summary>
        bool IsAvailable { get; }
    }
}
