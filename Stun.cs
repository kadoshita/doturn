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
            var stunAttr = StunAttr.MAPPED_ADDRESS.ToByte();
            var resDataLength = BitConverter.GetBytes((Int16)8);
            var resDataAddressType = BitConverter.GetBytes((Int16)1);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(resDataLength);
                Array.Reverse(resDataAddressType);
            }
            var res = new byte[32];
            var resStunHeader = new StunHeader(StunMessage.BINDING_SUCCESS, 12, this.stunHeader.magicCookie, this.stunHeader.transactionId);
            var resStunHeaderByte = resStunHeader.ToByte();
            int endPos = 0;
            Array.Copy(resStunHeaderByte, 0, res, endPos, resStunHeaderByte.Length);
            endPos += resStunHeaderByte.Length;
            Array.Copy(stunAttr, 0, res, endPos, stunAttr.Length);
            endPos += stunAttr.Length;
            Array.Copy(resDataLength, 0, res, endPos, resDataLength.Length);
            endPos += resDataLength.Length;
            Array.Copy(resDataAddressType, 0, res, endPos, resDataAddressType.Length);
            endPos += resDataAddressType.Length;
            Array.Copy(this.port, 0, res, endPos, this.port.Length);
            endPos += this.port.Length;
            Array.Copy(this.address, 0, res, endPos, this.address.Length);
            endPos += this.address.Length;
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
            var portBytesXor = new byte[2];
            portBytesXor[0] = (byte)(this.port[2] ^ stunHeader.magicCookie[0]);
            portBytesXor[1] = (byte)(this.port[3] ^ stunHeader.magicCookie[1]);

            var addressBytesXor = new byte[this.address.Length];
            for (int i = 0; i < this.address.Length; i++)
            {
                addressBytesXor[i] = (byte)(this.address[i] ^ stunHeader.magicCookie[i]);
            }

            var stunAttr = StunAttr.XOR_MAPPED_ADDRESS.ToByte();
            var resDataLength = BitConverter.GetBytes((Int16)8);
            var resDataAddressType = BitConverter.GetBytes((Int16)1);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(resDataLength);
                Array.Reverse(resDataAddressType);
            }
            var res = new byte[32];
            var resStunHeader = new StunHeader(StunMessage.BINDING_SUCCESS, 12, this.stunHeader.magicCookie, this.stunHeader.transactionId);
            var resStunHeaderByte = resStunHeader.ToByte();
            int endPos = 0;
            Array.Copy(resStunHeaderByte, 0, res, endPos, resStunHeaderByte.Length);
            endPos += resStunHeaderByte.Length;
            Array.Copy(stunAttr, 0, res, endPos, stunAttr.Length);
            endPos += stunAttr.Length;
            Array.Copy(resDataLength, 0, res, endPos, resDataLength.Length);
            endPos += resDataLength.Length;
            Array.Copy(resDataAddressType, 0, res, endPos, resDataAddressType.Length);
            endPos += resDataAddressType.Length;
            Array.Copy(portBytesXor, 0, res, endPos, portBytesXor.Length);
            endPos += portBytesXor.Length;
            Array.Copy(addressBytesXor, 0, res, endPos, addressBytesXor.Length);
            endPos += addressBytesXor.Length;
            return res;
        }
    }
}