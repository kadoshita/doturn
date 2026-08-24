using System;
using System.Linq;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Doturn.Network;

public interface IPortAllocator
{
    ushort GetPort();
}
public class PortAllocateException : Exception
{
    public PortAllocateException() : base() { }
}
public class PortAllocator(ILogger<PortAllocator> logger, IOptions<AppSettings> options, IConnectionManager connectionManager) : IPortAllocator
{
    private readonly ILogger<PortAllocator> _logger = logger;
    private readonly IOptions<AppSettings> _options = options;
    private readonly IConnectionManager _connectionManager = connectionManager;

    public ushort GetPort()
    {
        //ref:https://gist.github.com/jrusbatch/4211535?permalink_comment_id=3504205#gistcomment-3504205
        var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        var udpEndpoints = ipGlobalProperties.GetActiveUdpListeners();
        var notAvailablePorts = udpEndpoints.Select(e => e.Port);
        var port = (ushort)Enumerable.Range(_options.Value.MinPort, _options.Value.MaxPort - _options.Value.MinPort + 1).Except(notAvailablePorts).FirstOrDefault();
        _logger.LogDebug("GetPort: {Port}", port);
        return port;
    }
}
