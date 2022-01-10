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
        public readonly UInt16 listenPort;
        private UdpClient client;

        public StunServerService(ILogger<StunServerService> logger, IOptions<AppSettings> options)
        {
            _logger = logger;
            _options = options;
            listenPort = options.Value.ListeningPort;
            var endpoint = new IPEndPoint(IPAddress.Any, listenPort);
            client = new UdpClient(endpoint);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                _logger.LogDebug("Wait...");
                var data = await client.ReceiveAsync();
                try
                {
                    var message = StunMessage.StunMessageParser.Parse(data.Buffer);
                    _logger.LogDebug($"req: {message.Type} {BitConverter.ToString(data.Buffer)}");
                    if (message.Type == StunMessage.Type.BINDING)
                    {
                        var res = ((StunMessage.Binding)message).CreateSuccessResponse(data.RemoteEndPoint);
                        _logger.LogDebug($"res: {message.Type} {BitConverter.ToString(res)}");
                        await client.SendAsync(res, res.Length, data.RemoteEndPoint);
                    }
                    else if (message.Type == StunMessage.Type.ALLOCATE)
                    {
                        var res = ((StunMessage.Allocate)message).CreateSuccessResponse(data.RemoteEndPoint);
                        _logger.LogDebug($"res: {message.Type} {BitConverter.ToString(res)}");
                        await client.SendAsync(res, res.Length, data.RemoteEndPoint);
                    }
                    else if (message.Type == StunMessage.Type.BINDING)
                    {
                        var res = ((StunMessage.Binding)message).CreateSuccessResponse(data.RemoteEndPoint);
                        _logger.LogDebug($"res: {message.Type} {BitConverter.ToString(res)}");
                        await client.SendAsync(res, res.Length, data.RemoteEndPoint);
                    }
                    else if (message.Type == StunMessage.Type.CREATE_PERMISSION)
                    {
                        var res = ((StunMessage.CreatePermission)message).CreateSuccessResponse();
                        _logger.LogDebug($"res: {message.Type} {BitConverter.ToString(res)}");
                        await client.SendAsync(res, res.Length, data.RemoteEndPoint);
                    }
                    else if (message.Type == StunMessage.Type.REFRESH)
                    {
                        var res = ((StunMessage.Refresh)message).CreateSuccessResponse();
                        _logger.LogDebug($"res: {message.Type} {BitConverter.ToString(res)}");
                        await client.SendAsync(res, res.Length, data.RemoteEndPoint);
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