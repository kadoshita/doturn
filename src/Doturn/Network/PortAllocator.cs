using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Doturn.Network
{
    public interface IPortAllocator
    {
        ushort GetPort();
    }
    public class PortAllocateException : Exception
    {
        public PortAllocateException() : base() { }
    }
    public class PortAllocator : IPortAllocator
    {
        private readonly ILogger<StunServerService.StunServerService> _logger;
        private readonly IOptions<AppSettings> _options;
        private readonly IConnectionManager _connectionManager;
        public PortAllocator(ILogger<StunServerService.StunServerService> logger, IOptions<AppSettings> options, IConnectionManager connectionManager)
        {
            _logger = logger;
            _options = options;
            _connectionManager = connectionManager;
        }
        public ushort GetPort()
        {
            _logger.LogDebug($"Entries {_connectionManager.GetEntriesCount()} MinPort  {_options.Value.MinPort}");
            ushort port = (ushort)(_connectionManager.GetEntriesCount() + _options.Value.MinPort);
            if (port > _options.Value.MaxPort)
            {
                throw new PortAllocateException();
            }
            return port;
        }
    }
}