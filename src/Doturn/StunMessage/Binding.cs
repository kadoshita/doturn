using System;
using System.Collections.Generic;
using System.Net;
using Doturn.StunAttribute;

namespace Doturn.StunMessage;

public class Binding : StunMessageBase
{
    private readonly byte[] _magicCookie;
    public byte[] TransactionId { get; }
    public List<IStunAttribute> Attributes { get; } = new();
    private readonly IAppSettings _appSettings;
    public override Type Type { get; }

    public Binding(byte[] magicCookie, byte[] transactionId, IAppSettings appSettings)
    {
        Type = Type.Binding;
        _magicCookie = magicCookie;
        TransactionId = transactionId;
        _appSettings = appSettings;
    }
    public Binding(byte[] magicCookie, byte[] transactionId, List<IStunAttribute> attributes, bool isSuccess, IAppSettings appSettings)
    {
        Type = isSuccess ? Type.BindingSuccess : Type.BindingError;
        _magicCookie = magicCookie;
        TransactionId = transactionId;
        //TODO 必要なattributeが揃っているかチェックする
        Attributes = attributes;
        _appSettings = appSettings;
    }
    public byte[] CreateSuccessResponse(IPEndPoint endPoint)
    {
        List<IStunAttribute> attributes = new();
        StunHeader stunHeader;
        bool isXor = BitConverter.ToInt32(_magicCookie) != 0;
        if (isXor)
        {
            var attribute = new XorMappedAddress(endPoint);
            attributes.Add(attribute);
        }
        else
        {
            var attribute = new MappedAddress(endPoint);
            attributes.Add(attribute);
        }
        var bindingSuccessResponse = new Binding(_magicCookie, TransactionId, attributes, true, _appSettings);
        byte[] bindingSuccessResponseByteArray = bindingSuccessResponse.ToBytes();
        if (isXor)
        {
            stunHeader = new StunHeader(Type.BindingSuccess, (short)bindingSuccessResponseByteArray.Length, TransactionId);
        }
        else
        {
            stunHeader = new StunHeader(Type.BindingSuccess, (short)bindingSuccessResponseByteArray.Length, _magicCookie, TransactionId);
        }
        byte[] stunHeaderByteArray = stunHeader.ToBytes();
        byte[] responseByteArray = new byte[stunHeaderByteArray.Length + bindingSuccessResponseByteArray.Length];
        ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, bindingSuccessResponseByteArray);
        return responseByteArray;
    }
    public byte[] CreateErrorResponse()
    {
        var bindingErrorResponse = new Binding(_magicCookie, TransactionId, new List<IStunAttribute>(), false, _appSettings);
        byte[] bindingErrorResponseByteArray = bindingErrorResponse.ToBytes();
        var stunHeader = new StunHeader(Type.BindingError, (short)bindingErrorResponseByteArray.Length, TransactionId);
        byte[] stunHeaderByteArray = stunHeader.ToBytes();
        byte[] responseByteArray = new byte[stunHeaderByteArray.Length + bindingErrorResponseByteArray.Length];
        ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, bindingErrorResponseByteArray);
        return responseByteArray;
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
