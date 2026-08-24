using System.Collections.Generic;
using Doturn.StunAttribute;
using Xunit;

namespace Doturn.StunMessage.Test;

public class ChannelBindTest
{
    private readonly byte[] _magicCookie = new byte[] { 0x21, 0x12, 0xA4, 0x42 };
    private readonly byte[] _transactionId = new byte[] { 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58 };

    // ChannelNumber attribute TLV: type 0x000C, length 4, value 0x40000000
    private readonly byte[] _channelBindRequestByteArray = new byte[] {
        0x00, 0x0C, 0x00, 0x04, 0x40, 0x00, 0x00, 0x00,
    };

    private readonly byte[] _channelBindSuccessResponseByteArray = new byte[] {
        0x01, 0x09, 0x00, 0x00, 0x21, 0x12, 0xa4, 0x42, 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58, // header
    };

    private readonly byte[] _channelBindErrorResponseByteArray = new byte[] {
        0x01, 0x19, 0x00, 0x00, 0x21, 0x12, 0xa4, 0x42, 0x39, 0x50, 0x4d, 0x4b, 0x64, 0x63, 0x79, 0x30, 0x6e, 0x6c, 0x69, 0x58, // header
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
    public void Parse_And_Convert_To_ByteArray_ChannelBindRequest()
    {
        var channelBind = new ChannelBind(_magicCookie, _transactionId, _channelBindRequestByteArray, _appSettings);
        byte[] convertedChannelBindRequestByteArray = channelBind.ToBytes();
        Assert.Equal(_channelBindRequestByteArray, convertedChannelBindRequestByteArray);
    }

    [Fact]
    public void CreateSuccessResponse()
    {
        var channelBind = new ChannelBind(_magicCookie, _transactionId, new List<IStunAttribute>(), true, _appSettings);
        byte[] successResponseByteArray = channelBind.CreateSuccessResponse();
        Assert.Equal(_channelBindSuccessResponseByteArray, successResponseByteArray);
    }

    [Fact]
    public void CreateErrorResponse()
    {
        var channelBind = new ChannelBind(_magicCookie, _transactionId, new List<IStunAttribute>(), false, _appSettings);
        byte[] errorResponseByteArray = channelBind.CreateErrorResponse();
        Assert.Equal(_channelBindErrorResponseByteArray, errorResponseByteArray);
    }
}
