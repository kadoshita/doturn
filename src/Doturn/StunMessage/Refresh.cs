using System;
using System.Collections.Generic;
using Doturn.StunAttribute;

namespace Doturn.StunMessage
{
    public class Refresh : StunMessageBase
    {
        public readonly Type type;
        public readonly List<IStunAttribute> attributes = new List<IStunAttribute>();
        public override Type Type => this.type;

        public Refresh(byte[] data)
        {
            //TODO 必要なattributeが揃っているかチェックする
            this.attributes = StunAttributeParser.Parse(data);
            this.type = Type.REFRESH;
        }
        public Refresh(List<IStunAttribute> attributes, bool isSuccess)
        {
            //TODO 必要なattributeが揃っているかチェックする
            this.attributes = attributes;
            this.type = isSuccess ? Type.REFRESH_SUCCESS : Type.REFRESH_ERROR;
        }
        public static byte[] CreateSuccessResponse(byte[] transactionId)
        {
            var messageIntegrityLength = 24;
            var fingerprintlength = 8;

            List<IStunAttribute> attributes = new List<IStunAttribute>();
            var lifetime = new Lifetime();
            attributes.Add(lifetime);
            var software = new Software();
            attributes.Add(software);
            var tmpRefreshSuccessResponse = new Refresh(attributes, true);
            var tmpRefreshSuccessResponseByteArray = tmpRefreshSuccessResponse.ToBytes();

            var tmpStunHeader = new StunHeader(StunMessage.Type.REFRESH_SUCCESS, (short)(tmpRefreshSuccessResponseByteArray.Length + messageIntegrityLength), transactionId);
            var tmpStunHeaderByteArray = tmpStunHeader.ToBytes();
            var responseByteArray = new byte[tmpStunHeaderByteArray.Length + tmpRefreshSuccessResponseByteArray.Length + messageIntegrityLength + fingerprintlength];
            ByteArrayUtils.MergeByteArray(ref responseByteArray, tmpStunHeaderByteArray, tmpRefreshSuccessResponseByteArray);
            var messageIntegrity = new MessageIntegrity("username", "password", "example.com", responseByteArray[0..(responseByteArray.Length - (messageIntegrityLength + fingerprintlength))]);
            var messageIntegrityByteArray = messageIntegrity.ToBytes();

            var stunHeader = new StunHeader(StunMessage.Type.REFRESH_SUCCESS, (short)(tmpStunHeader.messageLength + fingerprintlength), transactionId);
            var stunHeaderByteArray = stunHeader.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, tmpRefreshSuccessResponseByteArray, messageIntegrityByteArray);
            var fingerprint = Fingerprint.CreateFingerprint(responseByteArray[0..(responseByteArray.Length - fingerprintlength)]);
            var fingerprintByteArray = fingerprint.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, responseByteArray.Length - fingerprintByteArray.Length, fingerprintByteArray);
            return responseByteArray;
        }
        public static byte[] CreateErrorResponse(byte[] transactionId)
        {
            var fingerprintlength = 8;

            List<IStunAttribute> attributes = new List<IStunAttribute>();
            var software = new Software();
            attributes.Add(software);
            var tmpRefreshErrorResponse = new Refresh(attributes, false);
            var tmpRefreshErrorResponseByteArray = tmpRefreshErrorResponse.ToBytes();

            var tmpStunHeader = new StunHeader(StunMessage.Type.REFRESH_ERROR, (short)(tmpRefreshErrorResponseByteArray.Length), transactionId);
            var tmpStunHeaderByteArray = tmpStunHeader.ToBytes();
            var responseByteArray = new byte[tmpStunHeaderByteArray.Length + tmpRefreshErrorResponseByteArray.Length + fingerprintlength];
            ByteArrayUtils.MergeByteArray(ref responseByteArray, tmpStunHeaderByteArray, tmpRefreshErrorResponseByteArray);

            var stunHeader = new StunHeader(StunMessage.Type.REFRESH_ERROR, (short)(tmpStunHeader.messageLength + fingerprintlength), transactionId);
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