using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class MessageIntegrityTest
    {
        [Theory]
        [InlineData(new byte[] { 0x00 }, "00-08-00-14-62-96-E4-0C-8F-6F-CD-5F-1C-6E-BC-7E-A0-55-C7-85-B5-CF-C4-20")]
        [InlineData(new byte[] { 0x10, 0xfa, 0x49, 0xff }, "00-08-00-14-7F-B7-55-13-86-D3-F6-8E-68-EC-9A-9C-D9-CB-8A-55-0D-56-20-37")]
        public void MessageIntegrity_Convert_To_ByteArray(byte[] data, string expect)
        {
            var messageIntegrity = new MessageIntegrity("username", "password", "example.com", data);
            var byteArray = messageIntegrity.ToBytes();
            var byteArrayString = BitConverter.ToString(byteArray);
            Assert.Equal(expect, byteArrayString);
        }

        [Fact]
        public void MessageIntegrity_Set_ByteArray_In_Constructor()
        {
            byte[] data = { 0x62, 0x96, 0xE4, 0x0C, 0x8F, 0x6F, 0xCD, 0x5F, 0x1C, 0x6E, 0xBC, 0x7E, 0xA0, 0x55, 0xC7, 0x85, 0xB5, 0xCF, 0xC4, 0x20 };
            var messageIntegrity = new MessageIntegrity(data);
            var byteArray = messageIntegrity.ToBytes();
            var byteArrayString = BitConverter.ToString(byteArray);
            Assert.Equal("00-08-00-14-62-96-E4-0C-8F-6F-CD-5F-1C-6E-BC-7E-A0-55-C7-85-B5-CF-C4-20", byteArrayString);
        }
    }
}