using System.Buffers.Binary;

namespace Doturn.StunAttribute;

public class ChannelNumber : StunAttributeBase
{
    public byte[] Value { get; } = new byte[] { 0x00, 0x00 };
    private readonly byte[] _reserved = new byte[] { 0x00, 0x00 };
    public override Type Type => Type.ChannelNumber;

    public ChannelNumber()
    {
    }
    public ChannelNumber(byte[] channelNumber, byte[] reserved)
    {
        Value = channelNumber;
        _reserved = reserved;
    }
    public override byte[] ToBytes()
    {
        int length = Value.Length + _reserved.Length;
        byte[] res = new byte[2 + 2 + length];
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(0, 2), (ushort)Type);
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(2, 2), (ushort)length);
        Value.CopyTo(res, 4);
        _reserved.CopyTo(res, 4 + Value.Length);
        return res;
    }
    public static ChannelNumber Parse(byte[] data)
    {
        byte[] channelNumber = data[0..2];
        byte[] reserved = data[2..data.Length];
        return new ChannelNumber(channelNumber, reserved);
    }
}
