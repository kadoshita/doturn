using System;
using System.Buffers.Binary;
using System.Net;

namespace Doturn.StunAttribute;

public class XorRelayedAddress : StunAttributeBase
{
    public IPEndPoint Endpoint { get; }
    public IPEndPoint RealEndpoint { get; }
    public override Type Type => Type.XorRelayedAddress;

    /// <summary>
    /// Create XorRelayedAddress from IP Address and Port
    /// </summary>
    public XorRelayedAddress(IPAddress address, ushort port)
    {
        RealEndpoint = new IPEndPoint(address, port);
        Endpoint = XorEndpoint(address, port);
    }
    /// <summary>
    /// Create XorRelayedAddress from IP Address and Port
    /// </summary>
    public XorRelayedAddress(string address, ushort port) : this(IPAddress.Parse(address), port)
    {
    }
    /// <summary>
    /// Create XorRelayedAddress from IP Endpoint
    /// </summary>
    public XorRelayedAddress(IPEndPoint endpoint) : this(endpoint.Address, (ushort)endpoint.Port)
    {
    }
    /// <summary>
    /// Create XorRelayedAddress from XOR-ed address / port byte arrays.
    /// </summary>
    public XorRelayedAddress(byte[] addressByteArray, byte[] portByteArray)
    {
        var address = new IPAddress(addressByteArray);
        ushort port = BinaryPrimitives.ReadUInt16BigEndian(portByteArray);
        Endpoint = new IPEndPoint(address, port);
        RealEndpoint = XorEndpoint(address, port);
    }
    private static IPEndPoint XorEndpoint(IPAddress address, ushort port)
    {
        byte[] addressByteArray = address.GetAddressBytes();
        byte[] xorAddress = ByteArrayUtils.XorAddress(addressByteArray);
        ushort xorPort = (ushort)(port ^ 0x2112);
        return new IPEndPoint(new IPAddress(xorAddress), xorPort);
    }

    public override byte[] ToBytes()
    {
        byte[] addressByteArray = Endpoint.Address.GetAddressBytes();
        int length = 1 + 1 + 2 + addressByteArray.Length;
        byte[] res = new byte[2 + 2 + length];
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(0, 2), (ushort)Type);
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(2, 2), (ushort)length);
        res[4] = 0x00; // reserved
        res[5] = 0x01; // address family (IPv4)
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(6, 2), (ushort)Endpoint.Port);
        addressByteArray.CopyTo(res, 8);
        return res;
    }

    public static XorRelayedAddress Parse(byte[] data)
    {
        byte[] portByteArray = data[2..4];
        byte[] addressByteArray = data[4..data.Length];
        return new XorRelayedAddress(addressByteArray, portByteArray);
    }
}
