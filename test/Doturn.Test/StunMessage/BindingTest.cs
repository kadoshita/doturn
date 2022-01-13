using System;
using System.Net;
using Xunit;

namespace Doturn.StunMessage.Test
{
    public class BindingTest
    {
        private readonly byte[] _magicCookie = new byte[] { 0x21, 0x12, 0xA4, 0x42 };
        private readonly byte[] _transactionId = new byte[] { 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58 };
        private readonly byte[] _bindingRequestByteArray = Array.Empty<byte>();

        private readonly byte[] _bindingSuccessResponseByteArray = new byte[] {
            0x01, 0x01, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58, // header
            0x00, 0x01, 0x00, 0x08, 0x00, 0x01, 0x4E, 0x20, 0x7F, 0x00, 0x00, 0x01, // MappedAddress
        };
        private readonly byte[] _bindingXorSuccessResponseByteArray = new byte[] {
            0x01, 0x01, 0x00, 0x0C, 0x21, 0x12, 0xa4, 0x42, 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58, // header
            0x00, 0x20, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0x5E, 0x12, 0xA4, 0x43, // XorMappedAddress
        };

        private readonly byte[] _bindingErrorResponseByteArray = new byte[] {
            0x01, 0x11, 0x00, 0x00, 0x21, 0x12, 0xa4, 0x42, 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58, // header
        };

        private readonly AppSettings _appSettings = new()
        {
            Username = "username",
            Password = "password",
            Realm = "example.com",
            ExternalIPAddress = "127.0.0.1",
            ListeningPort = 3478,
            MinPort = 49152,
            MaxPort = 65535
        };

        [Fact]
        public void Parse_And_Convert_To_ByteArray_BindingRequest()
        {
            var bindingRequest = new Binding(_magicCookie, _transactionId, _appSettings);
            byte[] convertedBindingRequestByteArray = bindingRequest.ToBytes();
            Assert.Equal(_bindingRequestByteArray, convertedBindingRequestByteArray);
        }

        [Fact]
        public void CreateSuccessResponse()
        {
            var bindingRequest = new Binding(BitConverter.GetBytes((int)0), _transactionId, _appSettings);
            IPEndPoint endpoint = new(IPAddress.Loopback, 20000);
            byte[] successResponseByteArray = bindingRequest.CreateSuccessResponse(endpoint);
            Assert.Equal(_bindingSuccessResponseByteArray, successResponseByteArray);
        }

        [Fact]
        public void CreateXorSuccessResponse()
        {
            var bindingRequest = new Binding(_magicCookie, _transactionId, _appSettings);
            IPEndPoint endpoint = new(IPAddress.Loopback, 20000);
            byte[] successResponseByteArray = bindingRequest.CreateSuccessResponse(endpoint);
            Assert.Equal(_bindingXorSuccessResponseByteArray, successResponseByteArray);
        }

        [Fact]
        public void CreateErrorResponse()
        {
            var bindingRequest = new Binding(_magicCookie, _transactionId, _appSettings);
            byte[] errorResponseByteArray = bindingRequest.CreateErrorResponse();
            Assert.Equal(_bindingErrorResponseByteArray, errorResponseByteArray);
        }
    }
}