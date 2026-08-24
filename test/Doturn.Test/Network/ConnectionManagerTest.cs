using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Doturn.Network.Test;

public class ConnectionManagerTest : IDisposable
{
    private readonly StunServerService.IStunServerService sss;
    private readonly ILogger<ConnectionManager> logger;
    private readonly IPortAllocator portAllocator;
    private readonly UdpClient client;

    public ConnectionManagerTest()
    {
        sss = Mock.Of<StunServerService.IStunServerService>();
        logger = Mock.Of<ILogger<ConnectionManager>>();
        portAllocator = Mock.Of<IPortAllocator>();
        client = new UdpClient();
    }

    public void Dispose()
    {
        client.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Set_Main_Client()
    {
        var connectionManager = new ConnectionManager(logger, portAllocator);
        connectionManager.SetMainClient(client);
        Assert.Equal(client, connectionManager.MainClient);
    }

    [Fact]
    public void Call_SendMainClientAsync()
    {
        var data = new byte[] { 0x00 };
        var endpoint = new IPEndPoint(IPAddress.Loopback, 20000);
        var connectionManager = new ConnectionManager(logger, portAllocator);
        connectionManager.SetMainClient(client);
        connectionManager.SendMainClientAsync(data, data.Length, endpoint);
    }

    [Fact]
    public void Add_And_Get_ConnectionEntry()
    {
        var entry = new ConnectionEntry(IPAddress.Loopback, 20000, sss);
        var connectionManager = new ConnectionManager(logger, portAllocator);
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
        var connectionManager = new ConnectionManager(logger, portAllocator);
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
        var connectionManager = new ConnectionManager(logger, portAllocator);
        connectionManager.AddConnectionEntry(entry);
        connectionManager.AddPeerEndpoint(client, peer);
        connectionManager.AddChannelNumber(client, channelNumber);
        Assert.Equal(1, connectionManager.GetEntriesCount());
        Assert.Equal(entry, connectionManager.GetEntry(client));
        Assert.Equal(entry, connectionManager.GetEntryByChannelNumber(channelNumber));
    }

    [Fact]
    public void Delete_Entry()
    {
        var client1 = new IPEndPoint(IPAddress.Loopback, 20000);
        var client2 = new IPEndPoint(IPAddress.Loopback, 20001);
        var entry1 = new ConnectionEntry(client1.Address, (ushort)client1.Port, sss);
        var entry2 = new ConnectionEntry(client2.Address, (ushort)client2.Port, sss);
        var connectionManager = new ConnectionManager(logger, portAllocator);
        connectionManager.AddConnectionEntry(entry1);
        connectionManager.AddConnectionEntry(entry2);
        Assert.Equal(2, connectionManager.GetEntriesCount());
        connectionManager.DeleteEntry(client1);
        Assert.Equal(1, connectionManager.GetEntriesCount());
    }
}
