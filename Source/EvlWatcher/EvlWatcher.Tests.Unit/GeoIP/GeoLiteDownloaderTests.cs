using NUnit.Framework;
using Moq;
using EvlWatcher.GeoIP;
using EvlWatcher.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EvlWatcher.Tests.Unit.GeoIP
{
    [TestFixture]
    public class GeoLiteDownloaderTests
    {
        private Mock<ILogger> _mockLogger;
        private GeoLiteDownloader _downloader;
        private string _testDatabasePath;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            _downloader = new GeoLiteDownloader(_mockLogger.Object);
            _testDatabasePath = Path.Combine(Path.GetTempPath(), "test_geolite.mmdb");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_testDatabasePath))
            {
                File.Delete(_testDatabasePath);
            }
        }

        [Test]
        public async Task DownloadDatabaseAsync_ValidPath_CreatesMockDatabase()
        {
            // Act
            bool result = await _downloader.DownloadDatabaseAsync(_testDatabasePath);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(_testDatabasePath));
        }

        [Test]
        public async Task DownloadDatabaseAsync_InvalidPath_ReturnsFalse()
        {
            // Arrange
            string invalidPath = "Z:\\Invalid\\Path\\That\\Does\\Not\\Exist\\database.mmdb";

            // Act
            bool result = await _downloader.DownloadDatabaseAsync(invalidPath);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsDatabaseValid_WithValidDatabase_ReturnsTrue()
        {
            // Arrange
            File.WriteAllText(_testDatabasePath, "GeoLite2-Country Database\nTest content");

            // Act
            bool result = _downloader.IsDatabaseValid(_testDatabasePath);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsDatabaseValid_WithInvalidDatabase_ReturnsFalse()
        {
            // Arrange
            File.WriteAllText(_testDatabasePath, "Invalid database content");

            // Act
            bool result = _downloader.IsDatabaseValid(_testDatabasePath);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsDatabaseValid_WithNonExistentFile_ReturnsFalse()
        {
            // Act
            bool result = _downloader.IsDatabaseValid(_testDatabasePath);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsDatabaseValid_WithEmptyFile_ReturnsFalse()
        {
            // Arrange
            File.WriteAllText(_testDatabasePath, "");

            // Act
            bool result = _downloader.IsDatabaseValid(_testDatabasePath);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetDatabaseLastModified_WithExistingFile_ReturnsDate()
        {
            // Arrange
            File.WriteAllText(_testDatabasePath, "Test content");
            var expectedDate = File.GetLastWriteTimeUtc(_testDatabasePath);

            // Act
            var result = _downloader.GetDatabaseLastModified(_testDatabasePath);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedDate, result.Value, TimeSpan.FromSeconds(1));
        }

        [Test]
        public void GetDatabaseLastModified_WithNonExistentFile_ReturnsNull()
        {
            // Act
            var result = _downloader.GetDatabaseLastModified(_testDatabasePath);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void NeedsUpdate_WithNonExistentFile_ReturnsTrue()
        {
            // Act
            bool result = _downloader.NeedsUpdate(_testDatabasePath);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void NeedsUpdate_WithRecentFile_ReturnsFalse()
        {
            // Arrange
            File.WriteAllText(_testDatabasePath, "GeoLite2-Country Database\nTest content");

            // Act
            bool result = _downloader.NeedsUpdate(_testDatabasePath, 30);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void NeedsUpdate_WithOldFile_ReturnsTrue()
        {
            // Arrange
            File.WriteAllText(_testDatabasePath, "GeoLite2-Country Database\nTest content");
            var oldDate = DateTime.UtcNow.AddDays(-60);
            File.SetLastWriteTimeUtc(_testDatabasePath, oldDate);

            // Act
            bool result = _downloader.NeedsUpdate(_testDatabasePath, 30);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void NeedsUpdate_WithCustomMaxAge_RespectsMaxAge()
        {
            // Arrange
            File.WriteAllText(_testDatabasePath, "GeoLite2-Country Database\nTest content");
            var oldDate = DateTime.UtcNow.AddDays(-10);
            File.SetLastWriteTimeUtc(_testDatabasePath, oldDate);

            // Act
            bool result = _downloader.NeedsUpdate(_testDatabasePath, 5); // 5 days max age

            // Assert
            Assert.IsTrue(result);
        }
    }
}
