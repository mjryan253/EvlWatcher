using System.Net;

namespace EvlWatcherConsole.Model
{
    /// <summary>
    /// Wrapper for IPAddress that includes country information for display
    /// </summary>
    public class IPAddressWithCountry
    {
        public IPAddress IPAddress { get; set; }
        public string Country { get; set; }

        public IPAddressWithCountry(IPAddress ipAddress, string country = "Unknown")
        {
            IPAddress = ipAddress;
            Country = country;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Country) || Country == "Unknown")
                return IPAddress.ToString();
            return $"{IPAddress} ({Country})";
        }

        public override bool Equals(object obj)
        {
            if (obj is IPAddressWithCountry other)
                return IPAddress.Equals(other.IPAddress);
            return false;
        }

        public override int GetHashCode()
        {
            return IPAddress.GetHashCode();
        }
    }
}
