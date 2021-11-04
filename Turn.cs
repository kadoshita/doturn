using System;
using System.Collections.Generic;

namespace doturn
{
    class AllocateRequest
    {
        public readonly StunHeader stunHeader;
        public readonly List<IStunAttribute> attributes = new List<IStunAttribute>();

        public AllocateRequest(StunHeader stunHeader, byte[] body)
        {
            this.stunHeader = stunHeader;
            var endPos = 0;
            for (; body.Length > endPos;)
            {
                var attrTypeByte = body[(0 + endPos)..(2 + endPos)];
                endPos += attrTypeByte.Length;
                var attrLengthByte = body[endPos..(2 + endPos)];
                endPos += attrLengthByte.Length;
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(attrTypeByte);
                    Array.Reverse(attrLengthByte);
                }

                var attrType = (StunAttrType)Enum.ToObject(typeof(StunAttrType), BitConverter.ToInt16(attrTypeByte));
                var attrLength = BitConverter.ToInt16(attrLengthByte);
                if (attrType == StunAttrType.REQUESTED_TRANSPORT)
                {
                    var transportByte = body[endPos..(1 + endPos)];
                    endPos += transportByte.Length;
                    var reserved = body[endPos..((attrLength - transportByte.Length) + endPos)];
                    endPos += reserved.Length;
                    var transport = (Transport)Enum.ToObject(typeof(Transport), (byte)transportByte[0]);
                    var stunAttributeRequestedTransport = new StunAttributeRequestedTransport(transport, reserved);
                    this.attributes.Add(stunAttributeRequestedTransport);
                }
                else if (attrType == StunAttrType.USERNAME)
                {
                    var usernameByte = body[endPos..(attrLength + endPos)];
                    endPos += usernameByte.Length;
                    var username = BitConverter.ToString(usernameByte);
                    var stunAttributeUsername = new StunAttributeUsername(username);
                    this.attributes.Add(stunAttributeUsername);
                }
                else if (attrType == StunAttrType.REALM)
                {
                    var realmByte = body[endPos..(attrLength + endPos)];
                    endPos += realmByte.Length;
                    var paddingLength = 8 - ((2 + 2 + attrLength) % 8);
                    endPos += paddingLength;
                    var realm = BitConverter.ToString(realmByte);
                    var stunAttributeRealm = new StunAttributeRealm(realm);
                    this.attributes.Add(stunAttributeRealm);
                }
                else if (attrType == StunAttrType.NONCE)
                {
                    var nonceByte = body[endPos..(attrLength + endPos)];
                    endPos += nonceByte.Length;
                    var nonce = BitConverter.ToString(nonceByte);
                    var stunAttributeNonce = new StunAttributeNonce(nonce);
                    this.attributes.Add(stunAttributeNonce);
                }
                else if (attrType == StunAttrType.MESSAGE_INTEGRITY)
                {
                    var messageIntegrityByte = body[endPos..(attrLength + endPos)];
                    endPos += messageIntegrityByte.Length;
                    var messageIntegrity = BitConverter.ToString(messageIntegrityByte);
                    var stunAttributemessageIntegrity = new StunAttributemessageIntegrity(messageIntegrity);
                    this.attributes.Add(stunAttributemessageIntegrity);
                }
            }
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