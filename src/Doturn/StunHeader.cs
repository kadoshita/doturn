using System.Buffers.Binary;
using Doturn.StunMessage;

namespace Doturn;

public class StunHeader
{
    private static readonly byte[] DefaultMagicCookie = new byte[] { 0x21, 0x12, 0xA4, 0x42 };

    public StunMessage.Type Type { get; }
    public short MessageLength { get; }
    public byte[] MagicCookie { get; }
    public byte[] TransactionId { get; }

    public StunHeader(StunMessage.Type type, short messageLength, byte[] transactionId)
        : this(type, messageLength, DefaultMagicCookie, transactionId)
    {
    }
    public StunHeader(StunMessage.Type type, short messageLength, byte[] magicCookie, byte[] transactionId)
    {
        Type = type;
        MessageLength = messageLength;
        MagicCookie = magicCookie;
        TransactionId = transactionId;
    }
    public StunHeader(byte[] data)
    {
        Type = (StunMessage.Type)BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(0, 2));
        MessageLength = BinaryPrimitives.ReadInt16BigEndian(data.AsSpan(2, 2));
        MagicCookie = data[4..8];
        TransactionId = data[8..20];
    }

    public byte[] ToBytes()
    {
        byte[] res = new byte[20];
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(0, 2), (ushort)Type);
        BinaryPrimitives.WriteInt16BigEndian(res.AsSpan(2, 2), MessageLength);
        MagicCookie.CopyTo(res, 4);
        TransactionId.CopyTo(res, 8);
        return res;
    }
}
