using System;
using System.Buffers.Binary;

namespace Doturn.StunAttribute;

public class Data : StunAttributeBase
{
    public byte[] Value { get; }
    public override Type Type => Type.Data;

    public Data() : this(Array.Empty<byte>())
    {
    }
    public Data(byte[] data)
    {
        Value = data;
    }
    public override byte[] ToBytes()
    {
        int length = Value.Length;
        int paddingLength = 8 - (length % 8);
        if (paddingLength >= 8)
        {
            paddingLength = 0;
        }
        byte[] res = new byte[2 + 2 + length + paddingLength];
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(0, 2), (ushort)Type);
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(2, 2), (ushort)length);
        Value.CopyTo(res, 4);
        return res;
    }

    public static Data Parse(byte[] data)
    {
        return new Data(data);
    }
}
