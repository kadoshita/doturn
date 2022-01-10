using System;
using System.Security.Cryptography;

namespace Doturn.StunAttribute
{
    public class MessageIntegrity : StunAttributeBase
    {
        public readonly Type type = Type.MESSAGE_INTEGRITY;
        private readonly byte[] messageIntegrity;
        public override Type Type => this.type;

        public MessageIntegrity(byte[] messageIntegrity)
        {
            this.messageIntegrity = messageIntegrity;
        }
        public MessageIntegrity(string username, string password, string realm, byte[] data)
        {
            var md5 = MD5.Create();
            var keyString = $"{username}:{realm}:{password}";
            var keyStringByteArray = System.Text.Encoding.ASCII.GetBytes(keyString);
            var md5HashByteArray = md5.ComputeHash(keyStringByteArray);
            var hmacSHA1 = new HMACSHA1(md5HashByteArray);
            md5.Clear();
            var hmacSHA1ByteArray = hmacSHA1.ComputeHash(data);
            this.messageIntegrity = hmacSHA1ByteArray;
            hmacSHA1.Clear();
        }
        public override byte[] ToBytes()
        {
            var typeByteArray = this.type.ToBytes();
            var length = this.messageIntegrity.Length;
            var lengthByteArray = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }

            var res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, this.messageIntegrity);
            return res;
        }

        public static MessageIntegrity Parse(byte[] data)
        {
            return new MessageIntegrity(data);
        }
    }
}