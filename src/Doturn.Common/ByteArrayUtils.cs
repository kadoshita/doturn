using System;

namespace Doturn.Common
{
    public static class ByteArrayUtils
    {
        public static void MergeByteArray(ref byte[] res, params byte[][] data)
        {
            int endPos = 0;
            foreach (var datum in data)
            {
                Buffer.BlockCopy(datum, 0, res, endPos, datum.Length);
                endPos += datum.Length;
            }
        }
    }
}
