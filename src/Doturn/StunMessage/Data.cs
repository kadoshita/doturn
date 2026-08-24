using System;
using System.Collections.Generic;
using System.Net;
using Doturn.StunAttribute;

namespace Doturn.StunMessage;

public class Data : StunMessageBase
{
    public List<IStunAttribute> Attributes { get; } = new();

    public override Type Type { get; }
    public Data(byte[] data)
    {
        Type = Type.DataIndication;
        var dataAttribute = new StunAttribute.Data(data);
        //TODO 必要なattributeが揃っているかチェックする
        Attributes.Add(dataAttribute);
    }

    public byte[] CreateDataIndication(IPEndPoint peer)
    {
        const int fingerprintLength = 8;

        var xorPeerAddress = new XorPeerAddress(peer);
        Attributes.Add(xorPeerAddress);
        byte[] dataIndicationBytes = ToBytes();
        byte[] transactionIdBytes = new byte[12];
        Random.Shared.NextBytes(transactionIdBytes);

        var stunHeader = new StunHeader(Type.DataIndication, (short)(dataIndicationBytes.Length + fingerprintLength), transactionIdBytes);
        byte[] stunHeaderBytes = stunHeader.ToBytes();
        byte[] responseByteArray = new byte[stunHeaderBytes.Length + dataIndicationBytes.Length + fingerprintLength];
        var fingerprint = Fingerprint.CreateFingerprint(responseByteArray[0..(responseByteArray.Length - fingerprintLength)]);
        byte[] fingerprintByteArray = fingerprint.ToBytes();
        stunHeaderBytes.CopyTo(responseByteArray, 0);
        dataIndicationBytes.CopyTo(responseByteArray, stunHeaderBytes.Length);
        fingerprintByteArray.CopyTo(responseByteArray, stunHeaderBytes.Length + dataIndicationBytes.Length);

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
