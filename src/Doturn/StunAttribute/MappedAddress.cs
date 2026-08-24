using System;
using System.Buffers.Binary;
using System.Net;

namespace Doturn.StunAttribute;

public class MappedAddress : StunAttributeBase
{
    public IPEndPoint Endpoint { get; }
    public override Type Type => Type.MappedAddress;

    public MappedAddress(IPAddress address, int port) : this(new IPEndPoint(address, port))
    {
    }
    public MappedAddress(string address, int port) : this(new IPEndPoint(IPAddress.Parse(address), port))
    {
    }
    public MappedAddress(IPEndPoint endpoint)
    {
        Endpoint = endpoint;
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

    public static IStunAttribute Parse(byte[] data)
    {
        int port = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(2, 2));
        byte[] addressByteArray = data[4..data.Length];
        var address = new IPAddress(addressByteArray);
        return new MappedAddress(address, port);
    }
}
