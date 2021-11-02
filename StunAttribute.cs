using System;
using System.Linq;
namespace doturn
{

    interface IStunAttribute
    {
        byte[] ToByte();
    }
    class StunAttribute : IStunAttribute
    {
        public readonly StunAttrType attr;
        private byte[] data;

        public StunAttribute(StunAttrType attr, byte[] data)
        {
            this.attr = attr;
            this.data = data;
        }
        public byte[] ToByte()
        {
            Int16 length = (Int16)this.data.Length;
            var res = new byte[2 + 2 + length];
            var attrByte = this.attr.ToByte();
            var lengthByte = BitConverter.GetBytes(length);
            int endPos = 0;
            Array.Copy(attrByte, 0, res, endPos, attrByte.Length);
            endPos += attrByte.Length;
            Array.Copy(lengthByte, 0, res, endPos, lengthByte.Length);
            endPos += lengthByte.Length;
            Array.Copy(data, 0, res, endPos, data.Length);
            return res;
        }
    }
    class StunAttributeErrorCode : IStunAttribute
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
        public byte[] ToByte()
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
    }
    class StunAttributeNonce : IStunAttribute
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
        public byte[] ToByte()
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
    }
    class StunAttributeRealm : IStunAttribute
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
        public byte[] ToByte()
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
    }
    class StunAttributeSoftware : IStunAttribute
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
        public byte[] ToByte()
        {
            var attrTypeByte = this.attrType.ToByte();
            var realmByte = System.Text.Encoding.ASCII.GetBytes(this.software);
            var length = realmByte.Length;
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
    }
}