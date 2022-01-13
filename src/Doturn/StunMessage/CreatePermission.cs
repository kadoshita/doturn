using System;
using System.Collections.Generic;
using Doturn.StunAttribute;

namespace Doturn.StunMessage
{
    public class CreatePermission : StunMessageBase
    {
        public readonly Type type;
        private readonly byte[] _magicCookie;
        public readonly byte[] transactionId;
        public readonly List<IStunAttribute> attributes = new();
        private AppSettings _appSettings;
        public override Type Type => type;

        public CreatePermission(byte[] magicCookie, byte[] transactionId, byte[] data, AppSettings appSettings)
        {
            type = Type.CREATE_PERMISSION;
            _magicCookie = magicCookie;
            this.transactionId = transactionId;
            //TODO 必要なattributeが揃っているかチェックする
            attributes = StunAttributeParser.Parse(data);
            _appSettings = appSettings;
        }
        public CreatePermission(byte[] magicCookie, byte[] transactionId, List<IStunAttribute> attributes, bool isSuccess, AppSettings appSettings)
        {
            type = isSuccess ? Type.CREATE_PERMISSION_SUCCESS : Type.CREATE_PERMISSION_ERROR;
            _magicCookie = magicCookie;
            this.transactionId = transactionId;
            //TODO 必要なattributeが揃っているかチェックする
            this.attributes = attributes;
            _appSettings = appSettings;
        }
        public byte[] CreateSuccessResponse()
        {
            int messageIntegrityLength = 24;
            int fingerprintLength = 8;

            List<IStunAttribute> attributes = new();
            var software = new Software();
            attributes.Add(software);
            var tmpCreatePermissionSuccessResponse = new CreatePermission(_magicCookie, transactionId, attributes, true, _appSettings);
            byte[] tmpCreatePermissionSuccessResponseByteArray = tmpCreatePermissionSuccessResponse.ToBytes();


            var tmpStunHeader = new StunHeader(Type.CREATE_PERMISSION_SUCCESS, (short)(tmpCreatePermissionSuccessResponseByteArray.Length + messageIntegrityLength), transactionId);
            byte[] tmpStunHeaderByteArray = tmpStunHeader.ToBytes();
            byte[] responseByteArray = new byte[tmpStunHeaderByteArray.Length + tmpCreatePermissionSuccessResponseByteArray.Length + messageIntegrityLength + fingerprintLength];
            ByteArrayUtils.MergeByteArray(ref responseByteArray, tmpStunHeaderByteArray, tmpCreatePermissionSuccessResponseByteArray);
            var messageIntegrity = new MessageIntegrity(_appSettings.Username, _appSettings.Password, _appSettings.Realm, responseByteArray[0..(responseByteArray.Length - (messageIntegrityLength + fingerprintLength))]);
            byte[] messageIntegrityByteArray = messageIntegrity.ToBytes();

            var stunHeader = new StunHeader(Type.CREATE_PERMISSION_SUCCESS, (short)(tmpStunHeader.messageLength + fingerprintLength), transactionId);
            byte[] stunHeaderByteArray = stunHeader.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, tmpCreatePermissionSuccessResponseByteArray, messageIntegrityByteArray);
            var fingerprint = Fingerprint.CreateFingerprint(responseByteArray[0..(responseByteArray.Length - fingerprintLength)]);
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
            var tmpCreatePermissionErrorResponse = new CreatePermission(_magicCookie, transactionId, attributes, false, _appSettings);
            byte[] tmpCreatePermissionErrorResponseByteArray = tmpCreatePermissionErrorResponse.ToBytes();

            var tmpStunHeader = new StunHeader(Type.CREATE_PERMISSION_ERROR, (short)tmpCreatePermissionErrorResponseByteArray.Length, transactionId);
            byte[] tmpStunHeaderByteArray = tmpStunHeader.ToBytes();
            byte[] responseByteArray = new byte[tmpStunHeaderByteArray.Length + tmpCreatePermissionErrorResponseByteArray.Length + fingerprintlength];
            ByteArrayUtils.MergeByteArray(ref responseByteArray, tmpStunHeaderByteArray, tmpCreatePermissionErrorResponseByteArray);

            var stunHeader = new StunHeader(Type.CREATE_PERMISSION_ERROR, (short)(tmpStunHeader.messageLength + fingerprintlength), transactionId);
            byte[] stunHeaderByteArray = stunHeader.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, tmpCreatePermissionErrorResponseByteArray);
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