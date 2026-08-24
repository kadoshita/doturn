using System;
using System.Buffers.Binary;

namespace Doturn.StunAttribute;

public class Lifetime : StunAttributeBase
{
    public int Value { get; }
    public override Type Type => Type.Lifetime;

    public Lifetime() : this(600)
    {
    }
    public Lifetime(int lifetime)
    {
        Value = lifetime;
    }
    public override byte[] ToBytes()
    {
        byte[] res = new byte[2 + 2 + 4];
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(0, 2), (ushort)Type);
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(2, 2), 4);
        BinaryPrimitives.WriteInt32BigEndian(res.AsSpan(4, 4), Value);
        return res;
    }

    public static Lifetime Parse(byte[] data)
    {
        int lifetime = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(0, 4));
        return new Lifetime(lifetime);
    }
}
