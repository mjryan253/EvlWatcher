using NUnit.Framework;
using Moq;
using EvlWatcher.Config;
using EvlWatcher.Logging;
using System.Linq;

namespace EvlWatcher.Tests.Unit.Config
{
    [TestFixture]
    public class XmlServiceConfigCountryTests
    {
        private Mock<ILogger> _mockLogger;
        private XmlServiceConfiguration _config;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            _config = new XmlServiceConfiguration(_mockLogger.Object);
        }

        [Test]
        public void CountryBlockingEnabled_DefaultValue_IsFalse()
        {
            // Act & Assert
            Assert.IsFalse(_config.CountryBlockingEnabled);
        }

        [Test]
        public void CountryBlockingEnabled_SetValue_UpdatesCorrectly()
        {
            // Act
            _config.CountryBlockingEnabled = true;

            // Assert
            Assert.IsTrue(_config.CountryBlockingEnabled);
        }

        [Test]
        public void BlockedCountries_DefaultValue_IsEmpty()
        {
            // Act & Assert
            Assert.AreEqual(0, _config.BlockedCountries.Count());
        }

        [Test]
        public void AddBlockedCountry_ValidCountryCode_AddsSuccessfully()
        {
            // Act
            bool result = _config.AddBlockedCountry("CN");

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, _config.BlockedCountries.Count());
            Assert.Contains("CN", _config.BlockedCountries.ToList());
        }

        [Test]
        public void AddBlockedCountry_InvalidCountryCode_ReturnsFalse()
        {
            // Act
            bool result = _config.AddBlockedCountry("INVALID");

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(0, _config.BlockedCountries.Count());
        }

        [Test]
        public void AddBlockedCountry_EmptyCountryCode_ReturnsFalse()
        {
            // Act
            bool result = _config.AddBlockedCountry("");

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(0, _config.BlockedCountries.Count());
        }

        [Test]
        public void AddBlockedCountry_NullCountryCode_ReturnsFalse()
        {
            // Act
            bool result = _config.AddBlockedCountry(null);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(0, _config.BlockedCountries.Count());
        }

        [Test]
        public void AddBlockedCountry_DuplicateCountryCode_ReturnsFalse()
        {
            // Arrange
            _config.AddBlockedCountry("CN");

            // Act
            bool result = _config.AddBlockedCountry("CN");

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(1, _config.BlockedCountries.Count());
        }

        [Test]
        public void AddBlockedCountry_LowercaseCountryCode_ConvertsToUppercase()
        {
            // Act
            bool result = _config.AddBlockedCountry("cn");

            // Assert
            Assert.IsTrue(result);
            Assert.Contains("CN", _config.BlockedCountries.ToList());
        }

        [Test]
        public void RemoveBlockedCountry_ExistingCountry_RemovesSuccessfully()
        {
            // Arrange
            _config.AddBlockedCountry("CN");

            // Act
            bool result = _config.RemoveBlockedCountry("CN");

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(0, _config.BlockedCountries.Count());
        }

        [Test]
        public void RemoveBlockedCountry_NonExistingCountry_ReturnsFalse()
        {
            // Act
            bool result = _config.RemoveBlockedCountry("CN");

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(0, _config.BlockedCountries.Count());
        }

        [Test]
        public void RemoveBlockedCountry_LowercaseCountryCode_RemovesSuccessfully()
        {
            // Arrange
            _config.AddBlockedCountry("CN");

            // Act
            bool result = _config.RemoveBlockedCountry("cn");

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(0, _config.BlockedCountries.Count());
        }

        [Test]
        public void MultipleBlockedCountries_AddAndRemove_WorksCorrectly()
        {
            // Arrange
            _config.AddBlockedCountry("CN");
            _config.AddBlockedCountry("RU");
            _config.AddBlockedCountry("KP");

            // Act & Assert
            Assert.AreEqual(3, _config.BlockedCountries.Count());
            Assert.Contains("CN", _config.BlockedCountries.ToList());
            Assert.Contains("RU", _config.BlockedCountries.ToList());
            Assert.Contains("KP", _config.BlockedCountries.ToList());

            // Remove one
            _config.RemoveBlockedCountry("RU");
            Assert.AreEqual(2, _config.BlockedCountries.Count());
            Assert.Contains("CN", _config.BlockedCountries.ToList());
            Assert.Contains("KP", _config.BlockedCountries.ToList());
        }
    }
}
