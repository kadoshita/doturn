using System;

namespace Doturn.StunAttribute
{
    public class Software : StunAttributeBase
    {
        public readonly Type type = Type.SOFTWARE;
        public readonly string software;
        public override Type Type => this.type;
        public Software()
        {
            this.software = "Doturn";
        }
        public Software(string software)
        {
            this.software = software;
        }

        public override byte[] ToBytes()
        {
            var typeByteArray = this.type.ToBytes();
            var softwareByteArray = System.Text.Encoding.ASCII.GetBytes(this.software);
            var length = softwareByteArray.Length;
            var paddingLength = 8 - (length % 8);
            if (paddingLength >= 8)
            {
                paddingLength = 0;
            }
            var lengthByteArray = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }
            var res = new byte[2 + 2 + length + paddingLength];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, softwareByteArray);
            byte[] padding = { 0 };
            for (var i = 0; i < paddingLength; i++)
            {
                ByteArrayUtils.MergeByteArray(ref res, res.Length - paddingLength, padding);
            }
            return res;
        }
        public static Software Parse(byte[] data)
        {
            var softwareStr = System.Text.Encoding.ASCII.GetString(data);
            return new Software(softwareStr);
        }
    }
}