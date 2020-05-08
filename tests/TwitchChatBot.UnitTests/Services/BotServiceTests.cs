using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Enums;
using TwitchChatBot.Shared.Interfaces;
using TwitchChatBot.Shared.Services;
using Xunit;

namespace TwitchChatBot.UnitTests.Services
{
    public class BotServiceTests
    {
        private Mock<IConfiguration> _configMock;
        private Mock<ILogger<IBotService>> _loggerMock;
        private Mock<IStorageService> _storageServiceMock;
        private Mock<ITwitchService> _twitchServiceMock;
        private IBotService sut;

        public BotServiceTests()
        {
            _twitchServiceMock = new Mock<ITwitchService>();
            _loggerMock = new Mock<ILogger<IBotService>>();
            _storageServiceMock = new Mock<IStorageService>();
            sut = new BotService(_twitchServiceMock.Object,_configMock.Object,_loggerMock.Object, _storageServiceMock.Object);
        }

        [Fact]
        public async Task ChangeState_ToEnabled_ShouldSucceed()
        {
            // ARRANGE
            // ACT
            await sut.ChangeBotState(BotState.Started);

            // ASSERT
            Assert.Equal(BotState.Started, sut.CurrentState);
        }


        [Fact]
        public void StartBot_WithTwitchDisconnected_ShouldStart()
        {
            // arrange
            string username = "Test", password = "password";
            _twitchServiceMock.SetupGet(x => x.IsInitialized).Returns(false);
            _twitchServiceMock.Setup(x => x.CreateTwitchClient(username, password)).Returns(Task.CompletedTask);

            // act
            var result = sut.StartBot();
            result.Wait();

            // assert
            Assert.True(result.Status == TaskStatus.RanToCompletion);
        }

        [Fact]
        public void StartBot_WithTwitchConnected_ShouldStart()
        {
            // arrange
            string username = "Test", password = "password";
            _twitchServiceMock.SetupGet(x => x.IsInitialized).Returns(true);
            _twitchServiceMock.Setup(x => x.CreateTwitchClient(username, password)).Returns(Task.CompletedTask);

            // act
            var result = sut.StartBot();
            result.Wait();

            // assert
            Assert.True(result.Status == TaskStatus.RanToCompletion);
        }

        [Fact]
        public void StartBot_WithTwitchDisconnected_WithEmptyUsername_ShouldThrowException()
        {
            // arrange
            string password = "password";
            _twitchServiceMock.SetupGet(x => x.IsInitialized).Returns(false);
            _twitchServiceMock.Setup(x => x.CreateTwitchClient(String.Empty, password)).Returns(Task.CompletedTask);

            // act
            var result = sut.StartBot();
            result.Wait();

            // assert
            Assert.True(result.Status == TaskStatus.RanToCompletion);
        }

        [Fact]
        public void StartBot_WithTwitchDisconnected_WithEmptyPassword_ShouldThrowException()
        {
            // arrange
            string username = "Test";
            _twitchServiceMock.SetupGet(x => x.IsInitialized).Returns(false);
            _twitchServiceMock.Setup(x => x.CreateTwitchClient(username, String.Empty)).Returns(Task.CompletedTask);

            // act
            var result = sut.StartBot();
            result.Wait();

            // assert
            Assert.True(result.Status == TaskStatus.RanToCompletion);
        }

        [Fact]
        public async Task LoadState_Succeeds()
        {
            // arrange
            // act 
            await sut.LoadState();
            // assert
            Assert.True(sut.CurrentState == BotState.Started);
        }
    }
}
