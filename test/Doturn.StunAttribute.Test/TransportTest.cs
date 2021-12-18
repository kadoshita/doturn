using System;
using Doturn;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class TransportTest
    {
        [Theory]
        [InlineData(StunAttribute.Transport.UDP, "11")]
        [InlineData(StunAttribute.Transport.TCP, "06")]
        public void Transport_Convert_To_ByteArray(StunAttribute.Transport transport, string byteArrayString)
        {
            var byteArray = transport.ToByte();
            Assert.Equal(1, byteArray.Length);
            Assert.Equal(byteArrayString, BitConverter.ToString(byteArray));
        }
    }
}
