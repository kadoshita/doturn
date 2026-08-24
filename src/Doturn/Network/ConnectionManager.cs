using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Doturn.Network;

public class ConnectionEntry
{
    public IPEndPoint Client { get; set; }
    public IPEndPoint? Peer { get; set; }
    public byte[]? ChannelNumber { get; set; }
    public StunServerService.IStunServerService RelayService { get; set; }

    public ConnectionEntry(IPAddress clientAddress, ushort clientPort, StunServerService.IStunServerService relayService)
    {
        Client = new IPEndPoint(clientAddress, clientPort);
        RelayService = relayService;
    }
    public ConnectionEntry(IPEndPoint client, StunServerService.IStunServerService relayService)
    {
        Client = client;
        RelayService = relayService;
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
    ConnectionEntry? GetEntry(IPEndPoint endpoint);
    ConnectionEntry? GetEntryByPeer(IPEndPoint endpoint);
    ConnectionEntry? GetEntryByChannelNumber(byte[] channelNumber);
    int GetEntriesCount();

}
public class ConnectionManager(ILogger<ConnectionManager> logger) : IConnectionManager
{
    // TODO Dictionaryを使う
    private readonly List<ConnectionEntry> _entries = new();
    private readonly ILogger<ConnectionManager> _logger = logger;
    private UdpClient? _mainClient;

    public UdpClient? MainClient => _mainClient;

    public void SetMainClient(UdpClient client)
    {
        _mainClient ??= client;
    }

    public Task<int> SendMainClientAsync(byte[] data, int length, IPEndPoint endpoint)
    {
        if (_mainClient == null)
        {
            throw new InvalidOperationException("MainClient has not been set");
        }
        return _mainClient.SendAsync(data, length, endpoint);
    }
    public void AddConnectionEntry(ConnectionEntry entry)
    {
        _logger.LogDebug("Add entry {Address}:{Port}", entry.Client.Address, entry.Client.Port);
        _entries.Add(entry);
        _logger.LogDebug("Entries: {Count}", _entries.Count);
    }

    public void AddPeerEndpoint(IPEndPoint client, IPEndPoint peer)
    {
        _logger.LogDebug("Add peer {ClientAddress}:{ClientPort} - {PeerAddress}:{PeerPort}", client.Address, client.Port, peer.Address, peer.Port);
        ConnectionEntry? entry = _entries.Find(e => e.Client.Equals(client));
        if (entry != null)
        {
            entry.Peer = peer;
        }
    }

    public void AddChannelNumber(IPEndPoint client, byte[] channelNumber)
    {
        _logger.LogDebug("Add channel number {Address}:{Port} - {ChannelNumber}", client.Address, client.Port, BitConverter.ToString(channelNumber));
        ConnectionEntry? entry = _entries.Find(e => e.Client.Equals(client));
        if (entry != null)
        {
            entry.ChannelNumber = channelNumber;
        }
    }

    public int GetEntriesCount()
    {
        return _entries.Count;
    }

    public ConnectionEntry? GetEntry(IPEndPoint endpoint)
    {
        _logger.LogDebug("Get entry {Address} {Port}", endpoint.Address, endpoint.Port);
        ConnectionEntry? entry = _entries.Find(e => e.Client.Equals(endpoint));
        _logger.LogDebug("Entry: {Entry}", entry);
        return entry;
    }

    public ConnectionEntry? GetEntryByPeer(IPEndPoint endpoint)
    {
        _logger.LogDebug("Get entry by Peer {Address} {Port}", endpoint.Address, endpoint.Port);
        ConnectionEntry? entry = _entries.Find(e =>
        {
            if (e.Peer == null)
            {
                return false;
            }
            return e.Peer.Equals(endpoint);
        });
        if (entry != null && entry.Peer != null)
        {
            _logger.LogDebug("Entry {ClientAddress}:{ClientPort} - {PeerAddress}:{PeerPort}", entry.Client.Address, entry.Client.Port, entry.Peer.Address, entry.Peer.Port);
        }
        return entry;
    }

    public ConnectionEntry? GetEntryByChannelNumber(byte[] channelNumber)
    {
        _logger.LogDebug("Get entry by Channel Number {ChannelNumber}", BitConverter.ToString(channelNumber));
        ConnectionEntry? entry = _entries.Find(e =>
        {
            if (e.ChannelNumber == null)
            {
                return false;
            }
            return e.ChannelNumber[0] == channelNumber[0] && e.ChannelNumber[1] == channelNumber[1];
        });
        if (entry != null && entry.Peer != null)
        {
            _logger.LogDebug("Entry {ClientAddress}:{ClientPort} - {PeerAddress}:{PeerPort}", entry.Client.Address, entry.Client.Port, entry.Peer.Address, entry.Peer.Port);
        }
        return entry;
    }

    public void DeleteEntry(IPEndPoint client)
    {
        _logger.LogDebug("Delete Entry {Address}:{Port}", client.Address, client.Port);
        var entry = _entries.Find(e => e.Client.Equals(client));
        if (entry != null)
        {
            if (entry.RelayService != null && entry.RelayService.Client != null)
            {
                _logger.LogDebug("Close connection");
                entry.RelayService.Client.Close();
            }
            _entries.Remove(entry);
        }
        _logger.LogDebug("Entries Count {Count}", GetEntriesCount());
    }
}
