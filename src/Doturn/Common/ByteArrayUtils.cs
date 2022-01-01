using System;

namespace Doturn
{
    public static class ByteArrayUtils
    {
        public static void MergeByteArray(ref byte[] res, params byte[][] data)
        {
            _MergeByteArray(ref res, 0, data);
        }
        public static void MergeByteArray(ref byte[] res, int offset, params byte[][] data)
        {
            _MergeByteArray(ref res, offset, data);
        }

        private static void _MergeByteArray(ref byte[] res, int offset, params byte[][] data)
        {
            int endPos = offset;
            foreach (var datum in data)
            {
                Buffer.BlockCopy(datum, 0, res, endPos, datum.Length);
                endPos += datum.Length;
            }
        }

        public static byte[] XorPort(byte[] portByteArray)
        {
            var magicCookie = BitConverter.GetBytes((Int32)0x2112a442);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(magicCookie);
            }
            var xorPortByteArray = new byte[portByteArray.Length];
            for (var i = 0; i < portByteArray.Length; i++)
            {
                xorPortByteArray[i] = (byte)(portByteArray[i] ^ magicCookie[i]);
            }

            return xorPortByteArray;
        }

        public static byte[] XorAddress(byte[] addressByteArray)
        {
            var magicCookie = BitConverter.GetBytes((Int32)0x2112a442);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(magicCookie);
            }
            var xorAddressByteArray = new byte[addressByteArray.Length];
            for (var i = 0; i < addressByteArray.Length; i++)
            {
                xorAddressByteArray[i] = (byte)(addressByteArray[i] ^ magicCookie[i]);
            }

            return xorAddressByteArray;
        }
    }
}
