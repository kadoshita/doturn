using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Doturn.Network
{
    public class ConnectionEntry
    {
        public IPEndPoint client { get; set; }
        public IPEndPoint? peer { get; set; }
        public byte[]? channelNumber { get; set; }
        public StunServerService.IStunServerService sss { get; set; }

        public ConnectionEntry(IPAddress clientAddress, ushort clientPort, StunServerService.IStunServerService sss)
        {
            client = new IPEndPoint(clientAddress, clientPort);
            this.sss = sss;
        }
        public ConnectionEntry(IPEndPoint client, StunServerService.IStunServerService sss)
        {
            this.client = client;
            this.sss = sss;
        }
    }

    public interface IConnectionManager
    {
        void SetMainClient(UdpClient client);
        Task<int> SendMainClientAsync(byte[] data, int length, IPEndPoint endpoint);
        void AddConnectionEntry(ConnectionEntry entry);
        void AddPeerEndpoint(IPEndPoint client, IPEndPoint peer);
        void AddChannelNumber(IPEndPoint client, byte[] channelNumber);
        void DeleteEntry(IPEndPoint client);
        ConnectionEntry GetEntry(IPEndPoint endpoint);
        ConnectionEntry GetEntryByPeer(IPEndPoint endpoint);
        ConnectionEntry GetEntryByChannelNumber(byte[] channelNumber);
        int GetEntriesCount();

    }
    public class ConnectionManager : IConnectionManager
    {
        // TODO Dictionaryを使う
        public readonly List<ConnectionEntry> _entries = new();
        private readonly ILogger<ConnectionManager> _logger;

        public UdpClient mainClient;

        public ConnectionManager(ILogger<ConnectionManager> logger)
        {
            _logger = logger;
        }

        public void SetMainClient(UdpClient client)
        {
            if (mainClient == null)
            {
                mainClient = client;
            }
        }

        public Task<int> SendMainClientAsync(byte[] data, int length, IPEndPoint endpoint)
        {
            return mainClient.SendAsync(data, length, endpoint);
        }
        public void AddConnectionEntry(ConnectionEntry entry)
        {
            _logger.LogDebug($"Add entry {entry.client.Address}:{entry.client.Port}");
            _entries.Add(entry);
            _logger.LogDebug($"Entries: {_entries.Count}");
        }

        public void AddPeerEndpoint(IPEndPoint client, IPEndPoint peer)
        {
            _logger.LogDebug($"Add peer {client.Address}:{client.Port} - {peer.Address}:{peer.Port}");
            ConnectionEntry entry = _entries.Find(e => e.client.Equals(client));
            entry.peer = peer;
        }

        public void AddChannelNumber(IPEndPoint client, byte[] channelNumber)
        {
            _logger.LogDebug($"Add channel number {client.Address}:{client.Port} - {BitConverter.ToString(channelNumber)}");
            ConnectionEntry entry = _entries.Find(e => e.client.Equals(client));
            entry.channelNumber = channelNumber;
        }

        public int GetEntriesCount()
        {
            return _entries.Count;
        }

        public ConnectionEntry GetEntry(IPEndPoint endpoint)
        {
            _logger.LogDebug($"Get entry {endpoint.Address} {endpoint.Port}");
            ConnectionEntry entry = _entries.Find(e => e.client.Equals(endpoint));
            _logger.LogDebug($"Entry: {entry}");
            return entry;
        }

        public ConnectionEntry GetEntryByPeer(IPEndPoint endpoint)
        {
            _logger.LogDebug($"Get entry by Peer {endpoint.Address} {endpoint.Port}");
            ConnectionEntry entry = _entries.Find(e =>
            {
                if (e.peer == null)
                {
                    return false;
                }
                return e.peer.Equals(endpoint);
            });
            if (entry != null)
            {
                _logger.LogDebug($"Entry {entry.client.Address}:{entry.client.Port} - {entry.peer.Address}:{entry.peer.Port}");
            }
            return entry;
        }

        public ConnectionEntry GetEntryByChannelNumber(byte[] channelNumber)
        {
            _logger.LogDebug($"Get entry by Channel Number {BitConverter.ToString(channelNumber)}");
            ConnectionEntry entry = _entries.Find(e =>
            {
                if (e.channelNumber == null)
                {
                    return false;
                }
                return e.channelNumber[0] == channelNumber[0] && e.channelNumber[1] == channelNumber[1];
            });
            if (entry != null)
            {
                _logger.LogDebug($"Entry {entry.client.Address}:{entry.client.Port} - {entry.peer.Address}:{entry.peer.Port}");
            }
            return entry;
        }

        public void DeleteEntry(IPEndPoint client)
        {
            _logger.LogDebug("Delete Entry {address}:{port}", client.Address.ToString(), client.Port);
            var entry = _entries.Find(e => e.client.Equals(client));
            if (entry != null)
            {
                if (entry.sss != null && entry.sss._client != null)
                {
                    _logger.LogDebug("Close connection");
                    entry.sss._client.Close();
                }
                _entries.Remove(entry);
            }
            _logger.LogDebug("Entries Count {count}", GetEntriesCount());
        }
    }
}