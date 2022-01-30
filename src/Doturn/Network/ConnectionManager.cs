using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Doturn.Network
{
    public class ConnectionEntry
    {
        public IPAddress address { get; set; }
        public ushort port { get; set; }
        public IPAddress remoteAddress { get; set; }
        public ushort remotePort { get; set; }

        public ConnectionEntry(IPAddress address, ushort port, IPAddress remoteAddress, ushort remotePort)
        {
            this.address = address;
            this.port = port;
            this.remoteAddress = remoteAddress;
            this.remotePort = remotePort;
        }
    }

    public interface IConnectionManager
    {

        void AddConnectionEntry(ConnectionEntry entry);
        int GetEntriesCount();
    }
    public class ConnectionManager : IConnectionManager
    {
        public readonly List<ConnectionEntry> _entries = new();
        private readonly ILogger<ConnectionManager> _logger;

        public ConnectionManager(ILogger<ConnectionManager> logger)
        {
            _logger = logger;
        }
        public void AddConnectionEntry(ConnectionEntry entry)
        {
            _logger.LogDebug($"Add entry {entry.remoteAddress}:{entry.remotePort} - {entry.address}:{entry.port}");
            _entries.Add(entry);
            _logger.LogDebug($"Entries: {_entries.Count}");
        }
        public int GetEntriesCount()
        {
            return _entries.Count;
        }
    }
}