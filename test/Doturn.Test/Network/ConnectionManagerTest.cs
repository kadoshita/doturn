using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using System.Net.Sockets;
using System.Net;

namespace Doturn.Network.Test
{
    public class ConnectionManagerTest

    {
        private readonly StunServerService.IStunServerService sss;
        private readonly ILogger<ConnectionManager> logger;
        private readonly UdpClient client;

        public ConnectionManagerTest()
        {
            sss = Mock.Of<StunServerService.IStunServerService>();
            logger = Mock.Of<ILogger<ConnectionManager>>();
            client = new UdpClient();
        }

        [Fact]
        public void Set_Main_Client()
        {
            var connectionManager = new ConnectionManager(logger);
            connectionManager.SetMainClient(client);
            Assert.Equal(client, connectionManager.mainClient);
        }

        [Fact]
        public void Call_SendMainClientAsync()
        {
            var data = new byte[] { 0x00 };
            var endpoint = new IPEndPoint(IPAddress.Loopback, 20000);
            var connectionManager = new ConnectionManager(logger);
            connectionManager.SetMainClient(client);
            connectionManager.SendMainClientAsync(data, data.Length, endpoint);
        }

        [Fact]
        public void Add_And_Get_ConnectionEntry()
        {
            var entry = new ConnectionEntry(IPAddress.Loopback, 20000, sss);
            var connectionManager = new ConnectionManager(logger);
            connectionManager.AddConnectionEntry(entry);
            Assert.Equal(1, connectionManager.GetEntriesCount());
            Assert.Equal(entry, connectionManager.GetEntry(new IPEndPoint(IPAddress.Loopback, 20000)));
        }

        [Fact]
        public void Add_And_Get_By_Peer()
        {
            var client = new IPEndPoint(IPAddress.Loopback, 20000);
            var peer = new IPEndPoint(IPAddress.Any, 20001);
            var entry = new ConnectionEntry(IPAddress.Loopback, 20000, sss);
            var connectionManager = new ConnectionManager(logger);
            connectionManager.AddConnectionEntry(entry);
            connectionManager.AddPeerEndpoint(client, peer);
            Assert.Equal(1, connectionManager.GetEntriesCount());
            Assert.Equal(entry, connectionManager.GetEntry(client));
            Assert.Equal(entry, connectionManager.GetEntryByPeer(peer));
        }

        [Fact]
        public void Add_And_Get_Entry_By_Channel_Number()
        {
            var client = new IPEndPoint(IPAddress.Loopback, 20000);
            var peer = new IPEndPoint(IPAddress.Any, 20001);
            var channelNumber = new byte[] { 0x00, 0x01 };
            var entry = new ConnectionEntry(IPAddress.Loopback, 20000, sss);
            var connectionManager = new ConnectionManager(logger);
            connectionManager.AddConnectionEntry(entry);
            connectionManager.AddPeerEndpoint(client, peer);
            connectionManager.AddChannelNumber(client, channelNumber);
            Assert.Equal(1, connectionManager.GetEntriesCount());
            Assert.Equal(entry, connectionManager.GetEntry(client));
            Assert.Equal(entry, connectionManager.GetEntryByChannelNumber(channelNumber));
        }
    }
}