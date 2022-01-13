using System;

namespace Doturn.StunAttribute
{
    public class ErrorCode : StunAttributeBase
    {
        public readonly Type type = Type.ERROR_CODE;
        public readonly byte errorClass;
        public readonly byte errorCode;
        public readonly string errorReasonPhrase;
        public override Type Type => type;
        public ErrorCode(byte errorClass, byte errorCode, string errorReasonPhrase)
        {
            this.errorClass = errorClass;
            this.errorCode = errorCode;
            this.errorReasonPhrase = errorReasonPhrase;
        }
        public override byte[] ToBytes()
        {
            byte[] typeByteArray = type.ToBytes();
            byte[] errorByteArray = { errorClass, errorCode };
            byte[] errorReasonPhraseByteArray = System.Text.Encoding.ASCII.GetBytes(errorReasonPhrase);
            byte[] reserved = BitConverter.GetBytes((short)0x0000);
            int length = errorByteArray.Length + errorReasonPhraseByteArray.Length + reserved.Length;
            byte[] lengthByteArray = BitConverter.GetBytes((short)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }
            byte[] res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, reserved, errorByteArray, errorReasonPhraseByteArray);
            return res;
        }

        public static ErrorCode Parse(byte[] data)
        {
            _ = data[0..2];
            byte errorClassByte = data[2];
            byte errorCodeByte = data[3];
            string errorReasonPhrase = System.Text.Encoding.ASCII.GetString(data[4..data.Length]);
            return new ErrorCode(errorClassByte, errorCodeByte, errorReasonPhrase);
        }
    }
}