using System;
using System.Security.Cryptography;

namespace Doturn.StunAttribute
{
    public class MessageIntegrity : StunAttributeBase
    {
        public readonly Type type = Type.MESSAGE_INTEGRITY;
        private readonly byte[] _messageIntegrity;
        public override Type Type => type;

        public MessageIntegrity(byte[] messageIntegrity)
        {
            _messageIntegrity = messageIntegrity;
        }
        public MessageIntegrity(string username, string password, string realm, byte[] data)
        {
            var md5 = MD5.Create();
            string keyString = $"{username}:{realm}:{password}";
            byte[] keyStringByteArray = System.Text.Encoding.ASCII.GetBytes(keyString);
            byte[] md5HashByteArray = md5.ComputeHash(keyStringByteArray);
            var hmacSHA1 = new HMACSHA1(md5HashByteArray);
            md5.Clear();
            byte[] hmacSHA1ByteArray = hmacSHA1.ComputeHash(data);
            _messageIntegrity = hmacSHA1ByteArray;
            hmacSHA1.Clear();
        }
        public override byte[] ToBytes()
        {
            byte[] typeByteArray = type.ToBytes();
            int length = _messageIntegrity.Length;
            byte[] lengthByteArray = BitConverter.GetBytes((short)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }

            byte[] res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, _messageIntegrity);
            return res;
        }

        public static MessageIntegrity Parse(byte[] data)
        {
            return new MessageIntegrity(data);
        }
    }
}