using System;
using System.Linq;

namespace Doturn.StunAttribute
{
    public class Nonce : StunAttributeBase
    {
        public readonly Type type = Type.NONCE;
        public readonly string nonce;
        private static readonly Random s_random = new();
        public override Type Type => type;
        public Nonce()
        {
            nonce = generateNonce(16);
        }
        public Nonce(string nonce)
        {
            this.nonce = nonce;
        }

        public override byte[] ToBytes()
        {
            byte[] typeByteArray = type.ToBytes();
            byte[] nonceByteArray = System.Text.Encoding.ASCII.GetBytes(nonce);
            int length = nonceByteArray.Length;
            byte[] lengthByteArray = BitConverter.GetBytes((short)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }
            byte[] res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, nonceByteArray);
            return res;
        }
        private static string generateNonce(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[s_random.Next(s.Length)]).ToArray());
        }

        public static Nonce Parse(byte[] data)
        {
            string nonceStr = System.Text.Encoding.ASCII.GetString(data);
            return new Nonce(nonceStr);
        }
    }
}