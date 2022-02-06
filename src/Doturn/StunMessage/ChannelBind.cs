using System;
using System.Collections.Generic;
using Doturn.StunAttribute;

namespace Doturn.StunMessage
{
    public class ChannelBind : StunMessageBase
    {
        public readonly Type type;
        private readonly byte[] _magicCookie;
        public readonly byte[] transactionId;
        public readonly List<IStunAttribute> attributes = new();
        private readonly IAppSettings _appSettings;
        public override Type Type => type;

        public ChannelBind(byte[] magicCookie, byte[] transactionId, byte[] data, IAppSettings appSettings)
        {
            type = Type.CHANNEL_BIND;
            _magicCookie = magicCookie;
            this.transactionId = transactionId;
            //TODO 必要なattributeが揃っているかチェックする
            attributes = StunAttributeParser.Parse(data);
            _appSettings = appSettings;
        }
        public ChannelBind(byte[] magicCookie, byte[] transactionId, List<IStunAttribute> attributes, bool isSuccess, IAppSettings appSettings)
        {
            type = isSuccess ? Type.CHANNEL_BIND_SUCCESS : Type.CHANNEL_BIND_ERROR;
            _magicCookie = magicCookie;
            this.transactionId = transactionId;
            //TODO 必要なattributeが揃っているかチェックする
            this.attributes = attributes;
            _appSettings = appSettings;
        }
        public byte[] CreateSuccessResponse()
        {
            var stunHeader = new StunHeader(Type.CHANNEL_BIND_SUCCESS, 0, transactionId);
            return stunHeader.ToBytes();
        }
        public byte[] CreateErrorResponse()
        {
            var stunHeader = new StunHeader(Type.CHANNEL_BIND_ERROR, 0, transactionId);
            return stunHeader.ToBytes();
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