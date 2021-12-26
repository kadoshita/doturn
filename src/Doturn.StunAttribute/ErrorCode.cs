using System;
using Doturn.Common;

namespace Doturn.StunAttribute
{
    public class ErrorCode : StunAttributeBase
    {
        public readonly Type type = Type.ERROR_CODE;
        private readonly byte errorClass;
        private readonly byte errorCode;
        private readonly string errorReasonPhrase;
        public override Type Type => this.type;
        public ErrorCode(byte errorClass, byte errorCode, string errorReasonPhrase)
        {
            this.errorClass = errorClass;
            this.errorCode = errorCode;
            this.errorReasonPhrase = errorReasonPhrase;
        }
        public override byte[] ToByte()
        {
            var typeByteArray = this.type.ToByte();
            byte[] errorByteArray = { this.errorClass, this.errorCode };
            var errorReasonPhraseByteArray = System.Text.Encoding.ASCII.GetBytes(this.errorReasonPhrase);
            var reserved = BitConverter.GetBytes((Int16)0x0000);
            var length = errorByteArray.Length + errorReasonPhraseByteArray.Length + reserved.Length;
            var lengthByteArray = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }
            var res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, reserved, errorByteArray, errorReasonPhraseByteArray);
            return res;
        }
    }
}