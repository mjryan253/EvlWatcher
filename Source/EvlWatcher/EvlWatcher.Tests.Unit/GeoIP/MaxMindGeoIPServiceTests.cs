using NUnit.Framework;
using Moq;
using EvlWatcher.GeoIP;
using EvlWatcher.Logging;
using System.Net;

namespace EvlWatcher.Tests.Unit.GeoIP
{
    [TestFixture]
    public class MaxMindGeoIPServiceTests
    {
        private Mock<ILogger> _mockLogger;
        private MaxMindGeoIPService _geoIPService;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            _geoIPService = new MaxMindGeoIPService(_mockLogger.Object);
        }

        [Test]
        public void IsAvailable_WithoutDatabase_ReturnsFalse()
        {
            // Act
            bool isAvailable = _geoIPService.IsAvailable;

            // Assert
            Assert.IsFalse(isAvailable);
        }

        [Test]
        public void GetCountryCode_WithoutDatabase_ReturnsNull()
        {
            // Arrange
            var testIP = IPAddress.Parse("8.8.8.8");

            // Act
            string countryCode = _geoIPService.GetCountryCode(testIP);

            // Assert
            Assert.IsNull(countryCode);
        }

        [Test]
        public void GetCountryInfo_WithoutDatabase_ReturnsNull()
        {
            // Arrange
            var testIP = IPAddress.Parse("8.8.8.8");

            // Act
            var countryInfo = _geoIPService.GetCountryInfo(testIP);

            // Assert
            Assert.IsNull(countryInfo);
        }

        [Test]
        public void GetCountryCode_WithInvalidIP_ReturnsNull()
        {
            // Arrange
            var invalidIP = IPAddress.Parse("127.0.0.1"); // Local IP

            // Act
            string countryCode = _geoIPService.GetCountryCode(invalidIP);

            // Assert
            Assert.IsNull(countryCode);
        }

        [Test]
        public void GetCountryInfo_WithInvalidIP_ReturnsNull()
        {
            // Arrange
            var invalidIP = IPAddress.Parse("127.0.0.1"); // Local IP

            // Act
            var countryInfo = _geoIPService.GetCountryInfo(invalidIP);

            // Assert
            Assert.IsNull(countryInfo);
        }

        [Test]
        public void Constructor_LogsDatabaseNotFound()
        {
            // Assert
            _mockLogger.Verify(
                x => x.Dump(It.Is<string>(s => s.Contains("GeoLite2-Country.mmdb database file not found")), 
                           SeverityLevel.Warning),
                Times.Once);
        }

        [Test]
        public void GetCountryCode_LogsDebugOnFailure()
        {
            // Arrange
            var testIP = IPAddress.Parse("8.8.8.8");

            // Act
            _geoIPService.GetCountryCode(testIP);

            // Assert
            _mockLogger.Verify(
                x => x.Dump(It.Is<string>(s => s.Contains("Failed to get country code")), 
                           SeverityLevel.Debug),
                Times.Once);
        }
    }
}
