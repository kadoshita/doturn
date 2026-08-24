using System;
using System.Buffers.Binary;
using System.Text;

namespace Doturn.StunAttribute;

public class Realm : StunAttributeBase
{
    public string Value { get; }
    public override Type Type => Type.Realm;
    public Realm() : this("example.com")
    {
    }
    public Realm(string realm)
    {
        Value = realm;
    }

    public override byte[] ToBytes()
    {
        byte[] realmByteArray = Encoding.ASCII.GetBytes(Value);
        int length = realmByteArray.Length;
        int paddingLength = 8 - ((2 + 2 + length) % 8);
        byte[] res = new byte[2 + 2 + length + paddingLength];
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(0, 2), (ushort)Type);
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(2, 2), (ushort)length);
        realmByteArray.CopyTo(res, 4);
        return res;
    }

    public static Realm Parse(byte[] data)
    {
        string realmStr = Encoding.ASCII.GetString(data);
        return new Realm(realmStr);
    }
}
