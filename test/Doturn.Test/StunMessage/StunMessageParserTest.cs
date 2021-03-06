using Xunit;

namespace Doturn.StunMessage.Test
{
    public class StunMessageParserTest
    {
        private readonly byte[] _bindingRequestBytes = new byte[] {
            0x00, 0x01, 0x00, 0x00, 0x21, 0x12, 0xa4, 0x42, 0x54, 0x45, 0x53, 0x54, 0x54, 0x45, 0x53, 0x54, 0x54, 0x45, 0x53, 0x54 // header
        };
        private readonly byte[] _allocateRequestBytes = new byte[] {
            0x00, 0x03, 0x00, 0x28, 0x21, 0x12, 0xa4, 0x42, 0x7a, 0x1b, 0x4f, 0x2f, 0x6b, 0x2f, 0x59, 0x37, 0xa8, 0x85, 0xec, 0x0c, // header
            0x00, 0x19, 0x00, 0x04, 0x11, 0x00, 0x00, 0x00, // requested transport
            0x00, 0x0d, 0x00, 0x04, 0x00, 0x00, 0x03, 0x09, // lifetime
            0x00, 0x18, 0x00, 0x01, 0x80, 0x00, 0x00, 0x00, // even port
            0x00, 0x17, 0x00, 0x04, 0x01, 0x00, 0x00, 0x00, // requested address family
            0x80, 0x28, 0x00, 0x04, 0xe4, 0xa8, 0xf4, 0xe1 // fingerprint
        };
        private readonly byte[] _createPermissionRequestBytes = new byte[] {
            0x00, 0x08, 0x00, 0x54, 0x21, 0x12, 0xa4, 0x42, 0x7a, 0x1b, 0x4f, 0x2f, 0x6b, 0x2f, 0x59, 0x37, 0xa8, 0x85, 0xec, 0x0c, // header
            0x00, 0x12, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0x5E, 0x12, 0xA4, 0x43, // XorPeerAddress 127.0.0.1:20000
            0x00, 0x06, 0x00, 0x08, 0x75, 0x73, 0x65, 0x72, 0x6E, 0x61, 0x6D, 0x65, // Username username
            0x00, 0x14, 0x00, 0x0B, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D, 0x00, // Realm example.com
            0x00, 0x15, 0x00, 0x10, 0x68, 0x6F, 0x32, 0x79, 0x64, 0x77, 0x35, 0x71, 0x65, 0x65, 0x71, 0x73, 0x67, 0x61, 0x73, 0x7A, // Nonce ho2ydw5qeeqsgasz
            0x00, 0x08, 0x00, 0x14, 0x8D, 0xA4, 0x75, 0x94, 0xAF, 0x6B, 0x72, 0xB2, 0x55, 0xD2, 0x92, 0x55, 0xF9, 0x58, 0xB5, 0x82, 0xDB, 0x87, 0x78, 0x6D // MessageIntegrity
        };
        private readonly byte[] _refreshRequestBytes = new byte[] {
            0x00, 0x04, 0x00, 0x10, 0x21, 0x12, 0xa4, 0x42, 0x07, 0x78, 0x37, 0x3f, 0x59, 0x09, 0x69, 0x44, 0x49, 0x6e, 0x2d, 0x51, // header
            0x00, 0x0d, 0x00, 0x04, 0x00, 0x00, 0x03, 0x09, // lifetime
            0x80, 0x28, 0x00, 0x04, 0x85, 0xa8, 0x5b, 0x26  // fingerprint
        };
        private readonly byte[] _unknownRequestBytes = new byte[] {
            0xFF, 0xFF, 0x00, 0x00, 0x21, 0x12, 0xa4, 0x42, 0x54, 0x45, 0x53, 0x54, 0x54, 0x45, 0x53, 0x54, 0x54, 0x45, 0x53, 0x54 // header
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
        public void Parse_Binding_Request()
        {
            IStunMessage result = StunMessageParser.Parse(_bindingRequestBytes, _appSettings);
            Assert.Equal(Type.BINDING, result.Type);
        }

        [Fact]
        public void Parse_Allocate_Request()
        {
            IStunMessage result = StunMessageParser.Parse(_allocateRequestBytes, _appSettings);
            Assert.Equal(Type.ALLOCATE, result.Type);
        }

        [Fact]
        public void Parse_CreatePermission_Request()
        {
            IStunMessage result = StunMessageParser.Parse(_createPermissionRequestBytes, _appSettings);
            Assert.Equal(Type.CREATE_PERMISSION, result.Type);
        }

        [Fact]
        public void Parse_Refresh_Request()
        {
            IStunMessage result = StunMessageParser.Parse(_refreshRequestBytes, _appSettings);
            Assert.Equal(Type.REFRESH, result.Type);
        }

        [Fact]
        public void Throw_Exception_If_Unknown_Request()
        {
            Assert.Throws<StunMessageParseException>(() => StunMessageParser.Parse(_unknownRequestBytes, _appSettings));
        }
    }
}