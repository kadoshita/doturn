using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class XorPeerAddressTest
    {
        [Theory]
        [InlineData("127.0.0.1", 20000, new byte[] { 0x00, 0x12, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0x5E, 0x12, 0xA4, 0x43 }, "94.18.164.67", 28466)]
        [InlineData("203.0.113.1", 20000, new byte[] { 0x00, 0x12, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0xEA, 0x12, 0xD5, 0x43 }, "234.18.213.67", 28466)]
        [InlineData("192.0.2.1", 20000, new byte[] { 0x00, 0x12, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0xE1, 0x12, 0xA6, 0x43 }, "225.18.166.67", 28466)]
        [InlineData("94.36.122.203", 20000, new byte[] { 0x00, 0x12, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0x7F, 0x36, 0xDE, 0x89 }, "127.54.222.137", 28466)]
        public void XorPeerAddress_Convert_To_ByteArray(string realAddress, UInt16 realPort, byte[] expect, string address, UInt16 port)
        {
            var xorPeerAddress = new XorPeerAddress(realAddress, realPort);
            var byteArray = xorPeerAddress.ToBytes();
            Assert.Equal(expect, byteArray);
            Assert.Equal(realAddress, xorPeerAddress.realEndpoint.Address.ToString());
            Assert.Equal(realPort, xorPeerAddress.realEndpoint.Port);
            Assert.Equal(port, xorPeerAddress.endpoint.Port);
            Assert.Equal(address, xorPeerAddress.endpoint.Address.ToString());
        }
    }
}