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
            _logger.LogDebug($"Listening on {listenPort}");
            while (true)
            {
                UdpReceiveResult data = await _client.ReceiveAsync();
                if (data.Buffer.Length < 20)
                {
                    _logger.LogDebug("Unknown data received");
                    continue;
                }
                try
                {
                    StunMessage.IStunMessage message = StunMessage.StunMessageParser.Parse(data.Buffer, _options.Value);
                    _logger.LogDebug($"req: {message.Type} {BitConverter.ToString(data.Buffer)}");
                    if (message.Type == StunMessage.Type.BINDING)
                    {
                        byte[] res = ((StunMessage.Binding)message).CreateSuccessResponse(data.RemoteEndPoint);
                        _logger.LogDebug($"res: {message.Type} {BitConverter.ToString(res)}");
                        await _client.SendAsync(res, res.Length, data.RemoteEndPoint);
                    }
                    else if (message.Type == StunMessage.Type.ALLOCATE)
                    {
                        IPAddress relayAddress = IPAddress.Parse(_options.Value.ExternalIPAddress);
                        ushort relayPort = _portAllocator.GetPort();
                        _connectionManager.AddConnectionEntry(new ConnectionEntry(relayAddress, relayPort, data.RemoteEndPoint.Address, (ushort)data.RemoteEndPoint.Port));
                        var sss = new StunServerService(_logger, _options, _connectionManager, _portAllocator, relayPort);
                        _ = sss.ExecuteAsync(new CancellationToken());
                        byte[] res = ((StunMessage.Allocate)message).CreateSuccessResponse(data.RemoteEndPoint, relayAddress, relayPort);
                        _logger.LogDebug($"res: {message.Type} {BitConverter.ToString(res)}");
                        await _client.SendAsync(res, res.Length, data.RemoteEndPoint);
                    }
                    else if (message.Type == StunMessage.Type.BINDING)
                    {
                        byte[] res = ((StunMessage.Binding)message).CreateSuccessResponse(data.RemoteEndPoint);
                        _logger.LogDebug($"res: {message.Type} {BitConverter.ToString(res)}");
                        await _client.SendAsync(res, res.Length, data.RemoteEndPoint);
                    }
                    else if (message.Type == StunMessage.Type.CREATE_PERMISSION)
                    {
                        byte[] res = ((StunMessage.CreatePermission)message).CreateSuccessResponse();
                        _logger.LogDebug($"res: {message.Type} {BitConverter.ToString(res)}");
                        await _client.SendAsync(res, res.Length, data.RemoteEndPoint);
                    }
                    else if (message.Type == StunMessage.Type.REFRESH)
                    {
                        byte[] res = ((StunMessage.Refresh)message).CreateSuccessResponse();
                        _logger.LogDebug($"res: {message.Type} {BitConverter.ToString(res)}");
                        await _client.SendAsync(res, res.Length, data.RemoteEndPoint);
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