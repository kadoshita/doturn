using System;
using System.Collections.Generic;
using Doturn.StunAttribute;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Doturn.StunMessage
{
    public class Refresh : StunMessageBase
    {
        public readonly Type type;
        private readonly byte[] _magicCookie;
        public readonly byte[] transactionId;
        public readonly List<IStunAttribute> attributes = new();
        private AppSettings _appSettings;

        public override Type Type => type;

        public Refresh(byte[] magicCookie, byte[] transactionId, byte[] data, AppSettings appSettings)
        {
            type = Type.REFRESH;
            _magicCookie = magicCookie;
            this.transactionId = transactionId;
            //TODO 必要なattributeが揃っているかチェックする
            attributes = StunAttributeParser.Parse(data);
            _appSettings = appSettings;
        }
        public Refresh(byte[] magicCookie, byte[] transactionId, List<IStunAttribute> attributes, bool isSuccess, AppSettings appSettings)
        {
            type = isSuccess ? Type.REFRESH_SUCCESS : Type.REFRESH_ERROR;
            _magicCookie = magicCookie;
            this.transactionId = transactionId;
            //TODO 必要なattributeが揃っているかチェックする
            this.attributes = attributes;
            _appSettings = appSettings;
        }
        public byte[] CreateSuccessResponse()
        {
            int messageIntegrityLength = 24;
            int fingerprintlength = 8;

            List<IStunAttribute> attributes = new();
            var lifetime = new Lifetime();
            attributes.Add(lifetime);
            var software = new Software();
            attributes.Add(software);
            var tmpRefreshSuccessResponse = new Refresh(_magicCookie, transactionId, attributes, true, _appSettings);
            byte[] tmpRefreshSuccessResponseByteArray = tmpRefreshSuccessResponse.ToBytes();

            var tmpStunHeader = new StunHeader(Type.REFRESH_SUCCESS, (short)(tmpRefreshSuccessResponseByteArray.Length + messageIntegrityLength), transactionId);
            byte[] tmpStunHeaderByteArray = tmpStunHeader.ToBytes();
            byte[] responseByteArray = new byte[tmpStunHeaderByteArray.Length + tmpRefreshSuccessResponseByteArray.Length + messageIntegrityLength + fingerprintlength];
            ByteArrayUtils.MergeByteArray(ref responseByteArray, tmpStunHeaderByteArray, tmpRefreshSuccessResponseByteArray);
            var messageIntegrity = new MessageIntegrity(_appSettings.Username, _appSettings.Password, _appSettings.Realm, responseByteArray[0..(responseByteArray.Length - (messageIntegrityLength + fingerprintlength))]);
            byte[] messageIntegrityByteArray = messageIntegrity.ToBytes();

            var stunHeader = new StunHeader(Type.REFRESH_SUCCESS, (short)(tmpStunHeader.messageLength + fingerprintlength), transactionId);
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
            var tmpRefreshErrorResponse = new Refresh(_magicCookie, transactionId, attributes, false, _appSettings);
            byte[] tmpRefreshErrorResponseByteArray = tmpRefreshErrorResponse.ToBytes();

            var tmpStunHeader = new StunHeader(Type.REFRESH_ERROR, (short)tmpRefreshErrorResponseByteArray.Length, transactionId);
            byte[] tmpStunHeaderByteArray = tmpStunHeader.ToBytes();
            byte[] responseByteArray = new byte[tmpStunHeaderByteArray.Length + tmpRefreshErrorResponseByteArray.Length + fingerprintlength];
            ByteArrayUtils.MergeByteArray(ref responseByteArray, tmpStunHeaderByteArray, tmpRefreshErrorResponseByteArray);

            var stunHeader = new StunHeader(Type.REFRESH_ERROR, (short)(tmpStunHeader.messageLength + fingerprintlength), transactionId);
            byte[] stunHeaderByteArray = stunHeader.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, tmpRefreshErrorResponseByteArray);
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