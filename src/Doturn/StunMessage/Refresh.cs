using System;
using System.Collections.Generic;
using Doturn.StunAttribute;

namespace Doturn.StunMessage;

public class Refresh : StunMessageBase
{
    private readonly byte[] _magicCookie;
    public byte[] TransactionId { get; }
    public List<IStunAttribute> Attributes { get; } = new();
    private readonly IAppSettings _appSettings;

    public override Type Type { get; }

    public Refresh(byte[] magicCookie, byte[] transactionId, byte[] data, IAppSettings appSettings)
    {
        Type = Type.Refresh;
        _magicCookie = magicCookie;
        TransactionId = transactionId;
        //TODO 必要なattributeが揃っているかチェックする
        Attributes.AddRange(StunAttributeParser.Parse(data));
        _appSettings = appSettings;
    }
    public Refresh(byte[] magicCookie, byte[] transactionId, List<IStunAttribute> attributes, bool isSuccess, IAppSettings appSettings)
    {
        Type = isSuccess ? Type.RefreshSuccess : Type.RefreshError;
        _magicCookie = magicCookie;
        TransactionId = transactionId;
        //TODO 必要なattributeが揃っているかチェックする
        Attributes = attributes;
        _appSettings = appSettings;
    }
    public byte[] CreateSuccessResponse()
    {
        return CreateSuccessResponse(600);
    }
    public byte[] CreateSuccessResponse(int lifetimeValue)
    {
        int messageIntegrityLength = 24;
        int fingerprintlength = 8;

        List<IStunAttribute> attributes = new();
        var lifetime = new Lifetime(lifetimeValue);
        attributes.Add(lifetime);
        var software = new Software();
        attributes.Add(software);
        var tmpRefreshSuccessResponse = new Refresh(_magicCookie, TransactionId, attributes, true, _appSettings);
        byte[] tmpRefreshSuccessResponseByteArray = tmpRefreshSuccessResponse.ToBytes();

        var tmpStunHeader = new StunHeader(Type.RefreshSuccess, (short)(tmpRefreshSuccessResponseByteArray.Length + messageIntegrityLength), TransactionId);
        byte[] tmpStunHeaderByteArray = tmpStunHeader.ToBytes();
        byte[] responseByteArray = new byte[tmpStunHeaderByteArray.Length + tmpRefreshSuccessResponseByteArray.Length + messageIntegrityLength + fingerprintlength];
        ByteArrayUtils.MergeByteArray(ref responseByteArray, tmpStunHeaderByteArray, tmpRefreshSuccessResponseByteArray);
        var messageIntegrity = new MessageIntegrity(_appSettings.Username, _appSettings.Password, _appSettings.Realm, responseByteArray[0..(responseByteArray.Length - (messageIntegrityLength + fingerprintlength))]);
        byte[] messageIntegrityByteArray = messageIntegrity.ToBytes();

        var stunHeader = new StunHeader(Type.RefreshSuccess, (short)(tmpStunHeader.MessageLength + fingerprintlength), TransactionId);
        byte[] stunHeaderByteArray = stunHeader.ToBytes();
        ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, tmpRefreshSuccessResponseByteArray, messageIntegrityByteArray);
        var fingerprint = Fingerprint.CreateFingerprint(responseByteArray[0..(responseByteArray.Length - fingerprintlength)]);
        byte[] fingerprintByteArray = fingerprint.ToBytes();
        ByteArrayUtils.MergeByteArray(ref responseByteArray, responseByteArray.Length - fingerprintByteArray.Length, fingerprintByteArray);
        return responseByteArray;
    }
    public byte[] CreateErrorResponse()
    {
        int fingerprintlength = 8;

        List<IStunAttribute> attributes = new();
        var software = new Software();
        attributes.Add(software);
        var tmpRefreshErrorResponse = new Refresh(_magicCookie, TransactionId, attributes, false, _appSettings);
        byte[] tmpRefreshErrorResponseByteArray = tmpRefreshErrorResponse.ToBytes();

        var tmpStunHeader = new StunHeader(Type.RefreshError, (short)tmpRefreshErrorResponseByteArray.Length, TransactionId);
        byte[] tmpStunHeaderByteArray = tmpStunHeader.ToBytes();
        byte[] responseByteArray = new byte[tmpStunHeaderByteArray.Length + tmpRefreshErrorResponseByteArray.Length + fingerprintlength];
        ByteArrayUtils.MergeByteArray(ref responseByteArray, tmpStunHeaderByteArray, tmpRefreshErrorResponseByteArray);

        var stunHeader = new StunHeader(Type.RefreshError, (short)(tmpStunHeader.MessageLength + fingerprintlength), TransactionId);
        byte[] stunHeaderByteArray = stunHeader.ToBytes();
        ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, tmpRefreshErrorResponseByteArray);
        var fingerprint = Fingerprint.CreateFingerprint(responseByteArray[0..(responseByteArray.Length - fingerprintlength)]);
        byte[] fingerprintByteArray = fingerprint.ToBytes();
        ByteArrayUtils.MergeByteArray(ref responseByteArray, responseByteArray.Length - fingerprintByteArray.Length, fingerprintByteArray);
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
