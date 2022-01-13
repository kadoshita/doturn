using System;
using Xunit;

namespace Doturn.Test
{
    public class StunHeaderTest
    {
        private readonly byte[] _bindingRequestByteArray = new byte[]{
            0x00, 0x01, 0x00, 0x00, 0x21, 0x12, 0xa4, 0x42, 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30,
            0x6e, 0x6c, 0x69, 0x58
        };
        private readonly byte[] _bindingRequestZeroMagicCookieByteArray = new byte[]{
            0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30,
            0x6e, 0x6c, 0x69, 0x58
        };

        [Fact]
        public void Parse_StunHeader()
        {
            var stunHeader = new StunHeader(_bindingRequestByteArray);
            Assert.Equal(StunMessage.Type.BINDING, stunHeader.type);
            Assert.Equal(0, stunHeader.messageLength);
            Assert.Equal(new byte[] { 0x21, 0x12, 0xa4, 0x42 }, stunHeader.magicCookie);
            Assert.Equal(new byte[] { 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58 }, stunHeader.transactionId);
        }

        [Fact]
        public void Convert_To_ByteArray_StunHeader()
        {
            StunMessage.Type type = StunMessage.Type.BINDING;
            short messageLength = 0;
            byte[] transactionId = new byte[] { 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58 };
            var stunHeader = new StunHeader(type, messageLength, transactionId);
            byte[] stunHeaderByteArray = stunHeader.ToBytes();
            Assert.Equal(_bindingRequestByteArray, stunHeaderByteArray);
        }
        [Fact]
        public void Convert_To_ByteArray_StunHeader_With_MagicCookie()
        {
            StunMessage.Type type = StunMessage.Type.BINDING;
            short messageLength = 0;
            byte[] magicCookie = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            byte[] transactionId = new byte[] { 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58 };
            var stunHeader = new StunHeader(type, messageLength, magicCookie, transactionId);
            byte[] stunHeaderByteArray = stunHeader.ToBytes();
            Assert.Equal(_bindingRequestZeroMagicCookieByteArray, stunHeaderByteArray);
        }
    }
}