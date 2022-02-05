using System.Collections.Generic;
using Doturn.StunAttribute;

namespace Doturn.StunMessage
{
    public class Send : StunMessageBase
    {
        public readonly Type type;
        public readonly List<IStunAttribute> attributes = new();

        public override Type Type => type;
        public Send(byte[] data)
        {
            type = Type.SEND_INDICATION;
            //TODO 必要なattributeが揃っているかチェックする
            attributes = StunAttributeParser.Parse(data);
        }

        public override byte[] ToBytes() => throw new System.NotImplementedException();

        public byte[] ToApplicationDataBytes()
        {
            var data = (StunAttribute.Data)attributes.Find(a => a.Type == StunAttribute.Type.DATA);
            return data.data;
        }
    }
}