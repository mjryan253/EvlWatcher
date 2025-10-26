namespace EvlWatcher.GeoIP
{
    /// <summary>
    /// Contains country information for an IP address
    /// </summary>
    public class CountryInfo
    {
        /// <summary>
        /// ISO 3166-1 alpha-2 country code (e.g., "US", "CN", "RU")
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Full country name (e.g., "United States", "China", "Russia")
        /// </summary>
        public string CountryName { get; set; }

        /// <summary>
        /// Returns a string representation of the country info
        /// </summary>
        /// <returns>Country code and name, or just country code if name is not available</returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(CountryName))
                return $"{CountryCode} ({CountryName})";
            return CountryCode ?? "Unknown";
        }
    }
}
