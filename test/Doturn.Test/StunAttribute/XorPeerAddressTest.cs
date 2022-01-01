using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class XorPeerAddressTest
    {
        [Theory]
        [InlineData("127.0.0.1", 20000, "00-12-00-08-00-01-6F-32-5E-12-A4-43")]
        [InlineData("203.0.113.1", 20000, "00-12-00-08-00-01-6F-32-EA-12-D5-43")]
        [InlineData("192.0.2.1", 20000, "00-12-00-08-00-01-6F-32-E1-12-A6-43")]
        [InlineData("94.36.122.203", 20000, "00-12-00-08-00-01-6F-32-7F-36-DE-89")]
        public void XorPeerAddress_Convert_To_ByteArray(string address, int port, string expect)
        {
            var xorPeerAddress = new XorPeerAddress(address, port);
            var byteArray = xorPeerAddress.ToBytes();
            var byteArrayString = BitConverter.ToString(byteArray);
            Assert.Equal(expect, byteArrayString);
        }
    }
}