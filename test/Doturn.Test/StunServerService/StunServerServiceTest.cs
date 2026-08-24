using System.Net.Sockets;
using Doturn.Network;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Doturn.StunServerService.Test
{
    public class StunServerServiceTest
    {
        private readonly ILogger<StunServerService> _logger;
        private readonly IPortAllocator _portAllocator;
        private readonly AppSettings _appSettings;

        public StunServerServiceTest()
        {
            _logger = Mock.Of<ILogger<StunServerService>>();
            _portAllocator = Mock.Of<IPortAllocator>();
            _appSettings = new AppSettings
            {
                Username = "username",
                Password = "password",
                Realm = "example.com",
                ExternalIPAddress = "127.0.0.1",
                ListeningPort = 0, // ephemeral port, avoids port collisions between test runs
                MinPort = 49152,
                MaxPort = 65535
            };
        }

        [Fact]
        public void Constructor_Uses_ListeningPort_From_Options_And_Sets_Main_Client()
        {
            var connectionManagerMock = new Mock<IConnectionManager>();
            var options = Options.Create(_appSettings);

            var service = new StunServerService(_logger, options, connectionManagerMock.Object, _portAllocator);

            Assert.Equal(_appSettings.ListeningPort, service.listenPort);
            connectionManagerMock.Verify(m => m.SetMainClient(It.IsAny<UdpClient>()), Times.Once);
            Assert.NotNull(((IStunServerService)service)._client);
        }

        [Fact]
        public void Constructor_With_Explicit_ListenPort_Does_Not_Set_Main_Client()
        {
            var connectionManagerMock = new Mock<IConnectionManager>();
            var options = Options.Create(_appSettings);
            const ushort explicitPort = 0; // ephemeral port

            var service = new StunServerService(_logger, options, connectionManagerMock.Object, _portAllocator, explicitPort);

            Assert.Equal(explicitPort, service.listenPort);
            connectionManagerMock.Verify(m => m.SetMainClient(It.IsAny<UdpClient>()), Times.Never);
            Assert.NotNull(((IStunServerService)service)._client);
        }
    }
}
