using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class StunAttributeParserTest
    {
        private readonly byte[] _errorCodeByteArray = new byte[] { 0x00, 0x09, 0x00, 0x10, 0x00, 0x00, 0x04, 0x01, 0x55, 0x6E, 0x61, 0x75, 0x74, 0x68, 0x6F, 0x72, 0x69, 0x7A, 0x65, 0x64 };
        private readonly byte[] _fingerprintByteArray = new byte[] { 0x80, 0x28, 0x00, 0x04, 0x81, 0x56, 0xBA, 0xC3 };
        private readonly byte[] _lifetimeByteArray = new byte[] { 0x00, 0x0D, 0x00, 0x04, 0x00, 0x00, 0x02, 0x58 };
        private readonly byte[] _mappedAddressByteArray = new byte[] { 0x00, 0x01, 0x00, 0x08, 0x00, 0x01, 0x4E, 0x20, 0x7F, 0x00, 0x00, 0x01 };
        private readonly byte[] _messageIntegrityByteArray = new byte[] { 0x00, 0x08, 0x00, 0x14, 0x62, 0x96, 0xE4, 0x0C, 0x8F, 0x6F, 0xCD, 0x5F, 0x1C, 0x6E, 0xBC, 0x7E, 0xA0, 0x55, 0xC7, 0x85, 0xB5, 0xCF, 0xC4, 0x20 };
        private readonly byte[] _nonceByteArray = new byte[] { 0x00, 0x15, 0x00, 0x10, 0x68, 0x6F, 0x32, 0x79, 0x64, 0x77, 0x35, 0x71, 0x65, 0x65, 0x71, 0x73, 0x67, 0x61, 0x73, 0x7A };
        private readonly byte[] _realmByteArray = new byte[] { 0x00, 0x14, 0x00, 0x0B, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D, 0x00 };
        private readonly byte[] _requestedTransportByteArray = new byte[] { 0x00, 0x19, 0x00, 0x04, 0x11, 0x00, 0x00, 0x00 };
        private readonly byte[] _softwareByteArray = new byte[] { 0x80, 0x22, 0x00, 0x04, 0x4E, 0x6F, 0x6E, 0x65 };
        private readonly byte[] _usernameByteArray = new byte[] { 0x00, 0x06, 0x00, 0x08, 0x75, 0x73, 0x65, 0x72, 0x6E, 0x61, 0x6D, 0x65 };
        private readonly byte[] _xorMappedAddressByteArray = new byte[] { 0x00, 0x20, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0x5E, 0x12, 0xA4, 0x43 };
        private readonly byte[] _xorPeerAddressByteArray = new byte[] { 0x00, 0x12, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0x5E, 0x12, 0xA4, 0x43 };
        private readonly byte[] _xorRelayedAddressByteArray = new byte[] { 0x00, 0x16, 0x00, 0x08, 0x00, 0x01, 0x6F, 0x32, 0x5E, 0x12, 0xA4, 0x43 };

        [Fact]
        public void Parse_ErrorCode_StunAttribute()
        {
            System.Collections.Generic.List<IStunAttribute> attributes = StunAttributeParser.Parse(_errorCodeByteArray);
            var attribute = (ErrorCode)attributes[0];
            Assert.Equal(Type.ERROR_CODE, attribute.Type);
            Assert.Equal(0x04, attribute.errorClass);
            Assert.Equal(0x01, attribute.errorCode);
            Assert.Equal("Unauthorized", attribute.errorReasonPhrase);
        }

        [Fact]
        public void Parse_Fingerprint_StunAttribute()
        {
            System.Collections.Generic.List<IStunAttribute> attributes = StunAttributeParser.Parse(_fingerprintByteArray);
            var attribute = (Fingerprint)attributes[0];
            Assert.Equal(Type.FINGERPRINT, attribute.Type);
        }

        [Fact]
        public void Parse_Lifetime_StunAttribute()
        {
            System.Collections.Generic.List<IStunAttribute> attributes = StunAttributeParser.Parse(_lifetimeByteArray);
            var attribute = (Lifetime)attributes[0];
            Assert.Equal(Type.LIFETIME, attribute.Type);
            Assert.Equal(600, attribute.lifetime);
        }

        [Fact]
        public void Parse_MappedAddress_StunAttribute()
        {
            System.Collections.Generic.List<IStunAttribute> attributes = StunAttributeParser.Parse(_mappedAddressByteArray);
            var attribute = (MappedAddress)attributes[0];
            Assert.Equal(Type.MAPPED_ADDRESS, attribute.Type);
            Assert.Equal("127.0.0.1", attribute.endpoint.Address.ToString());
            Assert.Equal(20000, attribute.endpoint.Port);
        }

        [Fact]
        public void Parse_MessageIntegrity_StunAttribute()
        {
            System.Collections.Generic.List<IStunAttribute> attributes = StunAttributeParser.Parse(_messageIntegrityByteArray);
            var attribute = (MessageIntegrity)attributes[0];
            Assert.Equal(Type.MESSAGE_INTEGRITY, attribute.Type);
        }

        [Fact]
        public void Parse_Nonce_StunAttribute()
        {
            System.Collections.Generic.List<IStunAttribute> attributes = StunAttributeParser.Parse(_nonceByteArray);
            var attribute = (Nonce)attributes[0];
            Assert.Equal(Type.NONCE, attribute.Type);
            Assert.Equal("ho2ydw5qeeqsgasz", attribute.nonce);
        }

        [Fact]
        public void Parse_Realm_StunAttribute()
        {
            System.Collections.Generic.List<IStunAttribute> attributes = StunAttributeParser.Parse(_realmByteArray);
            var attribute = (Realm)attributes[0];
            Assert.Equal(Type.REALM, attribute.Type);
            Assert.Equal("example.com", attribute.realm);
        }

        [Fact]
        public void Parse_RequestedTransport_StunAttribute()
        {
            System.Collections.Generic.List<IStunAttribute> attributes = StunAttributeParser.Parse(_requestedTransportByteArray);
            var attribute = (RequestedTransport)attributes[0];
            Assert.Equal(Type.REQUESTED_TRANSPORT, attribute.Type);
            Assert.Equal(Transport.UDP, attribute.transport);
        }

        [Fact]
        public void Parse_Software_StunAttribute()
        {
            System.Collections.Generic.List<IStunAttribute> attributes = StunAttributeParser.Parse(_softwareByteArray);
            var attribute = (Software)attributes[0];
            Assert.Equal(Type.SOFTWARE, attribute.Type);
            Assert.Equal("None", attribute.software);
        }

        [Fact]
        public void Parse_Username_StunAttribute()
        {
            System.Collections.Generic.List<IStunAttribute> attributes = StunAttributeParser.Parse(_usernameByteArray);
            var attribute = (Username)attributes[0];
            Assert.Equal(Type.USERNAME, attribute.Type);
            Assert.Equal("username", attribute.username);
        }

        [Fact]
        public void Parse_XorMappedAddress_StunAttribute()
        {
            System.Collections.Generic.List<IStunAttribute> attributes = StunAttributeParser.Parse(_xorMappedAddressByteArray);
            var attribute = (XorMappedAddress)attributes[0];
            Assert.Equal(Type.XOR_MAPPED_ADDRESS, attribute.Type);
            Assert.Equal("94.18.164.67", attribute.endpoint.Address.ToString());
            Assert.Equal(28466, attribute.endpoint.Port);
            Assert.Equal("127.0.0.1", attribute.realEndpoint.Address.ToString());
            Assert.Equal(20000, attribute.realEndpoint.Port);
        }

        [Fact]
        public void Parse_XorPeerAddress_StunAttribute()
        {
            System.Collections.Generic.List<IStunAttribute> attributes = StunAttributeParser.Parse(_xorPeerAddressByteArray);
            var attribute = (XorPeerAddress)attributes[0];
            Assert.Equal(Type.XOR_PEER_ADDRESS, attribute.Type);
            Assert.Equal("94.18.164.67", attribute.endpoint.Address.ToString());
            Assert.Equal(28466, attribute.endpoint.Port);
            Assert.Equal("127.0.0.1", attribute.realEndpoint.Address.ToString());
            Assert.Equal(20000, attribute.realEndpoint.Port);
        }

        [Fact]
        public void Parse_XorRelayedAddress_StunAttribute()
        {
            System.Collections.Generic.List<IStunAttribute> attributes = StunAttributeParser.Parse(_xorRelayedAddressByteArray);
            var attribute = (XorRelayedAddress)attributes[0];
            Assert.Equal(Type.XOR_RELAYED_ADDRESS, attribute.Type);
            Assert.Equal("94.18.164.67", attribute.endpoint.Address.ToString());
            Assert.Equal(28466, attribute.endpoint.Port);
            Assert.Equal("127.0.0.1", attribute.realEndpoint.Address.ToString());
            Assert.Equal(20000, attribute.realEndpoint.Port);
        }

    }
}