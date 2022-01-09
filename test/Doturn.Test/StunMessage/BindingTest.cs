using System;
using System.Net;
using Xunit;

namespace Doturn.StunMessage.Test
{
    public class BindingTest
    {
        private readonly byte[] magicCookie = BitConverter.GetBytes((Int32)0x2112a442);
        private readonly byte[] transactionId = new byte[] { 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58 };
        private byte[] bindingRequestByteArray = new byte[0];

        private readonly byte[] bindingSuccessResponseByteArray = new byte[] {
            0x01, 0x01, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58, // header
            0x00, 0x01, 0x00, 0x08, 0x00, 0x01, 0x4E, 0x20, 0x7F, 0x00, 0x00, 0x01, // MappedAddress
        };
        private readonly byte[] bindingXorSuccessResponseByteArray = new byte[] {
            0x01, 0x01, 0x00, 0x0C, 0x21, 0x12, 0xa4, 0x42, 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58, // header
            0x00, 0x20, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0x5E, 0x12, 0xA4, 0x43, // XorMappedAddress
        };

        private readonly byte[] bindingErrorResponseByteArray = new byte[] {
            0x01, 0x11, 0x00, 0x00, 0x21, 0x12, 0xa4, 0x42, 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58, // header
        };

        [Fact]
        public void Parse_And_Convert_To_ByteArray_BindingRequest()
        {
            var bindingRequest = new Binding(this.bindingRequestByteArray);
            var convertedBindingRequestByteArray = bindingRequest.ToBytes();
            Assert.Equal(this.bindingRequestByteArray, convertedBindingRequestByteArray);
        }

        [Fact]
        public void CreateSuccessResponse()
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, 20000);
            var successResponseByteArray = Binding.CreateSuccessResponse(this.transactionId, BitConverter.GetBytes((Int32)0), endpoint);
            Assert.Equal(this.bindingSuccessResponseByteArray, successResponseByteArray);
        }

        [Fact]
        public void CreateXorSuccessResponse()
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, 20000);
            var successResponseByteArray = Binding.CreateSuccessResponse(this.transactionId, this.magicCookie, endpoint);
            Assert.Equal(this.bindingXorSuccessResponseByteArray, successResponseByteArray);
        }

        [Fact]
        public void CreateErrorResponse()
        {
            var errorResponseByteArray = Binding.CreateErrorResponse(this.transactionId);
            Assert.Equal(this.bindingErrorResponseByteArray, errorResponseByteArray);
        }
    }
}