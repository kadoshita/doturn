using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class TypeTest
    {
        [Theory]
        [InlineData(StunAttribute.Type.MAPPED_ADDRESS, "00-01")]
        [InlineData(StunAttribute.Type.USERNAME, "00-06")]
        [InlineData(StunAttribute.Type.MESSAGE_INTEGRITY, "00-08")]
        [InlineData(StunAttribute.Type.ERROR_CODE, "00-09")]
        [InlineData(StunAttribute.Type.LIFETIME, "00-0D")]
        [InlineData(StunAttribute.Type.XOR_PEER_ADDRESS, "00-12")]
        [InlineData(StunAttribute.Type.REALM, "00-14")]
        [InlineData(StunAttribute.Type.NONCE, "00-15")]
        [InlineData(StunAttribute.Type.XOR_RELAYED_ADDRESS, "00-16")]
        [InlineData(StunAttribute.Type.REQUESTED_TRANSPORT, "00-19")]
        [InlineData(StunAttribute.Type.XOR_MAPPED_ADDRESS, "00-20")]
        [InlineData(StunAttribute.Type.SOFTWARE, "80-22")]
        [InlineData(StunAttribute.Type.ALTERNATE_SERVER, "80-23")]
        [InlineData(StunAttribute.Type.FINGERPRINT, "80-28")]
        public void StunAttributeType_Convert_To_ByteArray(StunAttribute.Type type, string byteArrayString)
        {
            var byteArray = type.ToBytes();
            Assert.Equal(2, byteArray.Length);
            Assert.Equal(byteArrayString, BitConverter.ToString(byteArray));
        }
    }
}