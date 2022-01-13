using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class RequestedTransportTest
    {
        [Theory]
        [InlineData(Transport.UDP, "00-19-00-04-11-00-00-00")]
        [InlineData(Transport.TCP, "00-19-00-04-06-00-00-00")]
        public void RequestedTransport_Convert_To_ByteArray(Transport transport, string expect)
        {
            var requestedTransport = new RequestedTransport(transport, new byte[] { 0x00, 0x00, 0x00 });
            byte[] byteArray = requestedTransport.ToBytes();
            string byteArrayString = BitConverter.ToString(byteArray);
            Assert.Equal(expect, byteArrayString);
        }
    }
}