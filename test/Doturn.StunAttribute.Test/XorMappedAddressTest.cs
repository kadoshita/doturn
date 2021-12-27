using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class XorMappedAddressTest
    {
        [Theory]
        [InlineData("127.0.0.1", 20000, "00-20-00-08-00-01-62-EA-3D-A4-12-20")]
        [InlineData("203.0.113.1", 20000, "00-20-00-08-00-01-62-EA-89-A4-63-20")]
        [InlineData("192.0.2.1", 20000, "00-20-00-08-00-01-62-EA-82-A4-10-20")]
        public void XorMappedAddress_Convert_To_ByteArray(string address, int port, string expect)
        {
            var magicCookie = BitConverter.GetBytes((Int32)0x2112a442);
            var xorMappedAddress = new XorMappedAddress(address, port, magicCookie);
            var byteArray = xorMappedAddress.ToByte();
            var byteArrayString = BitConverter.ToString(byteArray);
            Assert.Equal(expect, byteArrayString);
        }
    }
}