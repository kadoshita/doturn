using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Doturn.Network;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Doturn.StunServerService
{
    public class StunServerService : BackgroundService
    {
        private readonly ILogger<StunServerService> _logger;
        private readonly IOptions<AppSettings> _options;
        private readonly IConnectionManager _connectionManager;
        private readonly IPortAllocator _portAllocator;
        public readonly ushort listenPort;
        private readonly UdpClient _client;

        public StunServerService(ILogger<StunServerService> logger, IOptions<AppSettings> options, IConnectionManager connectionManager, IPortAllocator portAllocator)
        {
            _logger = logger;
            _options = options;
            _connectionManager = connectionManager;
            _portAllocator = portAllocator;
            listenPort = options.Value.ListeningPort;
            var endpoint = new IPEndPoint(IPAddress.Any, listenPort);
            _client = new UdpClient(endpoint);
            _connectionManager.SetMainClient(_client);
        }
        public StunServerService(ILogger<StunServerService> logger, IOptions<AppSettings> options, IConnectionManager connectionManager, IPortAllocator portAllocator, ushort listenPort)
        {
            _logger = logger;
            _options = options;
            _connectionManager = connectionManager;
            _portAllocator = portAllocator;
            this.listenPort = listenPort;
            var endpoint = new IPEndPoint(IPAddress.Any, listenPort);
            _client = new UdpClient(endpoint);
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug(listenPort, "Listening on {listenPort}", listenPort);
            while (true)
            {
                UdpReceiveResult data = await _client.ReceiveAsync();
                if (data.Buffer.Length < 20)
                {
                    _logger.LogDebug(listenPort, "Unknown data received");
                    continue;
                }
                try
                {
                    StunMessage.IStunMessage message = StunMessage.StunMessageParser.Parse(data.Buffer, _options.Value);
                    _logger.LogDebug(listenPort, "req: {messageType} {bytes}", message.Type, BitConverter.ToString(data.Buffer));
                    if (message.Type == StunMessage.Type.BINDING)
                    {
                        byte[] res = ((StunMessage.Binding)message).CreateSuccessResponse(data.RemoteEndPoint);
                        _logger.LogDebug(listenPort, "res: {messageType} {bytes}", message.Type, BitConverter.ToString(res));
                        await _client.SendAsync(res, res.Length, data.RemoteEndPoint);
                    }
                    else if (message.Type == StunMessage.Type.ALLOCATE)
                    {
                        IPAddress relayAddress = IPAddress.Parse(_options.Value.ExternalIPAddress);
                        ushort relayPort = _portAllocator.GetPort();
                        var sss = new StunServerService(_logger, _options, _connectionManager, _portAllocator, relayPort);
                        _ = sss.ExecuteAsync(new CancellationToken());
                        _connectionManager.AddConnectionEntry(new ConnectionEntry(data.RemoteEndPoint, sss));
                        byte[] res = ((StunMessage.Allocate)message).CreateSuccessResponse(data.RemoteEndPoint, relayAddress, relayPort);
                        _logger.LogDebug(listenPort, "res: {messageType} {bytes}", message.Type, BitConverter.ToString(res));
                        await _client.SendAsync(res, res.Length, data.RemoteEndPoint);
                    }
                    else if (message.Type == StunMessage.Type.BINDING)
                    {
                        byte[] res = ((StunMessage.Binding)message).CreateSuccessResponse(data.RemoteEndPoint);
                        _logger.LogDebug(listenPort, "res: {messageType} {bytes}", message.Type, BitConverter.ToString(res));
                        await _client.SendAsync(res, res.Length, data.RemoteEndPoint);
                    }
                    else if (message.Type == StunMessage.Type.CREATE_PERMISSION)
                    {
                        var createPermissionMessage = (StunMessage.CreatePermission)message;
                        var xorPeerAddress = (StunAttribute.XorPeerAddress)createPermissionMessage.attributes.Find(a => a.Type == StunAttribute.Type.XOR_PEER_ADDRESS);
                        _connectionManager.AddPeerEndpoint(data.RemoteEndPoint, xorPeerAddress.realEndpoint);
                        byte[] res = createPermissionMessage.CreateSuccessResponse();
                        _logger.LogDebug(listenPort, "res: {messageType} {bytes}", message.Type, BitConverter.ToString(res));
                        await _client.SendAsync(res, res.Length, data.RemoteEndPoint);
                    }
                    else if (message.Type == StunMessage.Type.REFRESH)
                    {
                        byte[] res = ((StunMessage.Refresh)message).CreateSuccessResponse();
                        _logger.LogDebug(listenPort, "res: {messageType} {bytes}", message.Type, BitConverter.ToString(res));
                        await _client.SendAsync(res, res.Length, data.RemoteEndPoint);
                    }
                    else if (message.Type == StunMessage.Type.CHANNEL_BIND)
                    {
                        var channelBindMessage = (StunMessage.ChannelBind)message;
                        var xorPeerAddress = (StunAttribute.XorPeerAddress)channelBindMessage.attributes.Find(a => a.Type == StunAttribute.Type.XOR_PEER_ADDRESS);
                        _connectionManager.AddPeerEndpoint(data.RemoteEndPoint, xorPeerAddress.realEndpoint);
                        var channelNumber = (StunAttribute.ChannelNumber)channelBindMessage.attributes.Find(a => a.Type == StunAttribute.Type.CHANNEL_NUMBER);
                        _connectionManager.AddChannelNumber(data.RemoteEndPoint, channelNumber.channelNumber);
                        byte[] res = channelBindMessage.CreateSuccessResponse();
                        _logger.LogDebug(listenPort, "res: {messageType} {bytes}", message.Type, BitConverter.ToString(res));
                        await _client.SendAsync(res, res.Length, data.RemoteEndPoint);
                    }
                    else if (message.Type == StunMessage.Type.SEND_INDICATION)
                    {
                        var sendIndication = (StunMessage.Send)message;
                        byte[] applicationData = sendIndication.ToApplicationDataBytes();
                        ConnectionEntry entry = _connectionManager.GetEntry(data.RemoteEndPoint);
                        _logger.LogDebug(listenPort, "send: {messageType} {bytes}", message.Type, BitConverter.ToString(applicationData));
                        await entry.sss._client.SendAsync(applicationData, applicationData.Length, entry.peer);
                    }
                    else if (message.Type == StunMessage.Type.DATA_INDICATION)
                    {
                        byte[] dataIndicationBytes = ((StunMessage.Data)message).CreateDataIndication(data.RemoteEndPoint);
                        ConnectionEntry entry = _connectionManager.GetEntryByPeer(data.RemoteEndPoint);
                        _logger.LogDebug(listenPort, "data: {messageType} {bytes}", message.Type, BitConverter.ToString(dataIndicationBytes));
                        await _client.SendAsync(dataIndicationBytes, dataIndicationBytes.Length, entry.client);
                    }
                }
                catch (StunMessage.StunMessageParseException)
                {
                    _logger.LogDebug($"Unknown data received: {BitConverter.ToString(data.Buffer)}");
                }
            }
        }
    }
}