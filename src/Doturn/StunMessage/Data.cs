using System;
using System.Collections.Generic;
using System.Net;
using Doturn.StunAttribute;

namespace Doturn.StunMessage
{
    public class Data : StunMessageBase
    {
        public readonly Type type;
        public readonly List<IStunAttribute> attributes = new();

        public override Type Type => type;
        public Data(byte[] data)
        {
            type = Type.DATA_INDICATION;
            var dataAttribute = new StunAttribute.Data(data);
            //TODO 必要なattributeが揃っているかチェックする
            attributes.Add(dataAttribute);
        }

        public byte[] CreateDataIndication(IPEndPoint peer)
        {
            int fingerprintLength = 8;

            var xorPeerAddress = new XorPeerAddress(peer);
            attributes.Add(xorPeerAddress);
            byte[] dataIndicationBytes = ToBytes();
            var random = new Random();
            byte[] transactionIdBytes = new byte[12];
            random.NextBytes(transactionIdBytes);


            var stunHeader = new StunHeader(Type.DATA_INDICATION, (short)(dataIndicationBytes.Length + fingerprintLength), transactionIdBytes);
            byte[] stunHeaderBytes = stunHeader.ToBytes();
            byte[] responseByteArray = new byte[stunHeaderBytes.Length + dataIndicationBytes.Length + fingerprintLength];
            var fingerprint = Fingerprint.CreateFingerprint(responseByteArray[0..(responseByteArray.Length - fingerprintLength)]);
            byte[] fingerprintByteArray = fingerprint.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, 0, stunHeaderBytes, dataIndicationBytes, fingerprintByteArray);

            return responseByteArray;
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
}