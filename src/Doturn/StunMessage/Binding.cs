using System;
using System.Collections.Generic;
using System.Net;
using Doturn.StunAttribute;

namespace Doturn.StunMessage
{
    public class Binding : StunMessageBase
    {
        public readonly Type type;
        private readonly byte[] _magicCookie;
        public readonly byte[] transactionId;
        public readonly List<IStunAttribute> attributes = new();
        private AppSettings _appSettings;
        public override Type Type => type;

        public Binding(byte[] magicCookie, byte[] transactionId, AppSettings appSettings)
        {
            type = Type.BINDING;
            _magicCookie = magicCookie;
            this.transactionId = transactionId;
            _appSettings = appSettings;
        }
        public Binding(byte[] magicCookie, byte[] transactionId, List<IStunAttribute> attributes, bool isSuccess, AppSettings appSettings)
        {
            type = isSuccess ? Type.BINDING_SUCCESS : Type.BINDING_ERROR;
            _magicCookie = magicCookie;
            this.transactionId = transactionId;
            //TODO 必要なattributeが揃っているかチェックする
            this.attributes = attributes;
            _appSettings = appSettings;
        }
        public byte[] CreateSuccessResponse(IPEndPoint endPoint)
        {
            List<IStunAttribute> attributes = new();
            StunHeader stunHeader;
            bool isXor = BitConverter.ToInt32(_magicCookie) != 0;
            if (isXor)
            {
                var attribute = new XorMappedAddress(endPoint);
                attributes.Add(attribute);
            }
            else
            {
                var attribute = new MappedAddress(endPoint);
                attributes.Add(attribute);
            }
            var bindingSuccessResponse = new Binding(_magicCookie, transactionId, attributes, true, _appSettings);
            byte[] bindingSuccessResponseByteArray = bindingSuccessResponse.ToBytes();
            if (isXor)
            {
                stunHeader = new StunHeader(Type.BINDING_SUCCESS, (short)bindingSuccessResponseByteArray.Length, transactionId);
            }
            else
            {
                stunHeader = new StunHeader(Type.BINDING_SUCCESS, (short)bindingSuccessResponseByteArray.Length, _magicCookie, transactionId);
            }
            byte[] stunHeaderByteArray = stunHeader.ToBytes();
            byte[] responseByteArray = new byte[stunHeaderByteArray.Length + bindingSuccessResponseByteArray.Length];
            ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, bindingSuccessResponseByteArray);
            return responseByteArray;
        }
        public byte[] CreateErrorResponse()
        {
            var bindingErrorResponse = new Binding(_magicCookie, transactionId, new List<IStunAttribute>(), false, _appSettings);
            byte[] bindingErrorResponseByteArray = bindingErrorResponse.ToBytes();
            var stunHeader = new StunHeader(Type.BINDING_ERROR, (short)bindingErrorResponseByteArray.Length, transactionId);
            byte[] stunHeaderByteArray = stunHeader.ToBytes();
            byte[] responseByteArray = new byte[stunHeaderByteArray.Length + bindingErrorResponseByteArray.Length];
            ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, bindingErrorResponseByteArray);
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