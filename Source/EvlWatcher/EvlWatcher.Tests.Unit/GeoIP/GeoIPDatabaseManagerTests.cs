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
    public class GeoIPDatabaseManagerTests
    {
        private Mock<ILogger> _mockLogger;
        private Mock<IGeoLiteDownloader> _mockDownloader;
        private GeoIPDatabaseManager _manager;
        private string _testDatabasePath;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            _mockDownloader = new Mock<IGeoLiteDownloader>();
            _testDatabasePath = Path.Combine(Path.GetTempPath(), "test_geolite_manager.mmdb");
            _manager = new GeoIPDatabaseManager(_mockLogger.Object, _mockDownloader.Object, _testDatabasePath);
        }

        [TearDown]
        public void TearDown()
        {
            _manager?.Dispose();
            if (File.Exists(_testDatabasePath))
            {
                File.Delete(_testDatabasePath);
            }
            if (File.Exists(_testDatabasePath + ".backup"))
            {
                File.Delete(_testDatabasePath + ".backup");
            }
        }

        [Test]
        public void IsDatabaseAvailable_WithValidDatabase_ReturnsTrue()
        {
            // Arrange
            _mockDownloader.Setup(x => x.IsDatabaseValid(_testDatabasePath)).Returns(true);

            // Act
            bool result = _manager.IsDatabaseAvailable;

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsDatabaseAvailable_WithInvalidDatabase_ReturnsFalse()
        {
            // Arrange
            _mockDownloader.Setup(x => x.IsDatabaseValid(_testDatabasePath)).Returns(false);

            // Act
            bool result = _manager.IsDatabaseAvailable;

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void DatabasePath_ReturnsCorrectPath()
        {
            // Act & Assert
            Assert.AreEqual(_testDatabasePath, _manager.DatabasePath);
        }

        [Test]
        public async Task CheckAndUpdateDatabaseAsync_DatabaseUpToDate_ReturnsTrue()
        {
            // Arrange
            _mockDownloader.Setup(x => x.NeedsUpdate(_testDatabasePath, 30)).Returns(false);

            // Act
            bool result = await _manager.CheckAndUpdateDatabaseAsync();

            // Assert
            Assert.IsTrue(result);
            _mockDownloader.Verify(x => x.DownloadDatabaseAsync(_testDatabasePath), Times.Never);
        }

        [Test]
        public async Task CheckAndUpdateDatabaseAsync_DatabaseNeedsUpdate_SuccessfulDownload_ReturnsTrue()
        {
            // Arrange
            _mockDownloader.Setup(x => x.NeedsUpdate(_testDatabasePath, 30)).Returns(true);
            _mockDownloader.Setup(x => x.DownloadDatabaseAsync(_testDatabasePath)).ReturnsAsync(true);
            _mockDownloader.Setup(x => x.IsDatabaseValid(_testDatabasePath)).Returns(true);

            // Act
            bool result = await _manager.CheckAndUpdateDatabaseAsync();

            // Assert
            Assert.IsTrue(result);
            _mockDownloader.Verify(x => x.DownloadDatabaseAsync(_testDatabasePath), Times.Once);
        }

        [Test]
        public async Task CheckAndUpdateDatabaseAsync_DatabaseNeedsUpdate_FailedDownload_ReturnsFalse()
        {
            // Arrange
            _mockDownloader.Setup(x => x.NeedsUpdate(_testDatabasePath, 30)).Returns(true);
            _mockDownloader.Setup(x => x.DownloadDatabaseAsync(_testDatabasePath)).ReturnsAsync(false);

            // Act
            bool result = await _manager.CheckAndUpdateDatabaseAsync();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public async Task CheckAndUpdateDatabaseAsync_WithExistingDatabase_CreatesBackup()
        {
            // Arrange
            File.WriteAllText(_testDatabasePath, "Existing database content");
            _mockDownloader.Setup(x => x.NeedsUpdate(_testDatabasePath, 30)).Returns(true);
            _mockDownloader.Setup(x => x.DownloadDatabaseAsync(_testDatabasePath)).ReturnsAsync(true);
            _mockDownloader.Setup(x => x.IsDatabaseValid(_testDatabasePath)).Returns(true);

            // Act
            await _manager.CheckAndUpdateDatabaseAsync();

            // Assert
            Assert.IsTrue(File.Exists(_testDatabasePath + ".backup"));
            var backupContent = File.ReadAllText(_testDatabasePath + ".backup");
            Assert.AreEqual("Existing database content", backupContent);
        }

        [Test]
        public async Task CheckAndUpdateDatabaseAsync_FailedDownload_RestoresBackup()
        {
            // Arrange
            File.WriteAllText(_testDatabasePath, "Original database content");
            _mockDownloader.Setup(x => x.NeedsUpdate(_testDatabasePath, 30)).Returns(true);
            _mockDownloader.Setup(x => x.DownloadDatabaseAsync(_testDatabasePath)).ReturnsAsync(false);

            // Act
            await _manager.CheckAndUpdateDatabaseAsync();

            // Assert
            // Original content should still be there since download failed
            var content = File.ReadAllText(_testDatabasePath);
            Assert.AreEqual("Original database content", content);
        }

        [Test]
        public async Task ForceUpdateAsync_SuccessfulDownload_ReturnsTrue()
        {
            // Arrange
            _mockDownloader.Setup(x => x.DownloadDatabaseAsync(_testDatabasePath)).ReturnsAsync(true);

            // Act
            bool result = await _manager.ForceUpdateAsync();

            // Assert
            Assert.IsTrue(result);
            _mockDownloader.Verify(x => x.DownloadDatabaseAsync(_testDatabasePath), Times.Once);
        }

        [Test]
        public async Task ForceUpdateAsync_FailedDownload_ReturnsFalse()
        {
            // Arrange
            _mockDownloader.Setup(x => x.DownloadDatabaseAsync(_testDatabasePath)).ReturnsAsync(false);

            // Act
            bool result = await _manager.ForceUpdateAsync();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Constructor_WithCustomParameters_SetsCorrectValues()
        {
            // Arrange
            var customPath = "custom/path.mmdb";
            var customInterval = 12;
            var customMaxAge = 7;

            // Act
            var customManager = new GeoIPDatabaseManager(
                _mockLogger.Object, 
                _mockDownloader.Object, 
                customPath, 
                customInterval, 
                customMaxAge);

            // Assert
            Assert.AreEqual(customPath, customManager.DatabasePath);
            customManager.Dispose();
        }

        [Test]
        public void Dispose_DisposesDownloader()
        {
            // Arrange
            var disposableDownloader = _mockDownloader.As<IDisposable>();

            // Act
            _manager.Dispose();

            // Assert
            disposableDownloader.Verify(x => x.Dispose(), Times.Once);
        }
    }
}
