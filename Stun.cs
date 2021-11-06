using System;

namespace doturn
{
    /*
        ref: https://datatracker.ietf.org/doc/html/rfc5389#section-6
         0                   1                   2                   3
         0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |0 0|     STUN Message Type     |         Message Length        |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                         Magic Cookie                          |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                                                               |
        |                     Transaction ID (96 bits)                  |
        |                                                               |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

        ref: https://datatracker.ietf.org/doc/html/rfc5389#section-15
         0                   1                   2                   3
         0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |         Type                  |            Length             |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                         Value (variable)                ....
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

        MAPPED-ADDRESS
         0                   1                   2                   3
         0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |0 0 0 0 0 0 0 0|    Family     |           Port                |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                                                               |
        |                 Address (32 bits or 128 bits)                 |
        |                                                               |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

        XOR-MAPPED-ADDRESS
         0                   1                   2                   3
         0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |x x x x x x x x|    Family     |         X-Port                |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                X-Address (Variable)
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    */
    class StunHeader
    {
        public readonly StunMessage messageType;
        public readonly Int16 messageLength;
        public readonly byte[] magicCookie;
        public readonly byte[] transactionId;

        public StunHeader(StunMessage messageType, Int16 messageLength, byte[] magicCookie, byte[] transactionId)
        {
            this.messageType = messageType;
            this.messageLength = messageLength;
            this.magicCookie = magicCookie;
            this.transactionId = transactionId;
        }
        public static StunHeader FromRawHeader(byte[] rawHeader)
        {
            var messageTypeByte = rawHeader[0..2]; // 16bit
            var messageLengthByte = rawHeader[2..4]; // 16bit
            var magicCookieByte = rawHeader[4..8]; // 32bit
            var transactionIdByte = rawHeader[8..20]; // 96bit
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(messageTypeByte);
                Array.Reverse(messageLengthByte);
            }
            var messageType = (StunMessage)Enum.ToObject(typeof(StunMessage), BitConverter.ToInt16(messageTypeByte));
            var messageLength = BitConverter.ToInt16(messageLengthByte);
            return new StunHeader(messageType, messageLength, magicCookieByte, transactionIdByte);
        }
        public byte[] ToByte()
        {
            var arr = new byte[20];
            int endPos = 0;
            var messageTypeByte = this.messageType.ToByte();
            var messageLengthByte = BitConverter.GetBytes(this.messageLength);
            Array.Copy(messageTypeByte, 0, arr, endPos, messageTypeByte.Length);
            endPos += messageTypeByte.Length;
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(messageLengthByte);
            }
            Array.Copy(messageLengthByte, 0, arr, endPos, messageLengthByte.Length);
            endPos += messageLengthByte.Length;
            Array.Copy(this.magicCookie, 0, arr, endPos, this.magicCookie.Length);
            endPos += this.magicCookie.Length;
            Array.Copy(this.transactionId, 0, arr, endPos, this.transactionId.Length);
            return arr;
        }
    }
    class MappedAddress
    {
        public readonly byte[] address;
        public readonly byte[] port;
        public readonly StunHeader stunHeader;
        public MappedAddress(byte[] address, byte[] port, StunHeader stunHeader)
        {
            this.address = address;
            this.port = port;
            this.stunHeader = stunHeader;
        }
        //https://github.com/coturn/coturn/blob/master/src/client/ns_turn_msg.c#L630
        public byte[] ToByte()
        {
            var stunAttributeMappedAddress = new StunAttributeMappedAddress(this.address, this.port, this.stunHeader.magicCookie);
            var stunAttributeMappedAddressByte = stunAttributeMappedAddress.ToByte();
            Int16 length = (Int16)stunAttributeMappedAddressByte.Length;
            var lengthByte = BitConverter.GetBytes(length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByte);
            }
            var resStunHeader = new StunHeader(StunMessage.BINDING_SUCCESS, length, this.stunHeader.magicCookie, this.stunHeader.transactionId);
            var resStunHeaderByte = resStunHeader.ToByte();
            var res = new byte[resStunHeaderByte.Length + length];
            int endPos = 0;
            Array.Copy(resStunHeaderByte, 0, res, endPos, resStunHeaderByte.Length);
            endPos += resStunHeaderByte.Length;
            Array.Copy(stunAttributeMappedAddressByte, 0, res, endPos, stunAttributeMappedAddressByte.Length);
            return res;
        }
    }
    class XorMappedAddress
    {
        public readonly byte[] address;
        public readonly byte[] port;
        public readonly StunHeader stunHeader;
        public XorMappedAddress(byte[] address, byte[] port, StunHeader stunHeader)
        {
            this.address = address;
            this.port = port;
            this.stunHeader = stunHeader;
        }
        public byte[] ToByte()
        {
            var stunAttributeXorMappedAddress = new StunAttributeXorMappedAddress(this.address, this.port, this.stunHeader.magicCookie);
            var stunAttributeXorMappedAddressByte = stunAttributeXorMappedAddress.ToByte();
            Int16 length = (Int16)stunAttributeXorMappedAddressByte.Length;
            var lengthByte = BitConverter.GetBytes(length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByte);
            }
            var resStunHeader = new StunHeader(StunMessage.BINDING_SUCCESS, length, this.stunHeader.magicCookie, this.stunHeader.transactionId);
            var resStunHeaderByte = resStunHeader.ToByte();
            var res = new byte[resStunHeaderByte.Length + length];
            int endPos = 0;
            Array.Copy(resStunHeaderByte, 0, res, endPos, resStunHeaderByte.Length);
            endPos += resStunHeaderByte.Length;
            Array.Copy(stunAttributeXorMappedAddressByte, 0, res, endPos, stunAttributeXorMappedAddressByte.Length);
            return res;
        }
    }
}