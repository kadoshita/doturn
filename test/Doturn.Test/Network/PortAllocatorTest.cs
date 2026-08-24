using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Doturn.Network.Test;

public class PortAllocatorTest
{
    private readonly ILogger<StunServerService.StunServerService> logger;
    private readonly IConnectionManager connectionManager;

    public PortAllocatorTest()
    {
        logger = Mock.Of<ILogger<StunServerService.StunServerService>>();
        connectionManager = Mock.Of<IConnectionManager>();
    }

    [Fact]
    public void GetPort_Returns_Port_Within_MinMax_Range()
    {
        var appSettings = new AppSettings
        {
            MinPort = 49152,
            MaxPort = 65535
        };
        var options = Options.Create(appSettings);
        var portAllocator = new PortAllocator(logger, options, connectionManager);

        ushort port = portAllocator.GetPort();

        Assert.InRange(port, appSettings.MinPort, appSettings.MaxPort);
    }

    [Fact]
    public void GetPort_With_Single_Port_Range_Returns_That_Port()
    {
        // Regression test: Enumerable.Range's second argument is a count, not an
        // upper bound. If MaxPort is passed directly as the count (the original
        // bug), a single-port range would incorrectly expand far past MaxPort.
        var appSettings = new AppSettings
        {
            MinPort = 61234,
            MaxPort = 61234
        };
        var options = Options.Create(appSettings);
        var portAllocator = new PortAllocator(logger, options, connectionManager);

        ushort port = portAllocator.GetPort();

        Assert.Equal((ushort)61234, port);
    }
}
