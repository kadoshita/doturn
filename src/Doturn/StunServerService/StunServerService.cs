using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Doturn.Network;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Doturn.StunServerService;

public interface IStunServerService
{
    UdpClient Client { get; }
    ushort ListenPort { get; }
}
public class StunServerService : BackgroundService, IStunServerService
{
    private const ushort MinChannelNumber = 0x4000; // 16384
    private const ushort MaxChannelNumber = 0x7FFF; // 32767

    private readonly ILogger<StunServerService> _logger;
    private readonly IOptions<AppSettings> _options;
    private readonly IConnectionManager _connectionManager;
    private readonly IPortAllocator _portAllocator;
    private readonly UdpClient _client;

    public ushort ListenPort { get; }
    public UdpClient Client => _client;

    public StunServerService(ILogger<StunServerService> logger, IOptions<AppSettings> options, IConnectionManager connectionManager, IPortAllocator portAllocator)
        : this(logger, options, connectionManager, portAllocator, options.Value.ListeningPort, isMain: true)
    {
    }
    public StunServerService(ILogger<StunServerService> logger, IOptions<AppSettings> options, IConnectionManager connectionManager, IPortAllocator portAllocator, ushort listenPort)
        : this(logger, options, connectionManager, portAllocator, listenPort, isMain: false)
    {
    }
    private StunServerService(ILogger<StunServerService> logger, IOptions<AppSettings> options, IConnectionManager connectionManager, IPortAllocator portAllocator, ushort listenPort, bool isMain)
    {
        _logger = logger;
        _options = options;
        _connectionManager = connectionManager;
        _portAllocator = portAllocator;
        ListenPort = listenPort;
        _client = new UdpClient(new IPEndPoint(IPAddress.Any, listenPort));
        if (isMain)
        {
            _connectionManager.SetMainClient(_client);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("Listening on {ListenPort}", ListenPort);
        while (!stoppingToken.IsCancellationRequested)
        {
            UdpReceiveResult data;
            try
            {
                data = await _client.ReceiveAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            if (data.Buffer.Length < 20)
            {
                _logger.LogDebug("Unknown data received");
                continue;
            }

            try
            {
                await HandleDatagramAsync(data, stoppingToken);
            }
            catch (StunMessage.StunMessageParseException)
            {
                _logger.LogDebug("Unknown data received");
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Unhandled exception in receive loop");
            }
        }
    }

    private async Task HandleDatagramAsync(UdpReceiveResult data, CancellationToken stoppingToken)
    {
        ushort channelNumber = BinaryPrimitives.ReadUInt16BigEndian(data.Buffer.AsSpan(0, 2));
        byte[] messageLength = data.Buffer[2..4];
        byte[] rawData = data.Buffer[4..data.Buffer.Length];

        // ChannelData: RFC 5766 §11
        if (channelNumber is >= MinChannelNumber and <= MaxChannelNumber)
        {
            _logger.LogInformation("{ChannelNumber} is ChannelNumber", channelNumber);
            var entry = _connectionManager.GetEntryByChannelNumber(data.Buffer[0..2]);
            if (entry != null && entry.Peer != null && entry.RelayService?.Client != null)
            {
                LogChannelDataReceived(channelNumber, messageLength, data.RemoteEndPoint, entry.Peer);
                await entry.RelayService.Client.SendAsync(rawData, rawData.Length, entry.Peer);
                return;
            }
        }

        _logger.LogDebug("data received");
        ConnectionEntry? peerEntry = _connectionManager.GetEntryByPeer(data.RemoteEndPoint);
        if (peerEntry != null)
        {
            LogApplicationDataReceived(messageLength, data.RemoteEndPoint, peerEntry.Client);
            if (peerEntry.ChannelNumber == null)
            {
                StunMessage.Data dataIndication = new(data.Buffer);
                byte[] dataIndicationBytes = dataIndication.CreateDataIndication(data.RemoteEndPoint);
                LogOutgoing(dataIndication.Type, data.Buffer);
                await _connectionManager.SendMainClientAsync(dataIndicationBytes, dataIndicationBytes.Length, peerEntry.Client);
            }
            else
            {
                byte[] channelData = new byte[4 + data.Buffer.Length];
                peerEntry.ChannelNumber.CopyTo(channelData, 0);
                BinaryPrimitives.WriteUInt16BigEndian(channelData.AsSpan(2, 2), (ushort)data.Buffer.Length);
                data.Buffer.CopyTo(channelData, 4);
                await _connectionManager.SendMainClientAsync(channelData, channelData.Length, peerEntry.Client);
            }
            return;
        }

        StunMessage.IStunMessage message = StunMessage.StunMessageParser.Parse(data.Buffer, _options.Value);
        LogIncomingStunMessage(message.Type, data.Buffer);

        switch (message.Type)
        {
            case StunMessage.Type.Binding:
                await SendResponseAsync(((StunMessage.Binding)message).CreateSuccessResponse(data.RemoteEndPoint), message.Type, data.RemoteEndPoint);
                break;

            case StunMessage.Type.Allocate:
                await HandleAllocateAsync((StunMessage.Allocate)message, data.RemoteEndPoint, stoppingToken);
                break;

            case StunMessage.Type.CreatePermission:
                {
                    var createPermissionMessage = (StunMessage.CreatePermission)message;
                    var xorPeerAddress = (StunAttribute.XorPeerAddress)createPermissionMessage.Attributes.Find(a => a.Type == StunAttribute.Type.XorPeerAddress)!;
                    _connectionManager.AddPeerEndpoint(data.RemoteEndPoint, xorPeerAddress.RealEndpoint);
                    await SendResponseAsync(createPermissionMessage.CreateSuccessResponse(), message.Type, data.RemoteEndPoint);
                }
                break;

            case StunMessage.Type.Refresh:
                {
                    var refreshRequest = (StunMessage.Refresh)message;
                    var lifetime = (StunAttribute.Lifetime)refreshRequest.Attributes.Find(a => a.Type == StunAttribute.Type.Lifetime)!;
                    if (lifetime.Value == 0)
                    {
                        _connectionManager.DeleteEntry(data.RemoteEndPoint);
                        _logger.LogInformation("delete entry {Address}:{Port}", data.RemoteEndPoint.Address, data.RemoteEndPoint.Port);
                        await SendResponseAsync(refreshRequest.CreateSuccessResponse(0), message.Type, data.RemoteEndPoint);
                    }
                    else
                    {
                        await SendResponseAsync(refreshRequest.CreateSuccessResponse(), message.Type, data.RemoteEndPoint);
                    }
                }
                break;

            case StunMessage.Type.ChannelBind:
                {
                    var channelBindMessage = (StunMessage.ChannelBind)message;
                    var xorPeerAddress = (StunAttribute.XorPeerAddress)channelBindMessage.Attributes.Find(a => a.Type == StunAttribute.Type.XorPeerAddress)!;
                    _connectionManager.AddPeerEndpoint(data.RemoteEndPoint, xorPeerAddress.RealEndpoint);
                    var boundChannelNumber = (StunAttribute.ChannelNumber)channelBindMessage.Attributes.Find(a => a.Type == StunAttribute.Type.ChannelNumber)!;
                    _connectionManager.AddChannelNumber(data.RemoteEndPoint, boundChannelNumber.Value);
                    await SendResponseAsync(channelBindMessage.CreateSuccessResponse(), message.Type, data.RemoteEndPoint);
                }
                break;

            case StunMessage.Type.SendIndication:
                {
                    var sendIndication = (StunMessage.Send)message;
                    byte[] applicationData = sendIndication.ToApplicationDataBytes();
                    ConnectionEntry entry = _connectionManager.GetEntry(data.RemoteEndPoint)!;
                    _logger.LogDebug("send: {MessageType} {Bytes}", message.Type, BitConverter.ToString(applicationData));
                    await entry.RelayService.Client.SendAsync(applicationData, applicationData.Length, entry.Peer!);
                }
                break;

            case StunMessage.Type.DataIndication:
                {
                    byte[] dataIndicationBytes = ((StunMessage.Data)message).CreateDataIndication(data.RemoteEndPoint);
                    ConnectionEntry entry = _connectionManager.GetEntryByPeer(data.RemoteEndPoint)!;
                    _logger.LogDebug("data: {MessageType} {Bytes}", message.Type, BitConverter.ToString(dataIndicationBytes));
                    await _client.SendAsync(dataIndicationBytes, dataIndicationBytes.Length, entry.Client);
                }
                break;

            default:
                _logger.LogDebug("Unhandled STUN message type {MessageType}", message.Type);
                break;
        }
    }

    private async Task HandleAllocateAsync(StunMessage.Allocate allocateMessage, IPEndPoint remoteEndpoint, CancellationToken stoppingToken)
    {
        IPAddress relayAddress = IPAddress.Parse(_options.Value.ExternalIPAddress);
        ushort relayPort = _portAllocator.GetPort();
        var relayService = new StunServerService(_logger, _options, _connectionManager, _portAllocator, relayPort);
        _ = Task.Run(() => relayService.ExecuteAsync(stoppingToken), stoppingToken);
        _connectionManager.AddConnectionEntry(new ConnectionEntry(remoteEndpoint, relayService));
        byte[] res = allocateMessage.CreateSuccessResponse(remoteEndpoint, relayAddress, relayPort);
        await SendResponseAsync(res, StunMessage.Type.Allocate, remoteEndpoint);
    }

    private async Task SendResponseAsync(byte[] res, StunMessage.Type messageType, IPEndPoint endpoint)
    {
        _logger.LogDebug("res: {MessageType} {Bytes}", messageType, BitConverter.ToString(res));
        await _client.SendAsync(res, res.Length, endpoint);
    }

    private void LogChannelDataReceived(ushort channelNumber, byte[] messageLength, IPEndPoint remote, IPEndPoint peer)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }
        _logger.LogDebug(
            "ChannelData received: {ChannelNumber} {MessageLength} {RemoteAddress}:{RemotePort} {ClientAddress}:{ClientPort}",
            channelNumber, BitConverter.ToString(messageLength), remote.Address, remote.Port, peer.Address, peer.Port);
    }

    private void LogApplicationDataReceived(byte[] messageLength, IPEndPoint remote, IPEndPoint client)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }
        _logger.LogDebug(
            "Application data received: {MessageLength} {RemoteAddress}:{RemotePort} {ClientAddress}:{ClientPort}",
            BitConverter.ToString(messageLength), remote.Address, remote.Port, client.Address, client.Port);
    }

    private void LogOutgoing(StunMessage.Type type, byte[] bytes)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }
        _logger.LogDebug("send: {MessageType} {Bytes}", type, BitConverter.ToString(bytes));
    }

    private void LogIncomingStunMessage(StunMessage.Type type, byte[] bytes)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }
        _logger.LogDebug("req: {MessageType} {Bytes}", type, BitConverter.ToString(bytes));
    }

    public override void Dispose()
    {
        _client.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
