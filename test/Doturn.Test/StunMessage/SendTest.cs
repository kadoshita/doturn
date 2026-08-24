using System;
using Xunit;

namespace Doturn.StunMessage.Test
{
    public class SendTest
    {
        private readonly byte[] _payload = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

        // DATA attribute TLV: type 0x0013, length 8, value = _payload
        private readonly byte[] _dataAttributeByteArray = new byte[] {
            0x00, 0x13, 0x00, 0x08, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
        };

        [Fact]
        public void Parse_And_ToApplicationDataBytes()
        {
            var send = new Send(_dataAttributeByteArray);
            byte[] result = send.ToApplicationDataBytes();
            Assert.Equal(_payload, result);
        }

        [Fact]
        public void ToBytes_Throws_NotImplementedException()
        {
            var send = new Send(_dataAttributeByteArray);
            Assert.Throws<NotImplementedException>(() => send.ToBytes());
        }
    }
}
