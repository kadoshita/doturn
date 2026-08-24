using System;
using Xunit;

namespace Doturn.StunAttribute.Test;

public class TransportTest
{
    [Theory]
    [InlineData(StunAttribute.Transport.Udp, "11")]
    [InlineData(StunAttribute.Transport.Tcp, "06")]
    public void Transport_Convert_To_ByteArray(StunAttribute.Transport transport, string byteArrayString)
    {
        byte[] byteArray = transport.ToBytes();
        Assert.Single(byteArray);
        Assert.Equal(byteArrayString, BitConverter.ToString(byteArray));
    }
}
