using System;
using Force.Crc32;

namespace Doturn.StunAttribute
{
    public class Fingerprint : StunAttributeBase
    {
        public readonly Type type = Type.FINGERPRINT;
        private readonly byte[] _crc32;
        public override Type Type => type;

        public Fingerprint(byte[] crc32)
        {
            _crc32 = crc32;
        }
        public static Fingerprint CreateFingerprint(byte[] data)
        {
            uint crc32 = Crc32Algorithm.Compute(data, 0, data.Length);
            byte[] crc32Byte = BitConverter.GetBytes(crc32);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(crc32Byte);
            }
            byte[] crc32XorByte = new byte[crc32Byte.Length];
            byte[] fingerprintXor = BitConverter.GetBytes(0x5354554e);
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
        public override byte[] ToBytes()
        {
            byte[] typeByteArray = type.ToBytes();
            int length = _crc32.Length;
            byte[] lengthByteArray = BitConverter.GetBytes((short)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }

            byte[] res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, _crc32);
            return res;
        }

        public static Fingerprint Parse(byte[] data)
        {
            return new Fingerprint(data);
        }
    }
}