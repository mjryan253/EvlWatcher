using NUnit.Framework;
using Moq;
using EvlWatcherConsole.Model;
using EvlWatcherConsole.ViewModel;
using EvlWatcher.WCF.DTO;
using System.Collections.Generic;
using System.Linq;

namespace EvlWatcher.Tests.Unit.Console
{
    [TestFixture]
    public class CountryBlockingViewModelTests
    {
        private Mock<EvlWatcherModel> _mockModel;
        private MainWindowViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            _mockModel = new Mock<EvlWatcherModel>();
            _viewModel = new MainWindowViewModel();
            
            // Use reflection to set the private _model field
            var modelField = typeof(MainWindowViewModel).GetField("_model", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(_viewModel, _mockModel.Object);
        }

        [Test]
        public void CountryBlockingEnabled_DefaultValue_IsFalse()
        {
            // Act & Assert
            Assert.IsFalse(_viewModel.CountryBlockingEnabled);
        }

        [Test]
        public void CountryBlockingEnabled_SetValue_UpdatesCorrectly()
        {
            // Act
            _viewModel.CountryBlockingEnabled = true;

            // Assert
            Assert.IsTrue(_viewModel.CountryBlockingEnabled);
        }

        [Test]
        public void BlockedCountries_DefaultValue_IsEmpty()
        {
            // Act & Assert
            Assert.AreEqual(0, _viewModel.BlockedCountries.Count);
        }

        [Test]
        public void NewCountryCode_DefaultValue_IsEmpty()
        {
            // Act & Assert
            Assert.AreEqual("", _viewModel.NewCountryCode);
        }

        [Test]
        public void NewCountryCode_SetValue_UpdatesCorrectly()
        {
            // Act
            _viewModel.NewCountryCode = "CN";

            // Assert
            Assert.AreEqual("CN", _viewModel.NewCountryCode);
        }

        [Test]
        public void SelectedCountry_DefaultValue_IsEmpty()
        {
            // Act & Assert
            Assert.AreEqual("", _viewModel.SelectedCountry);
        }

        [Test]
        public void SelectedCountry_SetValue_UpdatesCorrectly()
        {
            // Act
            _viewModel.SelectedCountry = "CN";

            // Assert
            Assert.AreEqual("CN", _viewModel.SelectedCountry);
        }

        [Test]
        public void ToggleCountryBlockingCommand_WhenEnabled_DisablesCountryBlocking()
        {
            // Arrange
            _viewModel.CountryBlockingEnabled = true;
            _mockModel.Setup(x => x.GetCountryBlockingEnabled()).Returns(true);

            // Act
            var command = _viewModel.ToggleCountryBlockingCommand;
            command.Execute(null);

            // Assert
            _mockModel.Verify(x => x.SetCountryBlockingEnabled(false), Times.Once);
        }

        [Test]
        public void ToggleCountryBlockingCommand_WhenDisabled_EnablesCountryBlocking()
        {
            // Arrange
            _viewModel.CountryBlockingEnabled = false;
            _mockModel.Setup(x => x.GetCountryBlockingEnabled()).Returns(false);

            // Act
            var command = _viewModel.ToggleCountryBlockingCommand;
            command.Execute(null);

            // Assert
            _mockModel.Verify(x => x.SetCountryBlockingEnabled(true), Times.Once);
        }

        [Test]
        public void AddBlockedCountryCommand_ValidCountryCode_AddsCountry()
        {
            // Arrange
            _viewModel.NewCountryCode = "CN";
            _viewModel.BlockedCountries.Add("RU"); // Existing country
            _mockModel.Setup(x => x.GetBlockedCountries()).Returns(new[] { "RU", "CN" });

            // Act
            var command = _viewModel.AddBlockedCountryCommand;
            command.Execute(null);

            // Assert
            _mockModel.Verify(x => x.SetBlockedCountries(It.IsAny<string[]>()), Times.Once);
            Assert.AreEqual("", _viewModel.NewCountryCode); // Should be cleared
        }

        [Test]
        public void AddBlockedCountryCommand_InvalidCountryCode_DoesNothing()
        {
            // Arrange
            _viewModel.NewCountryCode = "INVALID";

            // Act
            var command = _viewModel.AddBlockedCountryCommand;
            command.Execute(null);

            // Assert
            _mockModel.Verify(x => x.SetBlockedCountries(It.IsAny<string[]>()), Times.Never);
        }

        [Test]
        public void AddBlockedCountryCommand_EmptyCountryCode_DoesNothing()
        {
            // Arrange
            _viewModel.NewCountryCode = "";

            // Act
            var command = _viewModel.AddBlockedCountryCommand;
            command.Execute(null);

            // Assert
            _mockModel.Verify(x => x.SetBlockedCountries(It.IsAny<string[]>()), Times.Never);
        }

        [Test]
        public void RemoveBlockedCountryCommand_WithSelectedCountry_RemovesCountry()
        {
            // Arrange
            _viewModel.SelectedCountry = "CN";
            _viewModel.BlockedCountries.Add("CN");
            _viewModel.BlockedCountries.Add("RU");

            // Act
            var command = _viewModel.RemoveBlockedCountryCommand;
            command.Execute(null);

            // Assert
            _mockModel.Verify(x => x.SetBlockedCountries(It.IsAny<string[]>()), Times.Once);
        }

        [Test]
        public void RemoveBlockedCountryCommand_NoSelectedCountry_DoesNothing()
        {
            // Arrange
            _viewModel.SelectedCountry = "";

            // Act
            var command = _viewModel.RemoveBlockedCountryCommand;
            command.Execute(null);

            // Assert
            _mockModel.Verify(x => x.SetBlockedCountries(It.IsAny<string[]>()), Times.Never);
        }

        [Test]
        public void ApplyCountryRulesCommand_CallsModelMethod()
        {
            // Act
            var command = _viewModel.ApplyCountryRulesCommand;
            command.Execute(null);

            // Assert
            _mockModel.Verify(x => x.ApplyCountryRulesToExistingBans(), Times.Once);
        }

        [Test]
        public void AddBlockedCountryCommand_CanExecute_WithValidCountryCode_ReturnsTrue()
        {
            // Arrange
            _viewModel.NewCountryCode = "CN";

            // Act
            var command = _viewModel.AddBlockedCountryCommand;
            bool canExecute = command.CanExecute(null);

            // Assert
            Assert.IsTrue(canExecute);
        }

        [Test]
        public void AddBlockedCountryCommand_CanExecute_WithInvalidCountryCode_ReturnsFalse()
        {
            // Arrange
            _viewModel.NewCountryCode = "INVALID";

            // Act
            var command = _viewModel.AddBlockedCountryCommand;
            bool canExecute = command.CanExecute(null);

            // Assert
            Assert.IsFalse(canExecute);
        }

        [Test]
        public void RemoveBlockedCountryCommand_CanExecute_WithSelectedCountry_ReturnsTrue()
        {
            // Arrange
            _viewModel.SelectedCountry = "CN";

            // Act
            var command = _viewModel.RemoveBlockedCountryCommand;
            bool canExecute = command.CanExecute(null);

            // Assert
            Assert.IsTrue(canExecute);
        }

        [Test]
        public void RemoveBlockedCountryCommand_CanExecute_WithoutSelectedCountry_ReturnsFalse()
        {
            // Arrange
            _viewModel.SelectedCountry = "";

            // Act
            var command = _viewModel.RemoveBlockedCountryCommand;
            bool canExecute = command.CanExecute(null);

            // Assert
            Assert.IsFalse(canExecute);
        }
    }
}
