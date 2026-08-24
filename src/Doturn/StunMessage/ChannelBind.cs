using System;
using System.Collections.Generic;
using Doturn.StunAttribute;

namespace Doturn.StunMessage;

public class ChannelBind : StunMessageBase
{
    private readonly byte[] _magicCookie;
    public byte[] TransactionId { get; }
    public List<IStunAttribute> Attributes { get; } = new();
    private readonly IAppSettings _appSettings;
    public override Type Type { get; }

    public ChannelBind(byte[] magicCookie, byte[] transactionId, byte[] data, IAppSettings appSettings)
    {
        Type = Type.ChannelBind;
        _magicCookie = magicCookie;
        TransactionId = transactionId;
        //TODO 必要なattributeが揃っているかチェックする
        Attributes.AddRange(StunAttributeParser.Parse(data));
        _appSettings = appSettings;
    }
    public ChannelBind(byte[] magicCookie, byte[] transactionId, List<IStunAttribute> attributes, bool isSuccess, IAppSettings appSettings)
    {
        Type = isSuccess ? Type.ChannelBindSuccess : Type.ChannelBindError;
        _magicCookie = magicCookie;
        TransactionId = transactionId;
        //TODO 必要なattributeが揃っているかチェックする
        Attributes = attributes;
        _appSettings = appSettings;
    }
    public byte[] CreateSuccessResponse()
    {
        var stunHeader = new StunHeader(Type.ChannelBindSuccess, 0, TransactionId);
        return stunHeader.ToBytes();
    }
    public byte[] CreateErrorResponse()
    {
        var stunHeader = new StunHeader(Type.ChannelBindError, 0, TransactionId);
        return stunHeader.ToBytes();
    }
    public override byte[] ToBytes()
    {
        int totalLength = 0;
        foreach (var a in Attributes)
        {
            totalLength += a.ToBytes().Length;
        }
        byte[] res = new byte[totalLength];
        int endPos = 0;
        foreach (var a in Attributes)
        {
            byte[] data = a.ToBytes();
            data.CopyTo(res, endPos);
            endPos += data.Length;
        }
        return res;
    }
}
