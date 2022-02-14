using System;

namespace Doturn.StunAttribute
{
    public class Data : StunAttributeBase
    {
        public readonly Type type = Type.DATA;
        public readonly byte[] data;
        public override Type Type => type;

        public Data()
        {
            data = Array.Empty<byte>();
        }
        public Data(byte[] data)
        {
            this.data = data;
        }
        public override byte[] ToBytes()
        {
            byte[] typeByteArray = type.ToBytes();
            int length = data.Length;
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
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, data);
            byte[] padding = { 0 };
            for (int i = 0; i < paddingLength; i++)
            {
                ByteArrayUtils.MergeByteArray(ref res, res.Length - paddingLength, padding);
            }
            return res;
        }

        public static Data Parse(byte[] data)
        {
            return new Data(data);
        }
    }
}