using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
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

public class ConnectionManager(ILogger<ConnectionManager> logger, IPortAllocator portAllocator) : IConnectionManager
{
    private readonly ConcurrentDictionary<IPEndPoint, ConnectionEntry> _entriesByClient = new();
    private readonly ConcurrentDictionary<IPEndPoint, ConnectionEntry> _entriesByPeer = new();
    private readonly ConcurrentDictionary<ushort, ConnectionEntry> _entriesByChannel = new();
    private readonly ILogger<ConnectionManager> _logger = logger;
    private readonly IPortAllocator _portAllocator = portAllocator;
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
        _entriesByClient[entry.Client] = entry;
        _logger.LogDebug("Entries: {Count}", _entriesByClient.Count);
    }

    public void AddPeerEndpoint(IPEndPoint client, IPEndPoint peer)
    {
        _logger.LogDebug("Add peer {ClientAddress}:{ClientPort} - {PeerAddress}:{PeerPort}", client.Address, client.Port, peer.Address, peer.Port);
        if (_entriesByClient.TryGetValue(client, out var entry))
        {
            entry.Peer = peer;
            _entriesByPeer[peer] = entry;
        }
    }

    public void AddChannelNumber(IPEndPoint client, byte[] channelNumber)
    {
        _logger.LogDebug("Add channel number {Address}:{Port} - {ChannelNumber}", client.Address, client.Port, BitConverter.ToString(channelNumber));
        if (_entriesByClient.TryGetValue(client, out var entry))
        {
            entry.ChannelNumber = channelNumber;
            ushort key = BinaryPrimitives.ReadUInt16BigEndian(channelNumber);
            _entriesByChannel[key] = entry;
        }
    }

    public int GetEntriesCount() => _entriesByClient.Count;

    public ConnectionEntry? GetEntry(IPEndPoint endpoint)
    {
        _logger.LogDebug("Get entry {Address} {Port}", endpoint.Address, endpoint.Port);
        return _entriesByClient.TryGetValue(endpoint, out var entry) ? entry : null;
    }

    public ConnectionEntry? GetEntryByPeer(IPEndPoint endpoint)
    {
        _logger.LogDebug("Get entry by Peer {Address} {Port}", endpoint.Address, endpoint.Port);
        return _entriesByPeer.TryGetValue(endpoint, out var entry) ? entry : null;
    }

    public ConnectionEntry? GetEntryByChannelNumber(byte[] channelNumber)
    {
        _logger.LogDebug("Get entry by Channel Number {ChannelNumber}", BitConverter.ToString(channelNumber));
        ushort key = BinaryPrimitives.ReadUInt16BigEndian(channelNumber);
        return _entriesByChannel.TryGetValue(key, out var entry) ? entry : null;
    }

    public void DeleteEntry(IPEndPoint client)
    {
        _logger.LogDebug("Delete Entry {Address}:{Port}", client.Address, client.Port);
        if (_entriesByClient.TryRemove(client, out var entry))
        {
            if (entry.Peer != null)
            {
                _entriesByPeer.TryRemove(entry.Peer, out _);
            }
            if (entry.ChannelNumber != null)
            {
                ushort key = BinaryPrimitives.ReadUInt16BigEndian(entry.ChannelNumber);
                _entriesByChannel.TryRemove(key, out _);
            }
            if (entry.RelayService is { } relayService)
            {
                if (relayService.Client is { } relayClient)
                {
                    _logger.LogDebug("Close connection");
                    relayClient.Close();
                }
                _portAllocator.ReleasePort(relayService.ListenPort);
            }
        }
        _logger.LogDebug("Entries Count {Count}", GetEntriesCount());
    }
}
