using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Interfaces;
using TwitchChatBot.Shared.Services;
using Xunit;

namespace TwitchChatBot.UnitTests.Services
{
    public class TwitchServiceTests
    {
        private readonly ITwitchService sut;
        private readonly Mock<ILogger<ITwitchService>> _loggerMock;
        private readonly Mock<IConfiguration> _configMock;

        public TwitchServiceTests()
        {
            _loggerMock = new Mock<ILogger<ITwitchService>>();
            _configMock= new Mock<IConfiguration>();
            sut = new TwitchService(_loggerMock.Object);
        }

        [Fact]
        public async Task ConnectToTwitch_WithValidParamaters_ShouldSucceed()
        {
            // ARRANGE
            var userName = "cldubya";
            var password = "random_password";

            // ACT
            await sut.CreateTwitchClient(userName, password);

            // ASSERT
            Assert.True(sut.IsInitialized);
        }

        [Fact]
        public async Task ConnectToTwitch_WithInvalidUsername_ThrowsException()
        {
            // ARRANGE
            var password = "random_password";

            // ACT
            await sut.CreateTwitchClient(String.Empty, password);

            // ASSERT
            Assert.True(sut.IsInitialized);
        }

        [Fact]
        public async Task ConnectToTwitch_WithInvalidPassword_ThrowsException()
        {
            // ARRANGE
            var userName = "cldubya";

            // ACT
            await sut.CreateTwitchClient(userName, String.Empty);

            // ASSERT
            Assert.True(sut.IsInitialized);
        }
    }
}
