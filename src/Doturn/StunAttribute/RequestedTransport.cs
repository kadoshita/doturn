using System.Buffers.Binary;

namespace Doturn.StunAttribute;

public class RequestedTransport : StunAttributeBase
{
    public Transport Value { get; }
    private readonly byte[] _reserved;
    public override Type Type => Type.RequestedTransport;

    public RequestedTransport() : this(Transport.Udp, new byte[] { 0x00, 0x00, 0x00 })
    {
    }
    public RequestedTransport(Transport transport, byte[] reserved)
    {
        Value = transport;
        _reserved = reserved;
    }
    public override byte[] ToBytes()
    {
        int length = 1 + _reserved.Length;
        byte[] res = new byte[2 + 2 + length];
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(0, 2), (ushort)Type);
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(2, 2), (ushort)length);
        res[4] = (byte)Value;
        _reserved.CopyTo(res, 5);
        return res;
    }
    public static RequestedTransport Parse(byte[] data)
    {
        byte[] reserved = data[1..data.Length];
        var transport = (Transport)data[0];
        return new RequestedTransport(transport, reserved);
    }
}
