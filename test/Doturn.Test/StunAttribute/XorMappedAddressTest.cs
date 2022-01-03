using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class XorMappedAddressTest
    {
        [Theory]
        [InlineData("127.0.0.1", 20000, new byte[] { 0x00, 0x20, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0x5E, 0x12, 0xA4, 0x43 }, "94.18.164.67")]
        [InlineData("203.0.113.1", 20000, new byte[] { 0x00, 0x20, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0xEA, 0x12, 0xD5, 0x43 }, "234.18.213.67")]
        [InlineData("192.0.2.1", 20000, new byte[] { 0x00, 0x20, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0xE1, 0x12, 0xA6, 0x43 }, "225.18.166.67")]
        [InlineData("94.36.122.203", 20000, new byte[] { 0x00, 0x20, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0x7F, 0x36, 0xDE, 0x89 }, "127.54.222.137")]
        public void XorMappedAddress_Convert_To_ByteArray(string xorAddress, UInt16 xorPort, byte[] expect, string address)
        {
            var xorMappedAddress = new XorMappedAddress(xorAddress, xorPort);
            var byteArray = xorMappedAddress.ToBytes();
            Assert.Equal(expect, byteArray);
            Assert.Equal(xorAddress, xorMappedAddress.xorEndpoint.Address.ToString());
            Assert.Equal(xorPort, xorMappedAddress.xorEndpoint.Port);
            Assert.Equal(28466, xorMappedAddress.endpoint.Port);
            Assert.Equal(address, xorMappedAddress.endpoint.Address.ToString());
        }
    }
}