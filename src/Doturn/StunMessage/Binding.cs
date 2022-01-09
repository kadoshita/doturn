using System;
using System.Collections.Generic;
using System.Net;
using Doturn.StunAttribute;

namespace Doturn.StunMessage
{
    public class Binding : StunMessageBase
    {
        public readonly Type type;
        public readonly List<IStunAttribute> attributes = new List<IStunAttribute>();
        public override Type Type => this.type;

        public Binding(byte[] data)
        {
            //TODO 必要なattributeが揃っているかチェックする
            this.attributes = StunAttributeParser.Parse(data);
            this.type = Type.BINDING;
        }
        public Binding(List<IStunAttribute> attributes, bool isSuccess)
        {
            //TODO 必要なattributeが揃っているかチェックする
            this.attributes = attributes;
            this.type = isSuccess ? Type.BINDING_SUCCESS : Type.BINDING_ERROR;
        }
        public static byte[] CreateSuccessResponse(byte[] transactionId, byte[] magicCookie, IPEndPoint endPoint)
        {
            List<IStunAttribute> attributes = new List<IStunAttribute>();
            StunHeader stunHeader;
            var isXor = BitConverter.ToInt32(magicCookie) != 0;
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
            var bindingSuccessResponse = new Binding(attributes, true);
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
        public static byte[] CreateErrorResponse(byte[] transactionId)
        {
            var bindingErrorResponse = new Binding(new List<IStunAttribute>(), false);
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