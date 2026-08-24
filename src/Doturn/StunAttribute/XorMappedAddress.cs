using System;
using System.Buffers.Binary;
using System.Net;

namespace Doturn.StunAttribute;

public class XorMappedAddress : StunAttributeBase
{
    public IPEndPoint Endpoint { get; }
    public IPEndPoint RealEndpoint { get; }
    public override Type Type => Type.XorMappedAddress;
    /// <summary>
    /// Create XorMappedAddress from IP Address and Port
    /// </summary>
    /// <param name="address">Real address</param>
    /// <param name="port">Real port</param>
    public XorMappedAddress(IPAddress address, ushort port)
    {
        RealEndpoint = new IPEndPoint(address, port);
        Endpoint = XorEndpoint(address, port);
    }
    /// <summary>
    /// Create XorMappedAddress from IP Address and Port
    /// </summary>
    /// <param name="address">Real address</param>
    /// <param name="port">Real address</param>
    public XorMappedAddress(string address, ushort port) : this(IPAddress.Parse(address), port)
    {
    }
    /// <summary>
    /// Create XorMappedAddress from IP Address and Port
    /// </summary>
    /// <param name="endpoint">Real IP endpoint</param>
    public XorMappedAddress(IPEndPoint endpoint) : this(endpoint.Address, (ushort)endpoint.Port)
    {
    }
    /// <summary>
    /// Create XorMappedAddress from XOR-ed address / port byte arrays.
    /// </summary>
    public XorMappedAddress(byte[] addressByteArray, byte[] portByteArray)
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
    public static XorMappedAddress Parse(byte[] data)
    {
        byte[] portByteArray = data[2..4];
        byte[] addressByteArray = data[4..data.Length];
        return new XorMappedAddress(addressByteArray, portByteArray);
    }
}
