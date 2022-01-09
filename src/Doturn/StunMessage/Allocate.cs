
using System;
using System.Collections.Generic;
using System.Net;
using Doturn.StunAttribute;

namespace Doturn.StunMessage
{
    public class Allocate : StunMessageBase
    {
        public readonly Type type;
        public readonly List<IStunAttribute> attributes = new List<IStunAttribute>();
        public override Type Type => this.type;

        public Allocate(byte[] data)
        {
            //TODO 必要なattributeが揃っているかチェックする
            this.attributes = StunAttributeParser.Parse(data);
            this.type = Type.ALLOCATE;
        }
        public Allocate(List<IStunAttribute> attributes, bool isSuccess)
        {
            this.attributes = attributes;
            this.type = isSuccess ? Type.ALLOCATE_SUCCESS : Type.ALLOCATE_ERROR;
        }
        public static byte[] CreateSuccessResponse(byte[] transactionId, IPEndPoint endPoint)
        {
            var messageIntegrityLength = 24;
            var fingerprintlength = 8;

            List<IStunAttribute> attributes = new List<IStunAttribute>();
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
            var tmpAllocateSuccessResponse = new Allocate(attributes, true);
            var tmpAllocateSuccessResponseByteArray = tmpAllocateSuccessResponse.ToBytes();

            var tmpStunHeader = new StunHeader(StunMessage.Type.ALLOCATE_SUCCESS, (short)(tmpAllocateSuccessResponseByteArray.Length + messageIntegrityLength), transactionId);
            var tmpStunHeaderByteArray = tmpStunHeader.ToBytes();
            var responseByteArray = new byte[tmpStunHeaderByteArray.Length + tmpAllocateSuccessResponseByteArray.Length + messageIntegrityLength + fingerprintlength];
            ByteArrayUtils.MergeByteArray(ref responseByteArray, tmpStunHeaderByteArray, tmpAllocateSuccessResponseByteArray);
            var messageIntegrity = new MessageIntegrity("username", "password", "example.com", responseByteArray[0..(responseByteArray.Length - (messageIntegrityLength + fingerprintlength))]);
            var messageIntegrityByteArray = messageIntegrity.ToBytes();

            var stunHeader = new StunHeader(StunMessage.Type.ALLOCATE_SUCCESS, (short)(tmpStunHeader.messageLength + fingerprintlength), transactionId);
            var stunHeaderByteArray = stunHeader.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, tmpAllocateSuccessResponseByteArray, messageIntegrityByteArray);
            var fingerprint = Fingerprint.CreateFingerprint(responseByteArray[0..(responseByteArray.Length - fingerprintlength)]);
            var fingerprintByteArray = fingerprint.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, responseByteArray.Length - fingerprintByteArray.Length, fingerprintByteArray);
            return responseByteArray;
        }
        public static byte[] CreateErrorResponse(byte[] transactionId)
        {
            var fingerprintlength = 8;

            List<IStunAttribute> attributes = new List<IStunAttribute>();
            var nonce = new Nonce();
            attributes.Add(nonce);
            var realm = new Realm();
            attributes.Add(realm);
            var software = new Software();
            attributes.Add(software);
            var tmpAllocateSuccessResponse = new Allocate(attributes, true);
            var tmpAllocateSuccessResponseByteArray = tmpAllocateSuccessResponse.ToBytes();

            var tmpStunHeader = new StunHeader(StunMessage.Type.ALLOCATE_ERROR, (short)(tmpAllocateSuccessResponseByteArray.Length), transactionId);
            var tmpStunHeaderByteArray = tmpStunHeader.ToBytes();
            var responseByteArray = new byte[tmpStunHeaderByteArray.Length + tmpAllocateSuccessResponseByteArray.Length + fingerprintlength];
            ByteArrayUtils.MergeByteArray(ref responseByteArray, tmpStunHeaderByteArray, tmpAllocateSuccessResponseByteArray);

            var stunHeader = new StunHeader(StunMessage.Type.ALLOCATE_ERROR, (short)(tmpStunHeader.messageLength + fingerprintlength), transactionId);
            var stunHeaderByteArray = stunHeader.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, tmpAllocateSuccessResponseByteArray);
            var fingerprint = Fingerprint.CreateFingerprint(responseByteArray[0..(responseByteArray.Length - fingerprintlength)]);
            var fingerprintByteArray = fingerprint.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, responseByteArray.Length - fingerprintByteArray.Length, fingerprintByteArray);
            return responseByteArray;
        }
        public override byte[] ToBytes()
        {
            var res = new byte[0];
            var endPos = 0;
            this.attributes.ForEach(a =>
            {
                var data = a.ToBytes();
                Array.Resize(ref res, res.Length + data.Length);
                ByteArrayUtils.MergeByteArray(ref res, endPos, data);
                endPos += data.Length;
            });

            return res;
        }
    }
}