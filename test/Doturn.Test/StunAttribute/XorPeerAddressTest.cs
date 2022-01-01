using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class XorPeerAddressTest
    {
        [Theory]
        [InlineData("127.0.0.1", 20000, "00-12-00-08-00-01-62-EA-3D-A4-12-20")]
        [InlineData("203.0.113.1", 20000, "00-12-00-08-00-01-62-EA-89-A4-63-20")]
        [InlineData("192.0.2.1", 20000, "00-12-00-08-00-01-62-EA-82-A4-10-20")]
        public void XorPeerAddress_Convert_To_ByteArray(string address, int port, string expect)
        {
            var xorPeerAddress = new XorPeerAddress(address, port);
            var byteArray = xorPeerAddress.ToBytes();
            var byteArrayString = BitConverter.ToString(byteArray);
            Assert.Equal(expect, byteArrayString);
        }
    }
}