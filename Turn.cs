using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace doturn
{
    class AllocateRequest
    {
        public readonly StunHeader stunHeader;
        public readonly List<IStunAttribute> attributes = new List<IStunAttribute>();
        private readonly string inputUsername;
        private readonly string inputRealm;

        private readonly string username;
        private readonly string password;
        private readonly string realm;
        private readonly StunAttributeMessageIntegrity messageIntegrity;

        public AllocateRequest(StunHeader stunHeader, byte[] body, string username, string password, string realm)
        {
            this.stunHeader = stunHeader;
            this.username = username;
            this.password = password;
            this.realm = realm;
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
                    var usernameStr = System.Text.Encoding.ASCII.GetString(usernameByte);
                    var stunAttributeUsername = new StunAttributeUsername(usernameStr);
                    this.attributes.Add(stunAttributeUsername);
                    this.inputUsername = usernameStr;
                }
                else if (attrType == StunAttrType.REALM)
                {
                    var realmByte = body[endPos..(attrLength + endPos)];
                    endPos += realmByte.Length;
                    var paddingLength = 8 - ((2 + 2 + attrLength) % 8);
                    endPos += paddingLength;
                    string realmStr = System.Text.Encoding.ASCII.GetString(realmByte);
                    var stunAttributeRealm = new StunAttributeRealm(realmStr);
                    this.attributes.Add(stunAttributeRealm);
                    this.inputRealm = realmStr;
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
                    var stunAttributemessageIntegrity = new StunAttributeMessageIntegrity(messageIntegrityByte);
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

            var md5 = MD5.Create();
            var keyString = $"{this.username}:{this.realm}:{this.password}";
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
    class AllocateSuccessResponse
    {
        public readonly StunHeader stunHeader;
        private readonly byte[] port;
        private readonly byte[] address;
        private readonly string externalIPAddress;
        private readonly Int16 relayPort;
        private readonly string username;
        private readonly string password;
        private readonly string realm;
        public AllocateSuccessResponse(StunHeader stunHeader, byte[] port, byte[] address, string externalIPAddress, Int16 relayPort, string username, string password, string realm)
        {
            this.stunHeader = stunHeader;
            this.port = port;
            this.address = address;
            this.externalIPAddress = externalIPAddress;
            this.relayPort = relayPort;
            this.username = username;
            this.password = password;
            this.realm = realm;
        }

        public byte[] ToByte()
        {
            var xorRelayedAddressAttr = new StunAttributeXorRelayedAddress(this.externalIPAddress, this.relayPort, this.stunHeader.magicCookie);
            var xorMappedAddressAttr = new StunAttributeXorMappedAddress(this.address, this.port, this.stunHeader.magicCookie);
            var lifetimeAttr = new StunAttributeLifetime();
            var softwareAttr = new StunAttributeSoftware();

            var xorRelayedAddressAttrByte = xorRelayedAddressAttr.ToByte();
            var xorMappedAddressAttrByte = xorMappedAddressAttr.ToByte();
            var lifetimeAttrByte = lifetimeAttr.ToByte();
            var softwareAttrByte = softwareAttr.ToByte();

            var messageIntegrityAttrLength = 24;
            var fingerprintAttrLength = 8;
            var length = xorRelayedAddressAttrByte.Length + xorMappedAddressAttrByte.Length + lifetimeAttrByte.Length + softwareAttrByte.Length + messageIntegrityAttrLength + fingerprintAttrLength;
            var lengthByte = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByte);
            }
            var resStunHeader = new StunHeader(StunMessage.ALLOCATE_SUCCESS, (Int16)(length - fingerprintAttrLength), this.stunHeader.magicCookie, this.stunHeader.transactionId);
            var resStunHeaderByte = resStunHeader.ToByte();

            var res = new byte[resStunHeaderByte.Length + length];
            int endPos = 0;
            Array.Copy(resStunHeaderByte, 0, res, endPos, resStunHeaderByte.Length);
            endPos += resStunHeaderByte.Length;
            Array.Copy(xorRelayedAddressAttrByte, 0, res, endPos, xorRelayedAddressAttrByte.Length);
            endPos += xorRelayedAddressAttrByte.Length;
            Array.Copy(xorMappedAddressAttrByte, 0, res, endPos, xorMappedAddressAttrByte.Length);
            endPos += xorMappedAddressAttrByte.Length;
            Array.Copy(lifetimeAttrByte, 0, res, endPos, lifetimeAttrByte.Length);
            endPos += lifetimeAttrByte.Length;
            Array.Copy(softwareAttrByte, 0, res, endPos, softwareAttrByte.Length);
            endPos += softwareAttrByte.Length;

            var messageIntegrityAttr = new StunAttributeMessageIntegrity(res[0..((resStunHeaderByte.Length + length) - (messageIntegrityAttrLength + fingerprintAttrLength))], this.username, this.password, this.realm);
            var messageIntegrityAttrByte = messageIntegrityAttr.ToByte();
            Array.Copy(messageIntegrityAttrByte, 0, res, endPos, messageIntegrityAttrByte.Length);
            endPos += messageIntegrityAttrByte.Length;

            res[2] = lengthByte[0];
            res[3] = lengthByte[1];

            var fingerprintAttr = new StunAttributeFingerprint(res);
            var fingerprintAttrByte = fingerprintAttr.ToByte();
            Array.Copy(fingerprintAttrByte, 0, res, endPos, fingerprintAttrByte.Length);
            return res;
        }
    }

    class AllocateErrorResponse
    {
        public readonly StunHeader stunHeader;
        public AllocateErrorResponse(StunHeader stunHeader)
        {
            this.stunHeader = stunHeader;
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