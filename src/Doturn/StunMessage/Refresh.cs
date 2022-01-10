using System;
using System.Collections.Generic;
using Doturn.StunAttribute;

namespace Doturn.StunMessage
{
    public class Refresh : StunMessageBase
    {
        public readonly Type type;
        private readonly byte[] magicCookie;
        public readonly byte[] transactionId;
        public readonly List<IStunAttribute> attributes = new List<IStunAttribute>();

        public override Type Type => this.type;

        public Refresh(byte[] magicCookie, byte[] transactionId, byte[] data)
        {
            this.type = Type.REFRESH;
            this.magicCookie = magicCookie;
            this.transactionId = transactionId;
            //TODO 必要なattributeが揃っているかチェックする
            this.attributes = StunAttributeParser.Parse(data);
        }
        public Refresh(byte[] magicCookie, byte[] transactionId, List<IStunAttribute> attributes, bool isSuccess)
        {
            this.type = isSuccess ? Type.REFRESH_SUCCESS : Type.REFRESH_ERROR;
            this.magicCookie = magicCookie;
            this.transactionId = transactionId;
            //TODO 必要なattributeが揃っているかチェックする
            this.attributes = attributes;
        }
        public byte[] CreateSuccessResponse()
        {
            var messageIntegrityLength = 24;
            var fingerprintlength = 8;

            List<IStunAttribute> attributes = new List<IStunAttribute>();
            var lifetime = new Lifetime();
            attributes.Add(lifetime);
            var software = new Software();
            attributes.Add(software);
            var tmpRefreshSuccessResponse = new Refresh(this.magicCookie, this.transactionId, attributes, true);
            var tmpRefreshSuccessResponseByteArray = tmpRefreshSuccessResponse.ToBytes();

            var tmpStunHeader = new StunHeader(StunMessage.Type.REFRESH_SUCCESS, (short)(tmpRefreshSuccessResponseByteArray.Length + messageIntegrityLength), this.transactionId);
            var tmpStunHeaderByteArray = tmpStunHeader.ToBytes();
            var responseByteArray = new byte[tmpStunHeaderByteArray.Length + tmpRefreshSuccessResponseByteArray.Length + messageIntegrityLength + fingerprintlength];
            ByteArrayUtils.MergeByteArray(ref responseByteArray, tmpStunHeaderByteArray, tmpRefreshSuccessResponseByteArray);
            var messageIntegrity = new MessageIntegrity("username", "password", "example.com", responseByteArray[0..(responseByteArray.Length - (messageIntegrityLength + fingerprintlength))]);
            var messageIntegrityByteArray = messageIntegrity.ToBytes();

            var stunHeader = new StunHeader(StunMessage.Type.REFRESH_SUCCESS, (short)(tmpStunHeader.messageLength + fingerprintlength), this.transactionId);
            var stunHeaderByteArray = stunHeader.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, tmpRefreshSuccessResponseByteArray, messageIntegrityByteArray);
            var fingerprint = Fingerprint.CreateFingerprint(responseByteArray[0..(responseByteArray.Length - fingerprintlength)]);
            var fingerprintByteArray = fingerprint.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, responseByteArray.Length - fingerprintByteArray.Length, fingerprintByteArray);
            return responseByteArray;
        }
        public byte[] CreateErrorResponse()
        {
            var fingerprintlength = 8;

            List<IStunAttribute> attributes = new List<IStunAttribute>();
            var software = new Software();
            attributes.Add(software);
            var tmpRefreshErrorResponse = new Refresh(this.magicCookie, this.transactionId, attributes, false);
            var tmpRefreshErrorResponseByteArray = tmpRefreshErrorResponse.ToBytes();

            var tmpStunHeader = new StunHeader(StunMessage.Type.REFRESH_ERROR, (short)(tmpRefreshErrorResponseByteArray.Length), this.transactionId);
            var tmpStunHeaderByteArray = tmpStunHeader.ToBytes();
            var responseByteArray = new byte[tmpStunHeaderByteArray.Length + tmpRefreshErrorResponseByteArray.Length + fingerprintlength];
            ByteArrayUtils.MergeByteArray(ref responseByteArray, tmpStunHeaderByteArray, tmpRefreshErrorResponseByteArray);

            var stunHeader = new StunHeader(StunMessage.Type.REFRESH_ERROR, (short)(tmpStunHeader.messageLength + fingerprintlength), this.transactionId);
            var stunHeaderByteArray = stunHeader.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, tmpRefreshErrorResponseByteArray);
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