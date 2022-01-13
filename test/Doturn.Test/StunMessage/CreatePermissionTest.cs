using Xunit;

namespace Doturn.StunMessage.Test
{
    public class CreatePermissionTest
    {
        private readonly byte[] _magicCookie = new byte[] { 0x21, 0x12, 0xA4, 0x42 };
        private readonly byte[] _transactionId = new byte[] { 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58 };
        private readonly byte[] _createPermissionRequestByteArray = new byte[] {
            0x00, 0x12, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0x5E, 0x12, 0xA4, 0x43, // XorPeerAddress 127.0.0.1:20000
            0x00, 0x06, 0x00, 0x08, 0x75, 0x73, 0x65, 0x72, 0x6E, 0x61, 0x6D, 0x65, // Username username
            0x00, 0x14, 0x00, 0x0B, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D, 0x00, // Realm example.com
            0x00, 0x15, 0x00, 0x10, 0x68, 0x6F, 0x32, 0x79, 0x64, 0x77, 0x35, 0x71, 0x65, 0x65, 0x71, 0x73, 0x67, 0x61, 0x73, 0x7A, // Nonce ho2ydw5qeeqsgasz
            0x00, 0x08, 0x00, 0x14, 0x8D, 0xA4, 0x75, 0x94, 0xAF, 0x6B, 0x72, 0xB2, 0x55, 0xD2, 0x92, 0x55, 0xF9, 0x58, 0xB5, 0x82, 0xDB, 0x87, 0x78, 0x6D, // MessageIntegrity
        };
        private readonly byte[] _createPermissionSuccessResponseByteArray = new byte[] {
            0x01, 0x08, 0x00, 0x2C, 0x21, 0x12, 0xa4, 0x42, 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58, // header
            0x80, 0x22, 0x00, 0x06, 0x44, 0x6F, 0x74, 0x75, 0x72, 0x6E, 0x00, 0x00, // Software
            0x00, 0x08, 0x00, 0x14, 0x9B, 0x00, 0x56, 0xEC, 0x1C, 0x7D, 0xFD, 0x98, 0x34, 0x79, 0x60, 0xA5, 0x9B, 0xBD, 0x67, 0xF4, 0x15, 0xFC, 0x84, 0x21, // MessageIntegrity
            0x80, 0x28, 0x00, 0x04, 0xA0, 0x07, 0xDD, 0xDC // Fingerprint
        };

        private readonly byte[] _createPermissionErrorResponseByteArray = new byte[] {
            0x01, 0x18, 0x00, 0x14, 0x21, 0x12, 0xa4, 0x42, 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58, // header
            0x80, 0x22, 0x00, 0x06, 0x44, 0x6F, 0x74, 0x75, 0x72, 0x6E, 0x00, 0x00, // Software
            0x80, 0x28, 0x00, 0x04, 0x2E, 0x34, 0x9F, 0xFD // Fingerprint
        };

        [Fact]
        public void Parse_And_Convert_To_ByteArray_createPermissionRequest()
        {
            var createPermissionRequest = new CreatePermission(_magicCookie, _transactionId, _createPermissionRequestByteArray);
            byte[] convertedcreatePermissionRequestByteArray = createPermissionRequest.ToBytes();
            _ = (StunAttribute.XorPeerAddress)createPermissionRequest.attributes[0];
            Assert.Equal(_createPermissionRequestByteArray, convertedcreatePermissionRequestByteArray);
        }

        [Fact]
        public void CreateSuccessResponse()
        {
            var createPermissionRequest = new CreatePermission(_magicCookie, _transactionId, _createPermissionRequestByteArray);
            byte[] successResponseByteArray = createPermissionRequest.CreateSuccessResponse();
            Assert.Equal(_createPermissionSuccessResponseByteArray, successResponseByteArray);
        }

        [Fact]
        public void CreateErrorResponse()
        {
            var createPermissionRequest = new CreatePermission(_magicCookie, _transactionId, _createPermissionRequestByteArray);
            byte[] errorResponseByteArray = createPermissionRequest.CreateErrorResponse();
            Assert.Equal(_createPermissionErrorResponseByteArray, errorResponseByteArray);
        }
    }
}