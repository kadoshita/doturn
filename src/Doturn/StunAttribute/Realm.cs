using System;

namespace Doturn.StunAttribute
{
    public class Realm : StunAttributeBase
    {
        public readonly Type type = Type.REALM;
        public readonly string realm;
        public override Type Type => this.type;
        public Realm()
        {
            this.realm = "example.com";
        }
        public Realm(string realm)
        {
            this.realm = realm;
        }

        public override byte[] ToByte()
        {
            var typeByteArray = this.type.ToByte();
            var realmByteArray = System.Text.Encoding.ASCII.GetBytes(this.realm);
            var length = realmByteArray.Length;
            var paddingLength = 8 - ((2 + 2 + length) % 8);
            var lengthByteArray = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }
            var res = new byte[2 + 2 + length + paddingLength];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, realmByteArray);
            byte[] padding = { 0 };
            for (var i = 0; i < paddingLength; i++)
            {
                ByteArrayUtils.MergeByteArray(ref res, res.Length - paddingLength, padding);
            }
            return res;
        }
    }
}