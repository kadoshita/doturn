using System;

namespace Doturn.StunAttribute
{
    public class Realm : StunAttributeBase
    {
        public readonly Type type = Type.REALM;
        public readonly string realm;
        public override Type Type => type;
        public Realm()
        {
            realm = "example.com";
        }
        public Realm(string realm)
        {
            this.realm = realm;
        }

        public override byte[] ToBytes()
        {
            byte[] typeByteArray = type.ToBytes();
            byte[] realmByteArray = System.Text.Encoding.ASCII.GetBytes(realm);
            int length = realmByteArray.Length;
            int paddingLength = 8 - ((2 + 2 + length) % 8);
            byte[] lengthByteArray = BitConverter.GetBytes((short)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }
            byte[] res = new byte[2 + 2 + length + paddingLength];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, realmByteArray);
            byte[] padding = { 0 };
            for (int i = 0; i < paddingLength; i++)
            {
                ByteArrayUtils.MergeByteArray(ref res, res.Length - paddingLength, padding);
            }
            return res;
        }

        public static Realm Parse(byte[] data)
        {
            string realmStr = System.Text.Encoding.ASCII.GetString(data);
            return new Realm(realmStr);
        }
    }
}