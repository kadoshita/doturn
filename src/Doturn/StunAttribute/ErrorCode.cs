using System;
using System.Buffers.Binary;
using System.Text;

namespace Doturn.StunAttribute;

public class ErrorCode : StunAttributeBase
{
    public byte Class { get; }
    public byte Code { get; }
    public string ReasonPhrase { get; }
    public override Type Type => Type.ErrorCode;
    public ErrorCode(byte errorClass, byte errorCode, string errorReasonPhrase)
    {
        Class = errorClass;
        Code = errorCode;
        ReasonPhrase = errorReasonPhrase;
    }
    public override byte[] ToBytes()
    {
        byte[] errorReasonPhraseByteArray = Encoding.ASCII.GetBytes(ReasonPhrase);
        int length = 2 + 2 + errorReasonPhraseByteArray.Length; // reserved(2) + class/code(2) + phrase
        byte[] res = new byte[2 + 2 + length];
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(0, 2), (ushort)Type);
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(2, 2), (ushort)length);
        // res[4..6] = reserved (already 0)
        res[6] = Class;
        res[7] = Code;
        errorReasonPhraseByteArray.CopyTo(res, 8);
        return res;
    }

    public static ErrorCode Parse(byte[] data)
    {
        byte errorClass = data[2];
        byte errorCode = data[3];
        string errorReasonPhrase = Encoding.ASCII.GetString(data[4..data.Length]);
        return new ErrorCode(errorClass, errorCode, errorReasonPhrase);
    }
}
