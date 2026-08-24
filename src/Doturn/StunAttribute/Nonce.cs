using System;
using System.Buffers.Binary;
using System.Text;

namespace Doturn.StunAttribute;

public class Nonce : StunAttributeBase
{
    private const string NonceChars = "abcdefghijklmnopqrstuvwxyz0123456789";

    public string Value { get; }
    public override Type Type => Type.Nonce;
    public Nonce() : this(GenerateNonce(16))
    {
    }
    public Nonce(string nonce)
    {
        Value = nonce;
    }

    public override byte[] ToBytes()
    {
        byte[] nonceByteArray = Encoding.ASCII.GetBytes(Value);
        int length = nonceByteArray.Length;
        byte[] res = new byte[2 + 2 + length];
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(0, 2), (ushort)Type);
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(2, 2), (ushort)length);
        nonceByteArray.CopyTo(res, 4);
        return res;
    }
    private static string GenerateNonce(int length)
    {
        return string.Create(length, NonceChars, static (span, chars) =>
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = chars[Random.Shared.Next(chars.Length)];
            }
        });
    }

    public static Nonce Parse(byte[] data)
    {
        string nonceStr = Encoding.ASCII.GetString(data);
        return new Nonce(nonceStr);
    }
}
