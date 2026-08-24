using Xunit;

namespace Doturn.Test
{
    public class ByteArrayUtilsTest
    {
        [Fact]
        public void MergeByteArray_Without_Offset()
        {
            byte[] res = new byte[6];
            ByteArrayUtils.MergeByteArray(ref res, new byte[] { 0x01, 0x02 }, new byte[] { 0x03, 0x04, 0x05, 0x06 });
            Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 }, res);
        }

        [Fact]
        public void MergeByteArray_With_Offset()
        {
            byte[] res = new byte[6];
            ByteArrayUtils.MergeByteArray(ref res, 2, new byte[] { 0xAA, 0xBB });
            Assert.Equal(new byte[] { 0x00, 0x00, 0xAA, 0xBB, 0x00, 0x00 }, res);
        }

        [Fact]
        public void XorPort_With_MagicCookie()
        {
            // port 20000 (0x4E20) xor magic cookie first 2 bytes (0x21, 0x12)
            byte[] result = ByteArrayUtils.XorPort(new byte[] { 0x4E, 0x20 });
            Assert.Equal(new byte[] { 0x6F, 0x32 }, result);
        }

        [Fact]
        public void XorAddress_With_MagicCookie()
        {
            // 127.0.0.1 xor magic cookie (0x21, 0x12, 0xA4, 0x42)
            byte[] result = ByteArrayUtils.XorAddress(new byte[] { 0x7F, 0x00, 0x00, 0x01 });
            Assert.Equal(new byte[] { 0x5E, 0x12, 0xA4, 0x43 }, result);
        }
    }
}
