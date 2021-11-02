using System;
using System.Linq;
namespace doturn
{

    interface IStunAttribute
    {
        byte[] ToByte();
    }
    class StunAttribute : IStunAttribute
    {
        public readonly StunAttrType attr;
        private byte[] data;

        public StunAttribute(StunAttrType attr, byte[] data)
        {
            this.attr = attr;
            this.data = data;
        }
        public byte[] ToByte()
        {
            Int16 length = (Int16)this.data.Length;
            var res = new byte[2 + 2 + length];
            var attrByte = this.attr.ToByte();
            var lengthByte = BitConverter.GetBytes(length);
            int endPos = 0;
            Array.Copy(attrByte, 0, res, endPos, attrByte.Length);
            endPos += attrByte.Length;
            Array.Copy(lengthByte, 0, res, endPos, lengthByte.Length);
            endPos += lengthByte.Length;
            Array.Copy(data, 0, res, endPos, data.Length);
            return res;
        }
    }
}