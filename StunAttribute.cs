using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Force.Crc32;

namespace doturn
{

    interface IStunAttribute
    {
        byte[] ToByte();
        StunAttrType AttrType { get; }
    }
    abstract class StunAttributeBase : IStunAttribute
    {
        public abstract StunAttrType AttrType { get; }
        public abstract byte[] ToByte();
    }
    class StunAttribute : StunAttributeBase
    {
        public readonly StunAttrType attrType;
        private byte[] data;

        public StunAttribute(StunAttrType attr, byte[] data)
        {
            this.attrType = attr;
            this.data = data;
        }
        public override byte[] ToByte()
        {
            Int16 length = (Int16)this.data.Length;
            var res = new byte[2 + 2 + length];
            var attrByte = this.attrType.ToByte();
            var lengthByte = BitConverter.GetBytes(length);
            int endPos = 0;
            Array.Copy(attrByte, 0, res, endPos, attrByte.Length);
            endPos += attrByte.Length;
            Array.Copy(lengthByte, 0, res, endPos, lengthByte.Length);
            endPos += lengthByte.Length;
            Array.Copy(data, 0, res, endPos, data.Length);
            return res;
        }
        public override StunAttrType AttrType
        {
            get
            {
                return this.attrType;
            }
        }
    }
    class StunAttributeRequestedTransport : StunAttributeBase
    {
        public readonly StunAttrType attrType = StunAttrType.REQUESTED_TRANSPORT;
        public readonly Transport transport;
        public readonly byte[] reserved;

        public StunAttributeRequestedTransport(Transport transport, byte[] reserved)
        {
            this.transport = transport;
            this.reserved = reserved;
        }
        public override byte[] ToByte()
        {
            var attrTypeByte = this.attrType.ToByte();
            var transportByte = this.transport.ToByte();
            var length = transportByte.Length + this.reserved.Length;
            var lengthByte = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByte);
            }

            var res = new byte[2 + 2 + length];
            int endPos = 0;
            Array.Copy(attrTypeByte, 0, res, endPos, attrTypeByte.Length);
            endPos += attrTypeByte.Length;
            Array.Copy(lengthByte, 0, res, endPos, lengthByte.Length);
            endPos += lengthByte.Length;
            Array.Copy(transportByte, 0, res, endPos, transportByte.Length);
            endPos += transportByte.Length;
            Array.Copy(this.reserved, 0, res, endPos, this.reserved.Length);
            endPos += this.reserved.Length;
            return res;
        }
        public override StunAttrType AttrType
        {
            get
            {
                return this.attrType;
            }
        }
    }
    class StunAttributeUsername : StunAttributeBase
    {
        public readonly StunAttrType attrType = StunAttrType.USERNAME;
        public readonly string username;

        public StunAttributeUsername(string username)
        {
            this.username = username;
        }
        public override byte[] ToByte()
        {
            var attrTypeByte = this.attrType.ToByte();
            var usernameByte = System.Text.Encoding.ASCII.GetBytes(this.username);
            var length = usernameByte.Length;
            var lengthByte = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByte);
            }

            var res = new byte[2 + 2 + length];
            int endPos = 0;
            Array.Copy(attrTypeByte, 0, res, endPos, attrTypeByte.Length);
            endPos += attrTypeByte.Length;
            Array.Copy(lengthByte, 0, res, endPos, lengthByte.Length);
            endPos += lengthByte.Length;
            Array.Copy(usernameByte, 0, res, endPos, usernameByte.Length);
            return res;
        }
        public override StunAttrType AttrType
        {
            get
            {
                return this.attrType;
            }
        }
    }
    class StunAttributeErrorCode : StunAttributeBase
    {
        public readonly StunAttrType attrType = StunAttrType.ERROR_CODE;
        public readonly byte errorClass;
        public readonly byte errorCode;
        public readonly string errorReasonPhrase;

        public StunAttributeErrorCode(byte errorClass, byte errorCode, string errorReasonPhrase)
        {
            this.errorClass = errorClass;
            this.errorCode = errorCode;
            this.errorReasonPhrase = errorReasonPhrase;
        }
        public override byte[] ToByte()
        {
            byte[] errorByte = { this.errorClass, this.errorCode };
            var errorReasonPhraseByte = System.Text.Encoding.ASCII.GetBytes(this.errorReasonPhrase);
            var reserved = BitConverter.GetBytes((Int16)0x0000);
            var length = errorByte.Length + errorReasonPhraseByte.Length + reserved.Length;
            var lengthByte = BitConverter.GetBytes((Int16)length);
            var attrTypeByte = this.attrType.ToByte();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByte);
            }

            var res = new byte[2 + 2 + length];
            int endPos = 0;
            Array.Copy(attrTypeByte, 0, res, endPos, attrTypeByte.Length);
            endPos += attrTypeByte.Length;
            Array.Copy(lengthByte, 0, res, endPos, lengthByte.Length);
            endPos += lengthByte.Length;
            Array.Copy(reserved, 0, res, endPos, reserved.Length);
            endPos += reserved.Length;
            Array.Copy(errorByte, 0, res, endPos, errorByte.Length);
            endPos += errorByte.Length;
            Array.Copy(errorReasonPhraseByte, 0, res, endPos, errorReasonPhraseByte.Length);
            return res;
        }
        public override StunAttrType AttrType
        {
            get
            {
                return this.attrType;
            }
        }
    }
    class StunAttributeNonce : StunAttributeBase
    {
        public readonly StunAttrType attrType = StunAttrType.NONCE;
        public readonly string nonce;
        private static Random random = new Random();

        public StunAttributeNonce()
        {
            this.nonce = generateNonce(16);
        }
        public StunAttributeNonce(string nonce)
        {
            this.nonce = nonce;
        }
        public override byte[] ToByte()
        {
            var attrTypeByte = this.attrType.ToByte();
            var lengthByte = BitConverter.GetBytes((Int16)16);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByte);
            }
            var nonceByte = System.Text.Encoding.ASCII.GetBytes(this.nonce);

            var res = new byte[20];
            int endPos = 0;
            Array.Copy(attrTypeByte, 0, res, endPos, attrTypeByte.Length);
            endPos += attrTypeByte.Length;
            Array.Copy(lengthByte, 0, res, endPos, lengthByte.Length);
            endPos += lengthByte.Length;
            Array.Copy(nonceByte, 0, res, endPos, nonceByte.Length);
            return res;
        }
        private string generateNonce(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public override StunAttrType AttrType
        {
            get
            {
                return this.attrType;
            }
        }
    }
    class StunAttributeRealm : StunAttributeBase
    {
        public readonly StunAttrType attrType = StunAttrType.REALM;
        public readonly string realm;

        public StunAttributeRealm()
        {
            this.realm = "example.com";
        }
        public StunAttributeRealm(string realm)
        {
            this.realm = realm;
        }
        public override byte[] ToByte()
        {
            var attrTypeByte = this.attrType.ToByte();
            var realmByte = System.Text.Encoding.ASCII.GetBytes(this.realm);
            var length = realmByte.Length;
            var paddingLength = 8 - ((2 + 2 + length) % 8);
            var lengthByte = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByte);
            }

            var res = new byte[2 + 2 + length + paddingLength];
            int endPos = 0;
            Array.Copy(attrTypeByte, 0, res, endPos, attrTypeByte.Length);
            endPos += attrTypeByte.Length;
            Array.Copy(lengthByte, 0, res, endPos, lengthByte.Length);
            endPos += lengthByte.Length;
            Array.Copy(realmByte, 0, res, endPos, realmByte.Length);
            endPos += realmByte.Length;
            byte[] padding = { 0 };
            for (int i = 0; i < paddingLength; i++)
            {
                Array.Copy(padding, 0, res, endPos, padding.Length);
                endPos += padding.Length;
            }
            return res;
        }
        public override StunAttrType AttrType
        {
            get
            {
                return this.attrType;
            }
        }
    }
    class StunAttributeSoftware : StunAttributeBase
    {
        public readonly StunAttrType attrType = StunAttrType.SOFTWARE;
        public readonly string software;

        public StunAttributeSoftware()
        {
            this.software = "None";
        }
        public StunAttributeSoftware(string software)
        {
            this.software = software;
        }
        public override byte[] ToByte()
        {
            var attrTypeByte = this.attrType.ToByte();
            var softwareByte = System.Text.Encoding.ASCII.GetBytes(this.software);
            var length = softwareByte.Length;
            var paddingLength = 8 - ((2 + 2 + length) % 8);
            if (paddingLength >= 8)
            {
                paddingLength = 0;
            }
            var lengthByte = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByte);
            }

            var res = new byte[2 + 2 + length + paddingLength];
            int endPos = 0;
            Array.Copy(attrTypeByte, 0, res, endPos, attrTypeByte.Length);
            endPos += attrTypeByte.Length;
            Array.Copy(lengthByte, 0, res, endPos, lengthByte.Length);
            endPos += lengthByte.Length;
            Array.Copy(softwareByte, 0, res, endPos, softwareByte.Length);
            endPos += softwareByte.Length;
            byte[] padding = { 0 };
            for (int i = 0; i < paddingLength; i++)
            {
                Array.Copy(padding, 0, res, endPos, padding.Length);
                endPos += padding.Length;
            }
            return res;
        }
        public override StunAttrType AttrType
        {
            get
            {
                return this.attrType;
            }
        }
    }
    class StunAttributeFingerprint : StunAttributeBase
    {
        public readonly StunAttrType attrType = StunAttrType.FINGERPRINT;
        public readonly byte[] crc32;

        public StunAttributeFingerprint(byte[] data)
        {
            var crc32 = Crc32Algorithm.Compute(data, 0, data.Length);
            var crc32Byte = BitConverter.GetBytes(crc32);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(crc32Byte);
            }
            var crc32XorByte = new byte[crc32Byte.Length];
            var fingerprintXor = Stun.FINGERPRINT_XOR.ToByte();
            for (int i = 0; i < crc32Byte.Length; i++)
            {
                crc32XorByte[i] = (byte)(crc32Byte[i] ^ fingerprintXor[i]);
            }
            this.crc32 = crc32XorByte;
        }
        public override byte[] ToByte()
        {
            var attrTypeByte = this.attrType.ToByte();
            var lengthByte = BitConverter.GetBytes((Int16)4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByte);
            }

            var res = new byte[8];
            int endPos = 0;
            Array.Copy(attrTypeByte, 0, res, endPos, attrTypeByte.Length);
            endPos += attrTypeByte.Length;
            Array.Copy(lengthByte, 0, res, endPos, lengthByte.Length);
            endPos += lengthByte.Length;
            Array.Copy(this.crc32, 0, res, endPos, this.crc32.Length);
            return res;
        }
        public override StunAttrType AttrType
        {
            get
            {
                return this.attrType;
            }
        }
    }
    class StunAttributeMessageIntegrity : StunAttributeBase
    {
        public readonly StunAttrType attrType = StunAttrType.MESSAGE_INTEGRITY;
        public readonly byte[] messageIntegrity;

        public StunAttributeMessageIntegrity(byte[] messageIntegrity)
        {
            this.messageIntegrity = messageIntegrity;
        }
        public StunAttributeMessageIntegrity(byte[] data, string username, string password, string realm)
        {
            var md5 = MD5.Create();
            var keyString = $"{username}:{realm}:{password}";
            var keyStringByte = System.Text.Encoding.ASCII.GetBytes(keyString);
            var md5HashByte = md5.ComputeHash(keyStringByte);
            var hmacSHA1 = new HMACSHA1(md5HashByte);
            md5.Clear();
            var hmacSHA1Byte = hmacSHA1.ComputeHash(data);
            this.messageIntegrity = hmacSHA1Byte;
            hmacSHA1.Clear();
        }
        public override byte[] ToByte()
        {
            var attrTypeByte = this.attrType.ToByte();
            var length = this.messageIntegrity.Length;
            var lengthByte = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByte);
            }

            var res = new byte[2 + 2 + length];
            int endPos = 0;
            Array.Copy(attrTypeByte, 0, res, endPos, attrTypeByte.Length);
            endPos += attrTypeByte.Length;
            Array.Copy(lengthByte, 0, res, endPos, lengthByte.Length);
            endPos += lengthByte.Length;
            Array.Copy(this.messageIntegrity, 0, res, endPos, this.messageIntegrity.Length);
            return res;
        }
        public override StunAttrType AttrType
        {
            get
            {
                return this.attrType;
            }
        }
    }
    class StunAttributeMappedAddress : StunAttributeBase
    {
        public readonly StunAttrType attrType = StunAttrType.MAPPED_ADDRESS;
        public readonly byte[] address;
        public readonly byte[] port;
        public readonly byte[] magicCookie;

        public StunAttributeMappedAddress(byte[] address, byte[] port, byte[] magicCookie)
        {
            this.address = address;
            this.port = port;
            this.magicCookie = magicCookie;
        }
        //https://github.com/coturn/coturn/blob/master/src/client/ns_turn_msg.c#L630
        public override byte[] ToByte()
        {
            var attrTypeByte = this.attrType.ToByte();
            byte[] reserved = { 0x00 };
            byte[] addressFamilyByte = { 0x01 };
            var length = reserved.Length + addressFamilyByte.Length + this.port.Length + this.address.Length;
            var lengthByte = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByte);
            }
            var res = new byte[2 + 2 + length];
            int endPos = 0;
            Array.Copy(attrTypeByte, 0, res, endPos, attrTypeByte.Length);
            endPos += attrTypeByte.Length;
            Array.Copy(lengthByte, 0, res, endPos, lengthByte.Length);
            endPos += lengthByte.Length;
            Array.Copy(reserved, 0, res, endPos, reserved.Length);
            endPos += reserved.Length;
            Array.Copy(addressFamilyByte, 0, res, endPos, addressFamilyByte.Length);
            endPos += addressFamilyByte.Length;
            Array.Copy(this.port, 0, res, endPos, this.port.Length);
            endPos += this.port.Length;
            Array.Copy(this.address, 0, res, endPos, this.address.Length);
            endPos += this.address.Length;
            return res;
        }
        public override StunAttrType AttrType
        {
            get
            {
                return this.attrType;
            }
        }
    }

    class StunAttributeXorMappedAddress : StunAttributeBase
    {
        public readonly StunAttrType attrType = StunAttrType.XOR_MAPPED_ADDRESS;
        public readonly byte[] address;
        public readonly byte[] port;
        public readonly byte[] magicCookie;

        public StunAttributeXorMappedAddress(byte[] address, byte[] port, byte[] magicCookie)
        {
            this.address = address;
            this.port = port;
            this.magicCookie = magicCookie;
        }
        public override byte[] ToByte()
        {
            var portBytesXor = new byte[2];
            portBytesXor[0] = (byte)(this.port[2] ^ this.magicCookie[0]);
            portBytesXor[1] = (byte)(this.port[3] ^ this.magicCookie[1]);

            var addressBytesXor = new byte[this.address.Length];
            for (int i = 0; i < this.address.Length; i++)
            {
                addressBytesXor[i] = (byte)(this.address[i] ^ this.magicCookie[i]);
            }

            var attrTypeByte = this.attrType.ToByte();
            byte[] reserved = { 0x00 };
            byte[] addressFamilyByte = { 0x01 };
            var length = reserved.Length + addressFamilyByte.Length + portBytesXor.Length + addressBytesXor.Length;
            var lengthByte = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByte);
            }
            var res = new byte[2 + 2 + length];
            int endPos = 0;
            Array.Copy(attrTypeByte, 0, res, endPos, attrTypeByte.Length);
            endPos += attrTypeByte.Length;
            Array.Copy(lengthByte, 0, res, endPos, lengthByte.Length);
            endPos += lengthByte.Length;
            Array.Copy(reserved, 0, res, endPos, reserved.Length);
            endPos += reserved.Length;
            Array.Copy(addressFamilyByte, 0, res, endPos, addressFamilyByte.Length);
            endPos += addressFamilyByte.Length;
            Array.Copy(portBytesXor, 0, res, endPos, portBytesXor.Length);
            endPos += portBytesXor.Length;
            Array.Copy(addressBytesXor, 0, res, endPos, addressBytesXor.Length);
            endPos += addressBytesXor.Length;
            return res;
        }
        public override StunAttrType AttrType
        {
            get
            {
                return this.attrType;
            }
        }
    }
    class StunAttributeXorRelayedAddress : StunAttributeBase
    {
        public readonly StunAttrType attrType = StunAttrType.XOR_RELAYED_ADDRESS;
        public readonly byte[] address;
        public readonly byte[] port;
        public readonly byte[] magicCookie;

        public StunAttributeXorRelayedAddress(string externalIPAddress, Int16 relayPort, byte[] magicCookie)
        {
            var ipAddress = IPAddress.Parse(externalIPAddress);
            this.address = ipAddress.GetAddressBytes();
            var portByte = BitConverter.GetBytes(relayPort);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(portByte);
            }
            this.port = portByte;
            this.magicCookie = magicCookie;
        }
        public override byte[] ToByte()
        {
            var portBytesXor = new byte[2];
            portBytesXor[0] = (byte)(this.port[0] ^ this.magicCookie[0]);
            portBytesXor[1] = (byte)(this.port[1] ^ this.magicCookie[1]);

            var addressBytesXor = new byte[this.address.Length];
            for (int i = 0; i < this.address.Length; i++)
            {
                addressBytesXor[i] = (byte)(this.address[i] ^ this.magicCookie[i]);
            }

            var attrTypeByte = this.attrType.ToByte();
            byte[] reserved = { 0x00 };
            byte[] addressFamilyByte = { 0x01 };
            var length = reserved.Length + addressFamilyByte.Length + portBytesXor.Length + addressBytesXor.Length;
            var lengthByte = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByte);
            }
            var res = new byte[2 + 2 + length];
            int endPos = 0;
            Array.Copy(attrTypeByte, 0, res, endPos, attrTypeByte.Length);
            endPos += attrTypeByte.Length;
            Array.Copy(lengthByte, 0, res, endPos, lengthByte.Length);
            endPos += lengthByte.Length;
            Array.Copy(reserved, 0, res, endPos, reserved.Length);
            endPos += reserved.Length;
            Array.Copy(addressFamilyByte, 0, res, endPos, addressFamilyByte.Length);
            endPos += addressFamilyByte.Length;
            Array.Copy(portBytesXor, 0, res, endPos, portBytesXor.Length);
            endPos += portBytesXor.Length;
            Array.Copy(addressBytesXor, 0, res, endPos, addressBytesXor.Length);
            endPos += addressBytesXor.Length;
            return res;
        }
        public override StunAttrType AttrType
        {
            get
            {
                return this.attrType;
            }
        }
    }
}