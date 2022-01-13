using System;

namespace Doturn.StunAttribute
{
    public class Lifetime : StunAttributeBase
    {
        public readonly Type type = Type.LIFETIME;
        public readonly int lifetime;
        public override Type Type => type;

        public Lifetime()
        {
            lifetime = 600;
        }
        public Lifetime(int lifetime)
        {
            this.lifetime = lifetime;
        }
        public override byte[] ToBytes()
        {
            byte[] typeByteArray = type.ToBytes();
            byte[] lifetimeByteArray = BitConverter.GetBytes(lifetime);
            int length = lifetimeByteArray.Length;
            byte[] lengthByteArray = BitConverter.GetBytes((short)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lifetimeByteArray);
                Array.Reverse(lengthByteArray);
            }
            byte[] res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, lifetimeByteArray);
            return res;
        }

        public static Lifetime Parse(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            ushort lifetimeNum = BitConverter.ToUInt16(data);
            return new Lifetime(lifetimeNum);
        }
    }
}