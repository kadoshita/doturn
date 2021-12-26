using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class MappedAddressTest
    {
        [Theory]
        [InlineData("127.0.0.1", 20000, "00-01-00-08-00-01-4E-20-7F-00-00-01")]
        [InlineData("203.0.113.1", 20000, "00-01-00-08-00-01-4E-20-CB-00-71-01")]
        [InlineData("192.0.2.1", 20000, "00-01-00-08-00-01-4E-20-C0-00-02-01")]
        public void MappedAddress_Convert_To_ByteArray(string address, int port, string expect)
        {
            var magicCookie = BitConverter.GetBytes((Int32)0x2112a442);
            var mappedAddress = new MappedAddress(address, port, magicCookie);
            var byteArray = mappedAddress.ToByte();
            var byteArrayString = BitConverter.ToString(byteArray);
            Assert.Equal(expect, byteArrayString);
        }
    }
}