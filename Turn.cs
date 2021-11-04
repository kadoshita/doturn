using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace doturn
{
    class AllocateRequest
    {
        public readonly StunHeader stunHeader;
        public readonly List<IStunAttribute> attributes = new List<IStunAttribute>();
        private readonly string username;
        private readonly string realm;
        private readonly StunAttributemessageIntegrity messageIntegrity;

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
                    var username = System.Text.Encoding.ASCII.GetString(usernameByte);
                    var stunAttributeUsername = new StunAttributeUsername(username);
                    this.attributes.Add(stunAttributeUsername);
                    this.username = username;
                }
                else if (attrType == StunAttrType.REALM)
                {
                    var realmByte = body[endPos..(attrLength + endPos)];
                    endPos += realmByte.Length;
                    var paddingLength = 8 - ((2 + 2 + attrLength) % 8);
                    endPos += paddingLength;
                    string realm = System.Text.Encoding.ASCII.GetString(realmByte);
                    var stunAttributeRealm = new StunAttributeRealm(realm);
                    this.attributes.Add(stunAttributeRealm);
                    this.realm = realm;
                }
                else if (attrType == StunAttrType.NONCE)
                {
                    var nonceByte = body[endPos..(attrLength + endPos)];
                    endPos += nonceByte.Length;
                    var nonce = System.Text.Encoding.ASCII.GetString(nonceByte);
                    var stunAttributeNonce = new StunAttributeNonce(nonce);
                    this.attributes.Add(stunAttributeNonce);
                }
                else if (attrType == StunAttrType.MESSAGE_INTEGRITY)
                {
                    var messageIntegrityByte = body[endPos..(attrLength + endPos)];
                    endPos += messageIntegrityByte.Length;
                    var stunAttributemessageIntegrity = new StunAttributemessageIntegrity(messageIntegrityByte);
                    this.attributes.Add(stunAttributemessageIntegrity);
                    this.messageIntegrity = stunAttributemessageIntegrity;
                }
            }
        }

        public bool isValid()
        {
            if (this.messageIntegrity == null)
            {
                return false;
            }
            var messageIntegrityByte = this.messageIntegrity.ToByte();
            var messageByte = new byte[this.stunHeader.messageLength + 20 - messageIntegrityByte.Length];
            var stunHeaderByte = this.stunHeader.ToByte();
            var endPos = 0;
            Array.Copy(stunHeaderByte, 0, messageByte, endPos, stunHeaderByte.Length);
            endPos += stunHeaderByte.Length;
            foreach (var attr in this.attributes)
            {
                if (attr.AttrType == StunAttrType.MESSAGE_INTEGRITY)
                {
                    break;
                }
                var attrByte = attr.ToByte();
                Array.Copy(attrByte, 0, messageByte, endPos, attrByte.Length);
                endPos += attrByte.Length;
            }

            var username = "username";
            var password = "password";
            var realm = "example.com";
            var md5 = MD5.Create();
            var keyString = $"{username}:{realm}:{password}";
            var keyStringByte = System.Text.Encoding.ASCII.GetBytes(keyString);
            var md5HashByte = md5.ComputeHash(keyStringByte);
            var hmacSHA1 = new HMACSHA1(md5HashByte);
            md5.Clear();
            var hmacSHA1Byte = hmacSHA1.ComputeHash(messageByte);
            var hmacSHA1String = BitConverter.ToString(hmacSHA1Byte);
            hmacSHA1.Clear();
            var messageIntegrityString = BitConverter.ToString(this.messageIntegrity.messageIntegrity);

            return hmacSHA1String == messageIntegrityString;
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