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
        public readonly byte[] messageType;
        public readonly byte[] messageLength;
        public readonly byte[] magicCookie;
        public readonly byte[] transactionId;

        public StunHeader(byte[] rawHeader)
        {
            this.messageType = rawHeader[0..2]; // 16bit
            this.messageLength = rawHeader[2..4]; // 16bit
            this.magicCookie = rawHeader[4..8]; // 32bit
            this.transactionId = rawHeader[8..20]; // 96bit
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
            var resMessageType = Utils.stringToByteArray("0101");
            var resLength = Utils.stringToByteArray("000c");
            var stunAttr = Utils.stringToByteArray("0010");
            var resDataLength = Utils.stringToByteArray("0008");
            var resDataAddressType = Utils.stringToByteArray("0001");
            var res = new byte[32];
            int endPos = 0;
            Array.Copy(resMessageType, 0, res, endPos, resMessageType.Length);
            endPos += resMessageType.Length;
            Array.Copy(resLength, 0, res, endPos, resLength.Length);
            endPos += resLength.Length;
            Array.Copy(this.stunHeader.magicCookie, 0, res, endPos, this.stunHeader.magicCookie.Length);
            endPos += this.stunHeader.magicCookie.Length;
            Array.Copy(this.stunHeader.transactionId, 0, res, endPos, this.stunHeader.transactionId.Length);
            endPos += this.stunHeader.transactionId.Length;
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

            var resMessageType = Utils.stringToByteArray("0101");
            var resLength = Utils.stringToByteArray("000c");
            var stunAttr = Utils.stringToByteArray("0020");
            var resDataLength = Utils.stringToByteArray("0008");
            var resDataAddressType = Utils.stringToByteArray("0001");
            var res = new byte[32];
            int endPos = 0;
            Array.Copy(resMessageType, 0, res, endPos, resMessageType.Length);
            endPos += resMessageType.Length;
            Array.Copy(resLength, 0, res, endPos, resLength.Length);
            endPos += resLength.Length;
            Array.Copy(this.stunHeader.magicCookie, 0, res, endPos, this.stunHeader.magicCookie.Length);
            endPos += this.stunHeader.magicCookie.Length;
            Array.Copy(this.stunHeader.transactionId, 0, res, endPos, this.stunHeader.transactionId.Length);
            endPos += this.stunHeader.transactionId.Length;
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