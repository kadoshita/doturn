using System;
using System.Collections.Generic;
using Doturn.StunAttribute;

namespace Doturn.StunMessage
{
    public class CreatePermission : StunMessageBase
    {
        public readonly Type type;
        public readonly List<IStunAttribute> attributes = new List<IStunAttribute>();
        public override Type Type => this.type;

        public CreatePermission(byte[] data)
        {
            //TODO 必要なattributeが揃っているかチェックする
            this.attributes = StunAttributeParser.Parse(data);
            this.type = Type.CREATE_PERMISSION;
        }
        public CreatePermission(List<IStunAttribute> attributes, bool isSuccess)
        {
            //TODO 必要なattributeが揃っているかチェックする
            this.attributes = attributes;
            this.type = isSuccess ? Type.CREATE_PERMISSION_SUCCESS : Type.CREATE_PERMISSION_ERROR;
        }
        public static byte[] CreateSuccessResponse(byte[] transactionId)
        {
            var messageIntegrityLength = 24;
            var fingerprintlength = 8;

            List<IStunAttribute> attributes = new List<IStunAttribute>();
            var software = new Software();
            attributes.Add(software);
            var tmpCreatePermissionSuccessResponse = new CreatePermission(attributes, true);
            var tmpCreatePermissionSuccessResponseByteArray = tmpCreatePermissionSuccessResponse.ToBytes();


            var tmpStunHeader = new StunHeader(StunMessage.Type.CREATE_PERMISSION_SUCCESS, (short)(tmpCreatePermissionSuccessResponseByteArray.Length + messageIntegrityLength), transactionId);
            var tmpStunHeaderByteArray = tmpStunHeader.ToBytes();
            var responseByteArray = new byte[tmpStunHeaderByteArray.Length + tmpCreatePermissionSuccessResponseByteArray.Length + messageIntegrityLength + fingerprintlength];
            ByteArrayUtils.MergeByteArray(ref responseByteArray, tmpStunHeaderByteArray, tmpCreatePermissionSuccessResponseByteArray);
            var messageIntegrity = new MessageIntegrity("username", "password", "example.com", responseByteArray);
            var messageIntegrityByteArray = messageIntegrity.ToBytes();

            var stunHeader = new StunHeader(StunMessage.Type.CREATE_PERMISSION_SUCCESS, (short)(tmpStunHeader.messageLength + fingerprintlength), transactionId);
            var stunHeaderByteArray = stunHeader.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, tmpCreatePermissionSuccessResponseByteArray, messageIntegrityByteArray);
            var fingerprint = Fingerprint.CreateFingerprint(responseByteArray);
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
            var tmpCreatePermissionErrorResponse = new CreatePermission(attributes, false);
            var tmpCreatePermissionErrorResponseByteArray = tmpCreatePermissionErrorResponse.ToBytes();

            var tmpStunHeader = new StunHeader(StunMessage.Type.CREATE_PERMISSION_ERROR, (short)(tmpCreatePermissionErrorResponseByteArray.Length), transactionId);
            var tmpStunHeaderByteArray = tmpStunHeader.ToBytes();
            var responseByteArray = new byte[tmpStunHeaderByteArray.Length + tmpCreatePermissionErrorResponseByteArray.Length + fingerprintlength];
            ByteArrayUtils.MergeByteArray(ref responseByteArray, tmpStunHeaderByteArray, tmpCreatePermissionErrorResponseByteArray);

            var stunHeader = new StunHeader(StunMessage.Type.CREATE_PERMISSION_ERROR, (short)(tmpStunHeader.messageLength + fingerprintlength), transactionId);
            var stunHeaderByteArray = stunHeader.ToBytes();
            ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, tmpCreatePermissionErrorResponseByteArray);
            var fingerprint = Fingerprint.CreateFingerprint(responseByteArray);
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