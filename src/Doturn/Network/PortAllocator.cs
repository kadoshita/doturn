using System;
using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Doturn.Network;

public interface IPortAllocator
{
    ushort GetPort();
    void ReleasePort(ushort port);
}
public class PortAllocateException : Exception
{
    public PortAllocateException() : base() { }
}
public class PortAllocator : IPortAllocator
{
    private readonly ILogger<PortAllocator> _logger;
    private readonly ConcurrentQueue<ushort> _availablePorts = new();

    public PortAllocator(ILogger<PortAllocator> logger, IOptions<AppSettings> options)
    {
        _logger = logger;
        var settings = options.Value;
        var activeUdpPorts = new HashSet<int>();
        foreach (var e in IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners())
        {
            activeUdpPorts.Add(e.Port);
        }
        for (int p = settings.MinPort; p <= settings.MaxPort; p++)
        {
            if (!activeUdpPorts.Contains(p))
            {
                _availablePorts.Enqueue((ushort)p);
            }
        }
    }

    public ushort GetPort()
    {
        if (!_availablePorts.TryDequeue(out ushort port))
        {
            throw new PortAllocateException();
        }
        _logger.LogDebug("GetPort: {Port}", port);
        return port;
    }

    public void ReleasePort(ushort port)
    {
        _availablePorts.Enqueue(port);
        _logger.LogDebug("ReleasePort: {Port}", port);
    }
}
