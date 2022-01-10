using System;
using System.Collections.Generic;
using System.Net;
using Doturn.StunAttribute;

namespace Doturn.StunMessage
{
    public class Binding : StunMessageBase
    {
        public readonly Type type;
        private readonly byte[] magicCookie;
        public readonly byte[] transactionId;
        public readonly List<IStunAttribute> attributes = new List<IStunAttribute>();
        public override Type Type => this.type;

        public Binding(byte[] magicCookie, byte[] transactionId)
        {
            this.type = Type.BINDING;
            this.magicCookie = magicCookie;
            this.transactionId = transactionId;
        }
        public Binding(byte[] magicCookie, byte[] transactionId, List<IStunAttribute> attributes, bool isSuccess)
        {
            this.type = isSuccess ? Type.BINDING_SUCCESS : Type.BINDING_ERROR;
            this.magicCookie = magicCookie;
            this.transactionId = transactionId;
            //TODO 必要なattributeが揃っているかチェックする
            this.attributes = attributes;
        }
        public byte[] CreateSuccessResponse(IPEndPoint endPoint)
        {
            List<IStunAttribute> attributes = new List<IStunAttribute>();
            StunHeader stunHeader;
            var isXor = BitConverter.ToInt32(this.magicCookie) != 0;
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
            var bindingSuccessResponse = new Binding(this.magicCookie, this.transactionId, attributes, true);
            var bindingSuccessResponseByteArray = bindingSuccessResponse.ToBytes();
            if (isXor)
            {
                stunHeader = new StunHeader(StunMessage.Type.BINDING_SUCCESS, (short)bindingSuccessResponseByteArray.Length, transactionId);
            }
            else
            {
                stunHeader = new StunHeader(StunMessage.Type.BINDING_SUCCESS, (short)bindingSuccessResponseByteArray.Length, magicCookie, transactionId);
            }
            var stunHeaderByteArray = stunHeader.ToBytes();
            var responseByteArray = new byte[stunHeaderByteArray.Length + bindingSuccessResponseByteArray.Length];
            ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, bindingSuccessResponseByteArray);
            return responseByteArray;
        }
        public byte[] CreateErrorResponse()
        {
            var bindingErrorResponse = new Binding(this.magicCookie, this.transactionId, new List<IStunAttribute>(), false);
            var bindingErrorResponseByteArray = bindingErrorResponse.ToBytes();
            var stunHeader = new StunHeader(StunMessage.Type.BINDING_ERROR, (short)bindingErrorResponseByteArray.Length, transactionId);
            var stunHeaderByteArray = stunHeader.ToBytes();
            var responseByteArray = new byte[stunHeaderByteArray.Length + bindingErrorResponseByteArray.Length];
            ByteArrayUtils.MergeByteArray(ref responseByteArray, stunHeaderByteArray, bindingErrorResponseByteArray);
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