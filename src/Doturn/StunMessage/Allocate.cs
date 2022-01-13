using System;
using System.Collections.Generic;
using System.Net;
using Doturn.StunAttribute;

namespace Doturn.StunMessage
{
    public class Allocate : StunMessageBase
    {
        public readonly Type type;
        private readonly byte[] _magicCookie;
        public readonly byte[] transactionId;
        public readonly List<IStunAttribute> attributes = new();
        public override Type Type => type;

        public Allocate(byte[] magicCookie, byte[] transactionId, byte[] data)
        {
            type = Type.ALLOCATE;
            _magicCookie = magicCookie;
            this.transactionId = transactionId;
            //TODO 必要なattributeが揃っているかチェックする
            attributes = StunAttributeParser.Parse(data);
        }
        public Allocate(byte[] magicCookie, byte[] transactionId, List<IStunAttribute> attributes, bool isSuccess)
        {
            type = isSuccess ? Type.ALLOCATE_SUCCESS : Type.ALLOCATE_ERROR;
            _magicCookie = magicCookie;
            this.transactionId = transactionId;
            //TODO 必要なattributeが揃っているかチェックする
            this.attributes = attributes;
        }
        public byte[] CreateSuccessResponse(IPEndPoint endPoint)
        {
            int messageIntegrityLength = 24;
            int fingerprintlength = 8;

            List<IStunAttribute> attributes = new();
            //TODO external ip addressを設定から読み込む
            var ipAddress = IPAddress.Parse("127.0.0.1");
            var xorRelayedAddress = new XorRelayedAddress(ipAddress, 20000);
            attributes.Add(xorRelayedAddress);
            //TODO ここのip addressとportはrequestしてきたclientのもの
            var xorMappedAddress = new XorMappedAddress(endPoint);
            attributes.Add(xorMappedAddress);
            var lifetime = new Lifetime();
            attributes.Add(lifetime);
            var software = new Software();
            attributes.Add(software);
            var tmpAllocateSuccessResponse = new Allocate(_magicCookie, transactionId, attributes, true);
            byte[] tmpAllocateSuccessResponseByteArray = tmpAllocateSuccessResponse.ToBytes();

            var tmpStunHeader = new StunHeader(StunMessage.Type.ALLOCATE_SUCCESS, (short)(tmpAllocateSuccessResponseByteArray.Length + messageIntegrityLength), transactionId);
            byte[] tmpStunHeaderByteArray = tmpStunHeader.ToBytes();
            byte[] responseByteArray = new byte[tmpStunHeaderByteArray.Length + tmpAllocateSuccessResponseByteArray.Length + messageIntegrityLength + fingerprintlength];
            ByteArrayUtils.MergeByteArray(ref responseByteArray, tmpStunHeaderByteArray, tmpAllocateSuccessResponseByteArray);
            var messageIntegrity = new MessageIntegrity("username", "password", "example.com", responseByteArray[0..(responseByteArray.Length - (messageIntegrityLength + fingerprintlength))]);
            byte[] messageIntegrityByteArray = messageIntegrity.ToBytes();

            var stunHeader = new StunHeader(StunMessage.Type.ALLOCATE_SUCCESS, (short)(tmpStunHeader.messageLength + fingerprintlength), transactionId);
            byte[] stunHeaderByteArray = stunHeader.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, tmpAllocateSuccessResponseByteArray, messageIntegrityByteArray);
            var fingerprint = Fingerprint.CreateFingerprint(responseByteArray[0..(responseByteArray.Length - fingerprintlength)]);
            byte[] fingerprintByteArray = fingerprint.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, responseByteArray.Length - fingerprintByteArray.Length, fingerprintByteArray);
            return responseByteArray;
        }
        public byte[] CreateErrorResponse()
        {
            int fingerprintlength = 8;

            List<IStunAttribute> attributes = new();
            var nonce = new Nonce();
            attributes.Add(nonce);
            var realm = new Realm();
            attributes.Add(realm);
            var software = new Software();
            attributes.Add(software);
            var tmpAllocateSuccessResponse = new Allocate(_magicCookie, transactionId, attributes, true);
            byte[] tmpAllocateSuccessResponseByteArray = tmpAllocateSuccessResponse.ToBytes();

            var tmpStunHeader = new StunHeader(StunMessage.Type.ALLOCATE_ERROR, (short)(tmpAllocateSuccessResponseByteArray.Length), transactionId);
            byte[] tmpStunHeaderByteArray = tmpStunHeader.ToBytes();
            byte[] responseByteArray = new byte[tmpStunHeaderByteArray.Length + tmpAllocateSuccessResponseByteArray.Length + fingerprintlength];
            ByteArrayUtils.MergeByteArray(ref responseByteArray, tmpStunHeaderByteArray, tmpAllocateSuccessResponseByteArray);

            var stunHeader = new StunHeader(StunMessage.Type.ALLOCATE_ERROR, (short)(tmpStunHeader.messageLength + fingerprintlength), transactionId);
            byte[] stunHeaderByteArray = stunHeader.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, tmpAllocateSuccessResponseByteArray);
            var fingerprint = Fingerprint.CreateFingerprint(responseByteArray[0..(responseByteArray.Length - fingerprintlength)]);
            byte[] fingerprintByteArray = fingerprint.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, responseByteArray.Length - fingerprintByteArray.Length, fingerprintByteArray);
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