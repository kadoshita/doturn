using System;
using System.Collections.Generic;
using Doturn.StunAttribute;

namespace Doturn.StunMessage;

public class ChannelBind : StunMessageBase
{
    private readonly byte[] _magicCookie;
    public readonly byte[] transactionId;
    public readonly List<IStunAttribute> attributes = new();
    private readonly IAppSettings _appSettings;
    public override Type Type { get; }

    public ChannelBind(byte[] magicCookie, byte[] transactionId, byte[] data, IAppSettings appSettings)
    {
        Type = Type.ChannelBind;
        _magicCookie = magicCookie;
        this.transactionId = transactionId;
        //TODO 必要なattributeが揃っているかチェックする
        attributes = StunAttributeParser.Parse(data);
        _appSettings = appSettings;
    }
    public ChannelBind(byte[] magicCookie, byte[] transactionId, List<IStunAttribute> attributes, bool isSuccess, IAppSettings appSettings)
    {
        Type = isSuccess ? Type.ChannelBindSuccess : Type.ChannelBindError;
        _magicCookie = magicCookie;
        this.transactionId = transactionId;
        //TODO 必要なattributeが揃っているかチェックする
        this.attributes = attributes;
        _appSettings = appSettings;
    }
    public byte[] CreateSuccessResponse()
    {
        var stunHeader = new StunHeader(Type.ChannelBindSuccess, 0, transactionId);
        return stunHeader.ToBytes();
    }
    public byte[] CreateErrorResponse()
    {
        var stunHeader = new StunHeader(Type.ChannelBindError, 0, transactionId);
        return stunHeader.ToBytes();
    }
    public override byte[] ToBytes()
    {
        byte[] res = Array.Empty<byte>();
        int endPos = 0;
        attributes.ForEach(a =>
        {
            byte[] data = a.ToBytes();
            Array.Resize(ref res, res.Length + data.Length);
            ByteArrayUtils.MergeByteArray(ref res, endPos, data);
            endPos += data.Length;
        });

        return res;
    }
}
