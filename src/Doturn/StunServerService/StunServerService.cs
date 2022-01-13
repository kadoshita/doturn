using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Doturn.StunServerService
{
    public class StunServerService : BackgroundService
    {
        private readonly ILogger<StunServerService> _logger;
        private readonly IOptions<AppSettings> _options;
        public readonly ushort listenPort;
        private readonly UdpClient _client;

        public StunServerService(ILogger<StunServerService> logger, IOptions<AppSettings> options)
        {
            _logger = logger;
            _options = options;
            listenPort = options.Value.ListeningPort;
            var endpoint = new IPEndPoint(IPAddress.Any, listenPort);
            _client = new UdpClient(endpoint);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                _logger.LogDebug("Wait...");
                UdpReceiveResult data = await _client.ReceiveAsync();
                try
                {
                    StunMessage.IStunMessage message = StunMessage.StunMessageParser.Parse(data.Buffer);
                    _logger.LogDebug($"req: {message.Type} {BitConverter.ToString(data.Buffer)}");
                    if (message.Type == StunMessage.Type.BINDING)
                    {
                        byte[] res = ((StunMessage.Binding)message).CreateSuccessResponse(data.RemoteEndPoint);
                        _logger.LogDebug($"res: {message.Type} {BitConverter.ToString(res)}");
                        await _client.SendAsync(res, res.Length, data.RemoteEndPoint);
                    }
                    else if (message.Type == StunMessage.Type.ALLOCATE)
                    {
                        byte[] res = ((StunMessage.Allocate)message).CreateSuccessResponse(data.RemoteEndPoint);
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
                    _logger.LogDebug("Unknown data received");
                }
            }
        }
    }
}