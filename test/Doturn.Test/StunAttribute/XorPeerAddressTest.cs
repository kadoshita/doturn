using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class XorPeerAddressTest
    {
        [Theory]
        [InlineData("127.0.0.1", 20000, new byte[] { 0x00, 0x12, 0x00, 0x08, 0x00, 0x01, 0x20, 0x4E, 0x7F, 0x00, 0x00, 0x01 }, "94.18.164.67")]
        [InlineData("203.0.113.1", 20000, new byte[] { 0x00, 0x12, 0x00, 0x08, 0x00, 0x01, 0x20, 0x4E, 0xCB, 0x00, 0x71, 0x01 }, "234.18.213.67")]
        [InlineData("192.0.2.1", 20000, new byte[] { 0x00, 0x12, 0x00, 0x08, 0x00, 0x01, 0x20, 0x4E, 0xC0, 0x00, 0x02, 0x01 }, "225.18.166.67")]
        [InlineData("94.36.122.203", 20000, new byte[] { 0x00, 0x12, 0x00, 0x08, 0x00, 0x01, 0x20, 0x4E, 0x5E, 0x24, 0x7A, 0xCB }, "127.54.222.137")]
        public void XorPeerAddress_Convert_To_ByteArray(string xorAddress, UInt16 xorPort, byte[] expect, string address)
        {
            var xorPeerAddress = new XorPeerAddress(xorAddress, xorPort);
            var byteArray = xorPeerAddress.ToBytes();
            Assert.Equal(expect, byteArray);
            Assert.Equal(xorAddress, xorPeerAddress.xorEndpoint.Address.ToString());
            Assert.Equal(xorPort, xorPeerAddress.xorEndpoint.Port);
            Assert.Equal(28466, xorPeerAddress.endpoint.Port);
            Assert.Equal(address, xorPeerAddress.endpoint.Address.ToString());
        }
    }
}