using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class ErrorCodeTest
    {
        [Theory]
        [InlineData((byte)4, (byte)1, "Unauthorized", "00-09-00-10-00-00-04-01-55-6E-61-75-74-68-6F-72-69-7A-65-64")]
        public void ErrorCode_Convert_To_ByteArray(byte errorClass, byte errorCode, string errorReasonPhrase, string expect)
        {
            var errorCodeObj = new ErrorCode(errorClass, errorCode, errorReasonPhrase);
            var byteArray = errorCodeObj.ToBytes();
            var byteArrayString = BitConverter.ToString(byteArray);
            Assert.Equal(expect, byteArrayString);
        }
    }
}