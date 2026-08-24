using System;
using System.Buffers.Binary;
using System.Text;

namespace Doturn.StunAttribute;

public class UsernameIsEmptyException : Exception
{
    public UsernameIsEmptyException() : base() { }
}
public class Username : StunAttributeBase
{
    public string Value { get; }
    public override Type Type => Type.Username;

    public Username(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            throw new UsernameIsEmptyException();
        }
        Value = username;
    }

    public override byte[] ToBytes()
    {
        byte[] usernameByteArray = Encoding.ASCII.GetBytes(Value);
        int length = usernameByteArray.Length;
        byte[] res = new byte[2 + 2 + length];
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(0, 2), (ushort)Type);
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(2, 2), (ushort)length);
        usernameByteArray.CopyTo(res, 4);
        return res;
    }
    public static Username Parse(byte[] data)
    {
        string usernameStr = Encoding.ASCII.GetString(data);
        return new Username(usernameStr);
    }
}
