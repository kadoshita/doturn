using System;
using Xunit;

namespace Doturn.StunAttribute.Test;

public class TypeTest
{
    [Theory]
    [InlineData(StunAttribute.Type.MappedAddress, "00-01")]
    [InlineData(StunAttribute.Type.Username, "00-06")]
    [InlineData(StunAttribute.Type.MessageIntegrity, "00-08")]
    [InlineData(StunAttribute.Type.ErrorCode, "00-09")]
    [InlineData(StunAttribute.Type.Lifetime, "00-0D")]
    [InlineData(StunAttribute.Type.XorPeerAddress, "00-12")]
    [InlineData(StunAttribute.Type.Realm, "00-14")]
    [InlineData(StunAttribute.Type.Nonce, "00-15")]
    [InlineData(StunAttribute.Type.XorRelayedAddress, "00-16")]
    [InlineData(StunAttribute.Type.RequestedTransport, "00-19")]
    [InlineData(StunAttribute.Type.XorMappedAddress, "00-20")]
    [InlineData(StunAttribute.Type.Software, "80-22")]
    [InlineData(StunAttribute.Type.AlternateServer, "80-23")]
    [InlineData(StunAttribute.Type.Fingerprint, "80-28")]
    public void StunAttributeType_Convert_To_ByteArray(StunAttribute.Type type, string byteArrayString)
    {
        byte[] byteArray = type.ToBytes();
        Assert.Equal(2, byteArray.Length);
        Assert.Equal(byteArrayString, BitConverter.ToString(byteArray));
    }
}
