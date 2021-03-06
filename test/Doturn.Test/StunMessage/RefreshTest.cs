using Xunit;

namespace Doturn.StunMessage.Test
{
    public class RefreshTest
    {
        private readonly byte[] _magicCookie = new byte[] { 0x21, 0x12, 0xA4, 0x42 };
        private readonly byte[] _transactionId = new byte[] { 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58 };
        private readonly byte[] _refreshRequestByteArray = new byte[] {
            0x00, 0x0D, 0x00, 0x04, 0x00, 0x00, 0x02, 0x58, // Lifetime
            0x00, 0x06, 0x00, 0x08, 0x75, 0x73, 0x65, 0x72, 0x6E, 0x61, 0x6D, 0x65, // Username username
            0x00, 0x14, 0x00, 0x0B, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D, 0x00, // Realm example.com
            0x00, 0x15, 0x00, 0x10, 0x68, 0x6F, 0x32, 0x79, 0x64, 0x77, 0x35, 0x71, 0x65, 0x65, 0x71, 0x73, 0x67, 0x61, 0x73, 0x7A, // Nonce ho2ydw5qeeqsgasz
            0x80, 0x28, 0x00, 0x04, 0x76, 0xbf, 0x9e, 0x09 // Fingerprint (incorrect!!)
        };
        private readonly byte[] _refreshSuccessResponseByteArray = new byte[] {
            0x01, 0x04, 0x00, 0x34, 0x21, 0x12, 0xA4, 0x42, 0x39, 0x50, 0x4D, 0x4B, 0x64, 0x63, 0x79, 0x30, 0x6E, 0x6C, 0x69, 0x58, // header
            0x00, 0x0D, 0x00, 0x04, 0x00, 0x00, 0x02, 0x58, // Lifetime
            0x80, 0x22, 0x00, 0x06, 0x44, 0x6F, 0x74, 0x75, 0x72, 0x6E, 0x00, 0x00, // Software
            0x00, 0x08, 0x00, 0x14, 0x41, 0xF8, 0x0E, 0x6B, 0x16, 0x00, 0xFA, 0xD9, 0xB5, 0x9F, 0x7D, 0x7E, 0x78, 0x4E, 0xFC, 0x94, 0xBA, 0xFF, 0xBF, 0x47, // MessageIntegrity
            0x80, 0x28, 0x00, 0x04, 0xFB, 0x21, 0xE4, 0x30 // Fingerprint
        };

        private readonly byte[] _closeConnectionSuccessResponseByteArray = new byte[] {
            0x01, 0x04, 0x00, 0x34, 0x21, 0x12, 0xA4, 0x42, 0x39, 0x50, 0x4D, 0x4B, 0x64, 0x63, 0x79, 0x30, 0x6E, 0x6C, 0x69, 0x58, // header
            0x00, 0x0D, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, // Lifetime
            0x80, 0x22, 0x00, 0x06, 0x44, 0x6F, 0x74, 0x75, 0x72, 0x6E, 0x00, 0x00, // Software
            0x00, 0x08, 0x00, 0x14, 0x0A, 0xDE, 0x3D, 0xBB, 0xA6, 0xA5, 0xC2, 0xF2, 0x28, 0xF5, 0xFE, 0x64, 0xD9, 0x8B, 0xE5, 0x66, 0xBD, 0x04, 0xA9, 0x49, // MessageIntegrity
            0x80, 0x28, 0x00, 0x04, 0xD2, 0x37, 0xAE, 0x47 // Fingerprint
        };

        private readonly byte[] _refreshErrorResponseByteArray = new byte[] {
            0x01, 0x14, 0x00, 0x14, 0x21, 0x12, 0xa4, 0x42, 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58, // header
            0x80, 0x22, 0x00, 0x06, 0x44, 0x6F, 0x74, 0x75, 0x72, 0x6E, 0x00, 0x00, // Software
            0x80, 0x28, 0x00, 0x04, 0x62, 0x94, 0x7A, 0x31 // Fingerprint
        };

        private readonly IAppSettings _appSettings = new AppSettings()
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
        public void Parse_And_Convert_To_ByteArray_RefreshRequest()
        {
            var refreshRequest = new Refresh(_magicCookie, _transactionId, _refreshRequestByteArray, _appSettings);
            byte[] convertedRefreshRequestByteArray = refreshRequest.ToBytes();
            Assert.Equal(_refreshRequestByteArray, convertedRefreshRequestByteArray);
        }

        [Fact]
        public void CreateSuccessResponse()
        {
            var refreshRequest = new Refresh(_magicCookie, _transactionId, _refreshRequestByteArray, _appSettings);
            byte[] successResponseByteArray = refreshRequest.CreateSuccessResponse();
            Assert.Equal(_refreshSuccessResponseByteArray, successResponseByteArray);
        }

        [Fact]
        public void Close_Connection_SuccessResponse()
        {
            var refreshRequest = new Refresh(_magicCookie, _transactionId, _refreshRequestByteArray, _appSettings);
            byte[] successResponseByteArray = refreshRequest.CreateSuccessResponse(0);
            Assert.Equal(_closeConnectionSuccessResponseByteArray, successResponseByteArray);
        }

        [Fact]
        public void CreateErrorResponse()
        {
            var refreshRequest = new Refresh(_magicCookie, _transactionId, _refreshRequestByteArray, _appSettings);
            byte[] errorResponseByteArray = refreshRequest.CreateErrorResponse();
            Assert.Equal(_refreshErrorResponseByteArray, errorResponseByteArray);
        }
    }
}