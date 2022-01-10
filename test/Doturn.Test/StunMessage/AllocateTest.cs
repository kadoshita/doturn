using System.Net;
using Xunit;

namespace Doturn.StunMessage.Test
{
    public class AllocateTest
    {
        private readonly byte[] magicCookie = new byte[] { 0x21, 0x12, 0xA4, 0x42 };
        private readonly byte[] transactionId = new byte[] { 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58 };
        private readonly byte[] allocateRequestHeaderByteArray = new byte[]{
            0x00, 0x03, 0x00, 0x58, 0x21, 0x12, 0xa4, 0x42, 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58
        };
        private readonly byte[] allocateRequestByteArray = new byte[] {
            0x00, 0x19, 0x00, 0x04, 0x11, 0x00, 0x00, 0x00, // RequestedTransport UDP
            0x00, 0x06, 0x00, 0x08, 0x75, 0x73, 0x65, 0x72, 0x6E, 0x61, 0x6D, 0x65, // Username username
            0x00, 0x14, 0x00, 0x0B, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D, 0x00, // Realm example.com
            0x00, 0x15, 0x00, 0x10, 0x68, 0x6F, 0x32, 0x79, 0x64, 0x77, 0x35, 0x71, 0x65, 0x65, 0x71, 0x73, 0x67, 0x61, 0x73, 0x7A, // Nonce ho2ydw5qeeqsgasz
            0x00, 0x08, 0x00, 0x14, 0xA8, 0x98, 0xC7, 0xBE, 0x86, 0x43, 0x22, 0xFE, 0xBC, 0xC9, 0x37, 0x8B, 0x1C, 0x64, 0x40, 0xD2, 0x0A, 0x21, 0x2F, 0xD5, // MessageIntegrity
            0x80, 0x28, 0x00, 0x04, 0x23, 0x31, 0x31, 0x8a // Fingerprint
        };
        private readonly byte[] allocateSuccessResponseByteArray = new byte[] {
            0x01, 0x03, 0x00, 0x50, 0x21, 0x12, 0xA4, 0x42, 0x39, 0x50, 0x4D, 0x4B, 0x64, 0x63, 0x79, 0x30, 0x6E, 0x6C, 0x69, 0x58, // header
            0x00, 0x16, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0x5E, 0x12, 0xA4, 0x43, // XorRelayedAddress
            0x00, 0x20, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0x5E, 0x12, 0xA4, 0x43, // XorMappedAddress
            0x00, 0x0D, 0x00, 0x04, 0x00, 0x00, 0x02, 0x58, // Lifetime
            0x80, 0x22, 0x00, 0x06, 0x44, 0x6F, 0x74, 0x75, 0x72, 0x6E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Software
            0x08, 0x00, 0x14, 0xF3, 0x6E, 0x60, 0x06, 0x24, 0x12, 0x2F, 0x57, 0x81, 0x0C, 0x89, 0xA3, 0xB6, 0xB7, 0x3A, 0x89, 0x9A, 0x26, 0xC4, 0x2B, // MessageIntegrity
            0x80, 0x28, 0x00, 0x04, 0xCD, 0x32, 0x0F, 0x47 // Fingerprint
        };

        private readonly byte[] allocateErrorResponseByteArray = new byte[] {
            0x01, 0x13, 0x00, 0x3c, 0x21, 0x12, 0xa4, 0x42, 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58, // header
            0x00, 0x15, 0x00, 0x10, 0x37, 0x6D, 0x72, 0x34, 0x37, 0x7A, 0x64, 0x73, 0x38, 0x73, 0x33, 0x72, 0x78, 0x6B, 0x31, 0x72, // Nonce
            0x00, 0x14, 0x00, 0x0B, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D, 0x00, // Realm
            0x80, 0x22, 0x00, 0x06, 0x44, 0x6F, 0x74, 0x75, 0x72, 0x6E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Software
            0x80, 0x28, 0x00, 0x04, 0x23, 0x0C, 0xA7, 0x7C // Fingerprint
        };

        [Fact]
        public void Parse_And_Convert_To_ByteArray_AllocateRequest()
        {
            var allocateRequest = new Allocate(this.magicCookie, this.transactionId, this.allocateRequestByteArray);
            var convertedAllocateRequestByteArray = allocateRequest.ToBytes();

            var requestedTransport = (StunAttribute.RequestedTransport)allocateRequest.attributes[0];
            var username = (StunAttribute.Username)allocateRequest.attributes[1];
            var realm = (StunAttribute.Realm)allocateRequest.attributes[2];
            var nonce = (StunAttribute.Nonce)allocateRequest.attributes[3];
            var messageIntegrity = (StunAttribute.MessageIntegrity)allocateRequest.attributes[4];
            var fingerprint = (StunAttribute.Fingerprint)allocateRequest.attributes[5];
            Assert.Equal(StunAttribute.Type.REQUESTED_TRANSPORT, requestedTransport.Type);
            Assert.Equal(StunAttribute.Transport.UDP, requestedTransport.transport);
            Assert.Equal(StunAttribute.Type.USERNAME, username.Type);
            Assert.Equal("username", username.username);
            Assert.Equal(StunAttribute.Type.REALM, realm.Type);
            Assert.Equal("example.com", realm.realm);
            Assert.Equal(StunAttribute.Type.NONCE, nonce.Type);
            Assert.Equal("ho2ydw5qeeqsgasz", nonce.nonce);
            Assert.Equal(StunAttribute.Type.MESSAGE_INTEGRITY, messageIntegrity.Type);
            Assert.Equal(StunAttribute.Type.FINGERPRINT, fingerprint.Type);
            Assert.Equal(this.allocateRequestByteArray, convertedAllocateRequestByteArray);
        }

        [Fact]
        public void CreateSuccessResponse()
        {
            var allocateRequest = new Allocate(this.magicCookie, this.transactionId, this.allocateRequestByteArray);
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, 20000);
            var successResponseByteArray = allocateRequest.CreateSuccessResponse(endpoint);
            Assert.Equal(this.allocateSuccessResponseByteArray, successResponseByteArray);
        }

        [Fact]
        public void CreateErrorResponse()
        {
            var allocateRequest = new Allocate(this.magicCookie, this.transactionId, this.allocateRequestByteArray);
            var errorResponseByteArray = allocateRequest.CreateErrorResponse();
            Assert.Equal(this.allocateErrorResponseByteArray[0..24], errorResponseByteArray[0..24]); // exclude nonce
            Assert.Equal(this.allocateErrorResponseByteArray[40..76], errorResponseByteArray[40..76]); // exclude fingerprint
        }
    }
}