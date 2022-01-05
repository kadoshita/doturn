using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class XorPeerAddressTest
    {
        [Theory]
        [InlineData("127.0.0.1", 20000, new byte[] { 0x00, 0x12, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0x5E, 0x12, 0xA4, 0x43 }, "94.18.164.67")]
        [InlineData("203.0.113.1", 20000, new byte[] { 0x00, 0x12, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0xEA, 0x12, 0xD5, 0x43 }, "234.18.213.67")]
        [InlineData("192.0.2.1", 20000, new byte[] { 0x00, 0x12, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0xE1, 0x12, 0xA6, 0x43 }, "225.18.166.67")]
        [InlineData("94.36.122.203", 20000, new byte[] { 0x00, 0x12, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0x7F, 0x36, 0xDE, 0x89 }, "127.54.222.137")]
        public void XorPeerAddress_Convert_To_ByteArray(string address, UInt16 port, byte[] expect, string xorAddress)
        {
            var xorPeerAddress = new XorPeerAddress(address, port);
            var byteArray = xorPeerAddress.ToBytes();
            Assert.Equal(expect, byteArray);
            Assert.Equal(xorAddress, xorPeerAddress.xorEndpoint.Address.ToString());
            Assert.Equal(28466, xorPeerAddress.xorEndpoint.Port);
            Assert.Equal(port, xorPeerAddress.endpoint.Port);
            Assert.Equal(address, xorPeerAddress.endpoint.Address.ToString());
        }
    }
}