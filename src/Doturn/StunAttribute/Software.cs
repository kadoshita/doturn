using System;

namespace Doturn.StunAttribute
{
    public class Software : StunAttributeBase
    {
        public readonly Type type = Type.SOFTWARE;
        public readonly string software;
        public override Type Type => type;
        public Software()
        {
            software = "Doturn";
        }
        public Software(string software)
        {
            this.software = software;
        }

        public override byte[] ToBytes()
        {
            byte[] typeByteArray = type.ToBytes();
            byte[] softwareByteArray = System.Text.Encoding.ASCII.GetBytes(software);
            int length = softwareByteArray.Length;
            int paddingLength = 8 - (length % 8);
            if (paddingLength >= 8)
            {
                paddingLength = 0;
            }
            byte[] lengthByteArray = BitConverter.GetBytes((short)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }
            byte[] res = new byte[2 + 2 + length + paddingLength];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, softwareByteArray);
            byte[] padding = { 0 };
            for (int i = 0; i < paddingLength; i++)
            {
                ByteArrayUtils.MergeByteArray(ref res, res.Length - paddingLength, padding);
            }
            return res;
        }
        public static Software Parse(byte[] data)
        {
            string softwareStr = System.Text.Encoding.ASCII.GetString(data);
            return new Software(softwareStr);
        }
    }
}