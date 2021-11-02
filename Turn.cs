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

    class AllocateResponse
    {
        public readonly StunHeader stunHeader;
        private readonly bool isValid;
        public AllocateResponse(StunHeader stunHeader, bool isValid)
        {
            this.stunHeader = stunHeader;
            this.isValid = isValid;
        }

        public byte[] ToByte()
        {
            var errorCodeAttr = new StunAttributeErrorCode((byte)4, (byte)1, "Unauthorized");
            var nonceAttr = new StunAttributeNonce();
            var realmAttr = new StunAttributeRealm();
            var softwareAttr = new StunAttributeSoftware();
            var errorCodeAttrByte = errorCodeAttr.ToByte();
            var nonceAttrByte = nonceAttr.ToByte();
            var realmAttrByte = realmAttr.ToByte();
            var softwareAttrByte = softwareAttr.ToByte();
            var fingerprintAttrLength = 8;
            var length = errorCodeAttrByte.Length + nonceAttrByte.Length + realmAttrByte.Length + softwareAttrByte.Length + fingerprintAttrLength;
            var resStunHeader = new StunHeader(StunMessage.ALLOCATE_ERROR, (Int16)length, this.stunHeader.magicCookie, this.stunHeader.transactionId);
            var resStunHeaderByte = resStunHeader.ToByte();

            var res = new byte[20 + length];
            int endPos = 0;
            Array.Copy(resStunHeaderByte, 0, res, endPos, resStunHeaderByte.Length);
            endPos += resStunHeaderByte.Length;
            Array.Copy(errorCodeAttrByte, 0, res, endPos, errorCodeAttrByte.Length);
            endPos += errorCodeAttrByte.Length;
            Array.Copy(nonceAttrByte, 0, res, endPos, nonceAttrByte.Length);
            endPos += nonceAttrByte.Length;
            Array.Copy(realmAttrByte, 0, res, endPos, realmAttrByte.Length);
            endPos += realmAttrByte.Length;
            Array.Copy(softwareAttrByte, 0, res, endPos, softwareAttrByte.Length);
            endPos += softwareAttrByte.Length;

            var fingerprintAttr = new StunAttributeFingerprint(res);
            var fingerprintAttrByte = fingerprintAttr.ToByte();
            Array.Copy(fingerprintAttrByte, 0, res, endPos, fingerprintAttrByte.Length);
            return res;
        }
    }
}