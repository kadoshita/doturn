using System;
using Force.Crc32;

namespace Doturn.StunAttribute
{
    public class Fingerprint : StunAttributeBase
    {
        public readonly Type type = Type.FINGERPRINT;
        private readonly byte[] crc32;
        public override Type Type => this.type;

        public Fingerprint(byte[] crc32)
        {
            this.crc32 = crc32;
        }
        public static Fingerprint CreateFingerprint(ref byte[] data)
        {
            var crc32 = Crc32Algorithm.Compute(data, 0, data.Length);
            var crc32Byte = BitConverter.GetBytes(crc32);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(crc32Byte);
            }
            var crc32XorByte = new byte[crc32Byte.Length];
            var fingerprintXor = BitConverter.GetBytes(0x5354554e);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(fingerprintXor);
            }
            for (int i = 0; i < crc32Byte.Length; i++)
            {
                crc32XorByte[i] = (byte)(crc32Byte[i] ^ fingerprintXor[i]);
            }
            return new Fingerprint(crc32XorByte);
        }
        public override byte[] ToByte()
        {
            var typeByteArray = this.type.ToByte();
            var length = this.crc32.Length;
            var lengthByteArray = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }

            var res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, this.crc32);
            return res;
        }

        public static Fingerprint Parse(byte[] data)
        {
            return new Fingerprint(data);
        }
    }
}