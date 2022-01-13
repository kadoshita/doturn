using System;
using Xunit;

namespace Doturn.StunMessage.Test
{
    public class TypeTest
    {
        [Theory]
        [InlineData(StunMessage.Type.BINDING, "00-01")]
        [InlineData(StunMessage.Type.BINDING_SUCCESS, "01-01")]
        [InlineData(StunMessage.Type.BINDING_ERROR, "01-11")]
        [InlineData(StunMessage.Type.ALLOCATE, "00-03")]
        [InlineData(StunMessage.Type.ALLOCATE_SUCCESS, "01-03")]
        [InlineData(StunMessage.Type.ALLOCATE_ERROR, "01-13")]
        [InlineData(StunMessage.Type.REFRESH, "00-04")]
        [InlineData(StunMessage.Type.REFRESH_SUCCESS, "01-04")]
        [InlineData(StunMessage.Type.REFRESH_ERROR, "01-14")]
        [InlineData(StunMessage.Type.CREATE_PERMISSION, "00-08")]
        [InlineData(StunMessage.Type.CREATE_PERMISSION_SUCCESS, "01-08")]
        [InlineData(StunMessage.Type.CREATE_PERMISSION_ERROR, "01-18")]
        public void StunMessageType_Convert_To_ByteArray(StunMessage.Type type, string byteArrayString)
        {
            byte[] byteArray = type.ToBytes();
            Assert.Equal(2, byteArray.Length);
            Assert.Equal(byteArrayString, BitConverter.ToString(byteArray));
        }
    }
}