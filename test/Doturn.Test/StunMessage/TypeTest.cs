using System;
using Xunit;

namespace Doturn.StunMessage.Test
{
    public class TypeTest
    {
        [Theory]
        [InlineData(Type.BINDING, "00-01")]
        [InlineData(Type.BINDING_SUCCESS, "01-01")]
        [InlineData(Type.BINDING_ERROR, "01-11")]
        [InlineData(Type.ALLOCATE, "00-03")]
        [InlineData(Type.ALLOCATE_SUCCESS, "01-03")]
        [InlineData(Type.ALLOCATE_ERROR, "01-13")]
        [InlineData(Type.REFRESH, "00-04")]
        [InlineData(Type.REFRESH_SUCCESS, "01-04")]
        [InlineData(Type.REFRESH_ERROR, "01-14")]
        [InlineData(Type.SEND, "00-06")]
        [InlineData(Type.SEND_INDICATION, "00-16")]
        [InlineData(Type.DATA, "00-07")]
        [InlineData(Type.DATA_INDICATION, "00-17")]
        [InlineData(Type.CREATE_PERMISSION, "00-08")]
        [InlineData(Type.CREATE_PERMISSION_SUCCESS, "01-08")]
        [InlineData(Type.CREATE_PERMISSION_ERROR, "01-18")]
        public void StunMessageType_Convert_To_ByteArray(Type type, string byteArrayString)
        {
            byte[] byteArray = type.ToBytes();
            Assert.Equal(2, byteArray.Length);
            Assert.Equal(byteArrayString, BitConverter.ToString(byteArray));
        }
    }
}