using System;

namespace Doturn.StunAttribute
{
    public class Lifetime : StunAttributeBase
    {
        public readonly Type type = Type.LIFETIME;
        public readonly int lifetime;
        public override Type Type => this.type;

        public Lifetime()
        {
            this.lifetime = 600;
        }
        public Lifetime(int lifetime)
        {
            this.lifetime = lifetime;
        }
        public override byte[] ToBytes()
        {
            var typeByteArray = this.type.ToBytes();
            var lifetimeByteArray = BitConverter.GetBytes(this.lifetime);
            var length = lifetimeByteArray.Length;
            var lengthByteArray = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lifetimeByteArray);
                Array.Reverse(lengthByteArray);
            }
            var res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, lifetimeByteArray);
            return res;
        }

        public static Lifetime Parse(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            var lifetimeNum = BitConverter.ToUInt16(data);
            return new Lifetime(lifetimeNum);
        }
    }
}