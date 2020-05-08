using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Interfaces;
using TwitchChatBot.Shared.Services;
using Xunit;

namespace TwitchChatBot.UnitTests.Services
{
    public class StorageServiceTests
    {
        private Mock<ILogger<IStorageService>> _loggerMock;
        private Mock<IConfiguration> _configMock;
        private IStorageService sut;

        public StorageServiceTests()
        {
            _loggerMock = new Mock<ILogger<IStorageService>>();
            sut = new AzureTableStorageService(_loggerMock.Object, _configMock.Object);
        }

        [Fact]
        public void LoadBotSettings_Succeeds()
        {
            // arrange
            // act
            // assert
        }

        [Fact]
        public void SaveBotSettings_Succeeds()
        {
            // arrange
            // act
            // assert
        }

    }
}
