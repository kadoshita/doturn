using System;
using Xunit;

namespace Doturn.StunMessage.Test;

public class TypeTest
{
    [Theory]
    [InlineData(Type.Binding, "00-01")]
    [InlineData(Type.BindingSuccess, "01-01")]
    [InlineData(Type.BindingError, "01-11")]
    [InlineData(Type.Allocate, "00-03")]
    [InlineData(Type.AllocateSuccess, "01-03")]
    [InlineData(Type.AllocateError, "01-13")]
    [InlineData(Type.Refresh, "00-04")]
    [InlineData(Type.RefreshSuccess, "01-04")]
    [InlineData(Type.RefreshError, "01-14")]
    [InlineData(Type.Send, "00-06")]
    [InlineData(Type.SendIndication, "00-16")]
    [InlineData(Type.Data, "00-07")]
    [InlineData(Type.DataIndication, "00-17")]
    [InlineData(Type.CreatePermission, "00-08")]
    [InlineData(Type.CreatePermissionSuccess, "01-08")]
    [InlineData(Type.CreatePermissionError, "01-18")]
    public void StunMessageType_Convert_To_ByteArray(Type type, string byteArrayString)
    {
        byte[] byteArray = type.ToBytes();
        Assert.Equal(2, byteArray.Length);
        Assert.Equal(byteArrayString, BitConverter.ToString(byteArray));
    }
}
