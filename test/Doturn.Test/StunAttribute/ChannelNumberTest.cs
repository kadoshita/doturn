using Xunit;

namespace Doturn.StunAttribute.Test;

public class ChannelNumberTest
{
    [Fact]
    public void Convert_To_ByteArray_ChannelNumber()
    {
        var channelNumber = new ChannelNumber(new byte[] { 0x40, 0x00 }, new byte[] { 0x00, 0x00 });
        byte[] result = channelNumber.ToBytes();
        byte[] expect = new byte[] { 0x00, 0x0C, 0x00, 0x04, 0x40, 0x00, 0x00, 0x00 };
        Assert.Equal(expect, result);
    }

    [Fact]
    public void Parse_ChannelNumber()
    {
        byte[] data = new byte[] { 0x40, 0x00, 0x00, 0x00 };
        var channelNumber = ChannelNumber.Parse(data);
        Assert.Equal(new byte[] { 0x40, 0x00 }, channelNumber.Value);
        byte[] result = channelNumber.ToBytes();
        byte[] expect = new byte[] { 0x00, 0x0C, 0x00, 0x04, 0x40, 0x00, 0x00, 0x00 };
        Assert.Equal(expect, result);
    }
}
