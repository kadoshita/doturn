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
            byte[] lengthByteArray = BitConverter.GetBytes((short)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }
            byte[] res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, data);
            return res;
        }

        public static Data Parse(byte[] data)
        {
            return new Data(data);
        }
    }
}