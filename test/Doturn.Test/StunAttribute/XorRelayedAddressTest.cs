using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class XorRelayedAddressTest
    {
        [Theory]
        [InlineData("127.0.0.1", 20000, "00-16-00-08-00-01-62-EA-3D-A4-12-20")]
        [InlineData("203.0.113.1", 20000, "00-16-00-08-00-01-62-EA-89-A4-63-20")]
        [InlineData("192.0.2.1", 20000, "00-16-00-08-00-01-62-EA-82-A4-10-20")]
        public void XorRelayedAddress_Convert_To_ByteArray(string address, int port, string expect)
        {
            var xorRelayedAddress = new XorRelayedAddress(address, port);
            var byteArray = xorRelayedAddress.ToByte();
            var byteArrayString = BitConverter.ToString(byteArray);
            Assert.Equal(expect, byteArrayString);
        }
    }
}