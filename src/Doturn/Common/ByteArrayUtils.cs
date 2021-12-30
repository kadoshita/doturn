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
    }
}
