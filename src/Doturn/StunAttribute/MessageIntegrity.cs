using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Doturn.StunAttribute;

public class MessageIntegrity : StunAttributeBase
{
    private readonly byte[] _messageIntegrity;
    public override Type Type => Type.MessageIntegrity;

    public MessageIntegrity(byte[] messageIntegrity)
    {
        _messageIntegrity = messageIntegrity;
    }
    public MessageIntegrity(string username, string password, string realm, byte[] data)
    {
        string keyString = $"{username}:{realm}:{password}";
        byte[] key = MD5.HashData(Encoding.ASCII.GetBytes(keyString));
        _messageIntegrity = HMACSHA1.HashData(key, data);
    }
    public override byte[] ToBytes()
    {
        byte[] typeByteArray = Type.ToBytes();
        int length = _messageIntegrity.Length;
        byte[] lengthByteArray = new byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(lengthByteArray, (ushort)length);

        byte[] res = new byte[2 + 2 + length];
        ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, _messageIntegrity);
        return res;
    }

    public static MessageIntegrity Parse(byte[] data)
    {
        return new MessageIntegrity(data);
    }
}
