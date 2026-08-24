using System;
using System.Buffers.Binary;
using System.Text;

namespace Doturn.StunAttribute;

public class Software : StunAttributeBase
{
    public string Value { get; }
    public override Type Type => Type.Software;
    public Software() : this("Doturn")
    {
    }
    public Software(string software)
    {
        Value = software;
    }

    public override byte[] ToBytes()
    {
        byte[] softwareByteArray = Encoding.ASCII.GetBytes(Value);
        int length = softwareByteArray.Length;
        int paddingLength = 8 - (length % 8);
        if (paddingLength >= 8)
        {
            paddingLength = 0;
        }
        byte[] res = new byte[2 + 2 + length + paddingLength];
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(0, 2), (ushort)Type);
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(2, 2), (ushort)length);
        softwareByteArray.CopyTo(res, 4);
        return res;
    }
    public static Software Parse(byte[] data)
    {
        string softwareStr = Encoding.ASCII.GetString(data);
        return new Software(softwareStr);
    }
}
