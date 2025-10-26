using NUnit.Framework;
using Moq;
using EvlWatcher;
using EvlWatcher.Config;
using EvlWatcher.GeoIP;
using EvlWatcher.Logging;
using EvlWatcher.Tasks;
using EvlWatcher.WCF.DTO;
using System.Net;

namespace EvlWatcher.Tests.Unit.WCF
{
    [TestFixture]
    public class CountryBlockingServiceTests
    {
        private Mock<ILogger> _mockLogger;
        private Mock<IPersistentServiceConfiguration> _mockConfig;
        private Mock<IGenericTaskFactory> _mockTaskFactory;
        private Mock<IGeoIPService> _mockGeoIPService;
        private EvlWatcher.EvlWatcher _evlWatcher;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            _mockConfig = new Mock<IPersistentServiceConfiguration>();
            _mockTaskFactory = new Mock<IGenericTaskFactory>();
            _mockGeoIPService = new Mock<IGeoIPService>();

            _evlWatcher = new EvlWatcher.EvlWatcher(
                _mockLogger.Object,
                _mockConfig.Object,
                _mockTaskFactory.Object,
                _mockGeoIPService.Object);
        }

        [Test]
        public void GetBlockedCountries_ReturnsCorrectCountries()
        {
            // Arrange
            var expectedCountries = new[] { "CN", "RU", "KP" };
            _mockConfig.Setup(x => x.BlockedCountries).Returns(expectedCountries.AsQueryable());

            // Act
            var result = _evlWatcher.GetBlockedCountries();

            // Assert
            Assert.AreEqual(expectedCountries.Length, result.Length);
            CollectionAssert.AreEquivalent(expectedCountries, result);
        }

        [Test]
        public void SetBlockedCountries_ValidCountries_UpdatesConfiguration()
        {
            // Arrange
            var countryCodes = new[] { "CN", "RU", "KP" };
            _mockConfig.Setup(x => x.BlockedCountries).Returns(new string[0].AsQueryable());

            // Act
            _evlWatcher.SetBlockedCountries(countryCodes);

            // Assert
            _mockConfig.Verify(x => x.RemoveBlockedCountry(It.IsAny<string>()), Times.AtLeastOnce);
            _mockConfig.Verify(x => x.AddBlockedCountry("CN"), Times.Once);
            _mockConfig.Verify(x => x.AddBlockedCountry("RU"), Times.Once);
            _mockConfig.Verify(x => x.AddBlockedCountry("KP"), Times.Once);
        }

        [Test]
        public void SetBlockedCountries_InvalidCountryCodes_IgnoresInvalid()
        {
            // Arrange
            var countryCodes = new[] { "CN", "INVALID", "RU", "", "KP" };
            _mockConfig.Setup(x => x.BlockedCountries).Returns(new string[0].AsQueryable());

            // Act
            _evlWatcher.SetBlockedCountries(countryCodes);

            // Assert
            _mockConfig.Verify(x => x.AddBlockedCountry("CN"), Times.Once);
            _mockConfig.Verify(x => x.AddBlockedCountry("RU"), Times.Once);
            _mockConfig.Verify(x => x.AddBlockedCountry("KP"), Times.Once);
            _mockConfig.Verify(x => x.AddBlockedCountry("INVALID"), Times.Never);
            _mockConfig.Verify(x => x.AddBlockedCountry(""), Times.Never);
        }

        [Test]
        public void GetCountryBlockingEnabled_ReturnsConfigurationValue()
        {
            // Arrange
            _mockConfig.Setup(x => x.CountryBlockingEnabled).Returns(true);

            // Act
            var result = _evlWatcher.GetCountryBlockingEnabled();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void SetCountryBlockingEnabled_UpdatesConfiguration()
        {
            // Act
            _evlWatcher.SetCountryBlockingEnabled(true);

            // Assert
            _mockConfig.VerifySet(x => x.CountryBlockingEnabled = true, Times.Once);
        }

        [Test]
        public void GetIPCountry_GeoIPServiceUnavailable_ReturnsUnknown()
        {
            // Arrange
            var testIP = IPAddress.Parse("8.8.8.8");
            _mockGeoIPService.Setup(x => x.IsAvailable).Returns(false);

            // Act
            var result = _evlWatcher.GetIPCountry(testIP);

            // Assert
            Assert.AreEqual("Unknown", result);
        }

        [Test]
        public void GetIPCountry_GeoIPServiceAvailable_ReturnsCountryInfo()
        {
            // Arrange
            var testIP = IPAddress.Parse("8.8.8.8");
            var mockCountryInfo = new CountryInfo { CountryCode = "US", CountryName = "United States" };
            _mockGeoIPService.Setup(x => x.IsAvailable).Returns(true);
            _mockGeoIPService.Setup(x => x.GetCountryInfo(testIP)).Returns(mockCountryInfo);

            // Act
            var result = _evlWatcher.GetIPCountry(testIP);

            // Assert
            Assert.AreEqual("US (United States)", result);
        }

        [Test]
        public void GetIPCountry_GeoIPServiceThrows_ReturnsUnknown()
        {
            // Arrange
            var testIP = IPAddress.Parse("8.8.8.8");
            _mockGeoIPService.Setup(x => x.IsAvailable).Returns(true);
            _mockGeoIPService.Setup(x => x.GetCountryInfo(testIP)).Throws(new System.Exception("Test exception"));

            // Act
            var result = _evlWatcher.GetIPCountry(testIP);

            // Assert
            Assert.AreEqual("Unknown", result);
            _mockLogger.Verify(x => x.Dump(It.Is<string>(s => s.Contains("Failed to get country")), SeverityLevel.Debug), Times.Once);
        }

        [Test]
        public void ApplyCountryRulesToExistingBans_NoBans_DoesNothing()
        {
            // Arrange
            _mockConfig.Setup(x => x.BlacklistAddresses).Returns(new IPAddress[0].AsQueryable());

            // Act
            _evlWatcher.ApplyCountryRulesToExistingBans();

            // Assert
            _mockLogger.Verify(x => x.Dump("Applying country rules to existing bans", SeverityLevel.Info), Times.Once);
        }

        [Test]
        public void ApplyCountryRulesToExistingBans_WithBansFromBlockedCountries_RemovesThem()
        {
            // Arrange
            var bannedIPs = new[] { IPAddress.Parse("1.1.1.1"), IPAddress.Parse("2.2.2.2") };
            _mockConfig.Setup(x => x.BlacklistAddresses).Returns(bannedIPs.AsQueryable());
            _mockConfig.Setup(x => x.CountryBlockingEnabled).Returns(true);
            _mockGeoIPService.Setup(x => x.IsAvailable).Returns(true);
            _mockGeoIPService.Setup(x => x.GetCountryCode(bannedIPs[0])).Returns("CN");
            _mockGeoIPService.Setup(x => x.GetCountryCode(bannedIPs[1])).Returns("US");
            _mockConfig.Setup(x => x.BlockedCountries).Returns(new[] { "CN" }.AsQueryable());

            // Act
            _evlWatcher.ApplyCountryRulesToExistingBans();

            // Assert
            _mockConfig.Verify(x => x.RemoveBlackListAddress(bannedIPs[0]), Times.Once);
            _mockConfig.Verify(x => x.RemoveBlackListAddress(bannedIPs[1]), Times.Never);
        }
    }
}
