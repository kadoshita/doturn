using System;

namespace doturn
{
    class AllocateRequest
    {
        public readonly StunHeader stunHeader;
        public readonly StunAttrType attrType;
        public readonly Int16 attrLength;
        public readonly Transport transport;
        public readonly byte[] reserved;

        public AllocateRequest(StunHeader stunHeader, byte[] body)
        {
            this.stunHeader = stunHeader;
            var attrTypeByte = body[0..2];
            var attrLengthByte = body[2..4];
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(attrTypeByte);
                Array.Reverse(attrLengthByte);
            }
            var transportByte = body[4..6];
            this.reserved = body[6..body.Length];

            this.attrType = (StunAttrType)Enum.ToObject(typeof(StunAttrType), BitConverter.ToInt16(attrTypeByte));
            this.attrLength = BitConverter.ToInt16(attrLengthByte);
            this.transport = (Transport)Enum.ToObject(typeof(Transport), BitConverter.ToInt16(transportByte));
        }

        public bool isValid()
        {
            return false;
        }
    }
}