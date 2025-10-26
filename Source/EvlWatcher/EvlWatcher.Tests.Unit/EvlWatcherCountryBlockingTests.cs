using NUnit.Framework;
using Moq;
using EvlWatcher;
using EvlWatcher.Config;
using EvlWatcher.GeoIP;
using EvlWatcher.Logging;
using EvlWatcher.Tasks;
using System.Net;

namespace EvlWatcher.Tests.Unit
{
    [TestFixture]
    public class EvlWatcherCountryBlockingTests
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
        public void IsCountryBlocked_CountryBlockingDisabled_ReturnsFalse()
        {
            // Arrange
            _mockConfig.Setup(x => x.CountryBlockingEnabled).Returns(false);
            var testIP = IPAddress.Parse("8.8.8.8");

            // Act
            var result = _evlWatcher.GetType()
                .GetMethod("IsCountryBlocked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_evlWatcher, new object[] { testIP });

            // Assert
            Assert.IsFalse((bool)result);
        }

        [Test]
        public void IsCountryBlocked_GeoIPServiceUnavailable_ReturnsFalse()
        {
            // Arrange
            _mockConfig.Setup(x => x.CountryBlockingEnabled).Returns(true);
            _mockGeoIPService.Setup(x => x.IsAvailable).Returns(false);
            var testIP = IPAddress.Parse("8.8.8.8");

            // Act
            var result = _evlWatcher.GetType()
                .GetMethod("IsCountryBlocked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_evlWatcher, new object[] { testIP });

            // Assert
            Assert.IsFalse((bool)result);
        }

        [Test]
        public void IsCountryBlocked_CountryNotBlocked_ReturnsFalse()
        {
            // Arrange
            _mockConfig.Setup(x => x.CountryBlockingEnabled).Returns(true);
            _mockGeoIPService.Setup(x => x.IsAvailable).Returns(true);
            _mockGeoIPService.Setup(x => x.GetCountryCode(It.IsAny<IPAddress>())).Returns("US");
            _mockConfig.Setup(x => x.BlockedCountries).Returns(new[] { "CN", "RU" }.AsQueryable());
            var testIP = IPAddress.Parse("8.8.8.8");

            // Act
            var result = _evlWatcher.GetType()
                .GetMethod("IsCountryBlocked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_evlWatcher, new object[] { testIP });

            // Assert
            Assert.IsFalse((bool)result);
        }

        [Test]
        public void IsCountryBlocked_CountryIsBlocked_ReturnsTrue()
        {
            // Arrange
            _mockConfig.Setup(x => x.CountryBlockingEnabled).Returns(true);
            _mockGeoIPService.Setup(x => x.IsAvailable).Returns(true);
            _mockGeoIPService.Setup(x => x.GetCountryCode(It.IsAny<IPAddress>())).Returns("CN");
            _mockConfig.Setup(x => x.BlockedCountries).Returns(new[] { "CN", "RU" }.AsQueryable());
            var testIP = IPAddress.Parse("8.8.8.8");

            // Act
            var result = _evlWatcher.GetType()
                .GetMethod("IsCountryBlocked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_evlWatcher, new object[] { testIP });

            // Assert
            Assert.IsTrue((bool)result);
        }

        [Test]
        public void IsCountryBlocked_CountryLookupFails_ReturnsFalse()
        {
            // Arrange
            _mockConfig.Setup(x => x.CountryBlockingEnabled).Returns(true);
            _mockGeoIPService.Setup(x => x.IsAvailable).Returns(true);
            _mockGeoIPService.Setup(x => x.GetCountryCode(It.IsAny<IPAddress>())).Returns((string)null);
            var testIP = IPAddress.Parse("8.8.8.8");

            // Act
            var result = _evlWatcher.GetType()
                .GetMethod("IsCountryBlocked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_evlWatcher, new object[] { testIP });

            // Assert
            Assert.IsFalse((bool)result);
        }

        [Test]
        public void IsCountryBlocked_GeoIPServiceThrows_ReturnsFalse()
        {
            // Arrange
            _mockConfig.Setup(x => x.CountryBlockingEnabled).Returns(true);
            _mockGeoIPService.Setup(x => x.IsAvailable).Returns(true);
            _mockGeoIPService.Setup(x => x.GetCountryCode(It.IsAny<IPAddress>())).Throws(new System.Exception("Test exception"));
            var testIP = IPAddress.Parse("8.8.8.8");

            // Act
            var result = _evlWatcher.GetType()
                .GetMethod("IsCountryBlocked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_evlWatcher, new object[] { testIP });

            // Assert
            Assert.IsFalse((bool)result);
            _mockLogger.Verify(x => x.Dump(It.Is<string>(s => s.Contains("Failed to check country")), SeverityLevel.Debug), Times.Once);
        }

        [Test]
        public void IsCountryBlocked_CaseInsensitiveCountryCode_WorksCorrectly()
        {
            // Arrange
            _mockConfig.Setup(x => x.CountryBlockingEnabled).Returns(true);
            _mockGeoIPService.Setup(x => x.IsAvailable).Returns(true);
            _mockGeoIPService.Setup(x => x.GetCountryCode(It.IsAny<IPAddress>())).Returns("cn");
            _mockConfig.Setup(x => x.BlockedCountries).Returns(new[] { "CN", "RU" }.AsQueryable());
            var testIP = IPAddress.Parse("8.8.8.8");

            // Act
            var result = _evlWatcher.GetType()
                .GetMethod("IsCountryBlocked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_evlWatcher, new object[] { testIP });

            // Assert
            Assert.IsTrue((bool)result);
        }
    }
}
