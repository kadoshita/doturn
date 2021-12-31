using System;
using System.Linq;

namespace Doturn.StunAttribute
{
    public class Nonce : StunAttributeBase
    {
        public readonly Type type = Type.NONCE;
        public readonly string nonce;
        private static Random random = new Random();
        public override Type Type => this.type;
        public Nonce()
        {
            this.nonce = this.generateNonce(16);
        }
        public Nonce(string nonce)
        {
            this.nonce = nonce;
        }

        public override byte[] ToByte()
        {
            var typeByteArray = this.type.ToByte();
            var nonceByteArray = System.Text.Encoding.ASCII.GetBytes(this.nonce);
            var length = nonceByteArray.Length;
            var lengthByteArray = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }
            var res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, nonceByteArray);
            return res;
        }
        private string generateNonce(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static Nonce Parse(byte[] data)
        {
            var nonceStr = System.Text.Encoding.ASCII.GetString(data);
            return new Nonce(nonceStr);
        }
    }
}