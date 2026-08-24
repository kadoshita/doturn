using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Doturn.Network;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Doturn.StunServerService.Test;

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

    [Fact]
    public async Task Responds_To_Binding_Request_Over_Udp()
    {
        var connectionManager = Mock.Of<IConnectionManager>();
        var options = Options.Create(_appSettings);
        var service = new StunServerService(_logger, options, connectionManager, _portAllocator);
        int boundPort = ((IPEndPoint)((IStunServerService)service)._client.Client.LocalEndPoint!).Port;

        // BackgroundService.StartAsync begins running ExecuteAsync on a background task.
        _ = service.StartAsync(default);

        using var client = new UdpClient(0);
        var serverEndpoint = new IPEndPoint(IPAddress.Loopback, boundPort);

        byte[] transactionId = new byte[] { 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58 };
        byte[] bindingRequest = new byte[20];
        ByteArrayUtils.MergeByteArray(ref bindingRequest,
            new byte[] { 0x00, 0x01, 0x00, 0x00 }, // type: BINDING, length: 0
            new byte[] { 0x21, 0x12, 0xa4, 0x42 }, // magic cookie
            transactionId);

        await client.SendAsync(bindingRequest, bindingRequest.Length, serverEndpoint);

        Task<UdpReceiveResult> receiveTask = client.ReceiveAsync();
        Task completedTask = await Task.WhenAny(receiveTask, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.Same(receiveTask, completedTask);

        byte[] response = (await receiveTask).Buffer;
        Assert.Equal(new byte[] { 0x01, 0x01 }, response[0..2]); // BINDING_SUCCESS
        Assert.Equal(transactionId, response[8..20]);
    }
}
