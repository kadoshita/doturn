using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class UsernameTest
    {
        [Theory]
        [InlineData("username", "00-06-00-08-75-73-65-72-6E-61-6D-65")]
        [InlineData("bob", "00-06-00-03-62-6F-62")]
        [InlineData("alice", "00-06-00-05-61-6C-69-63-65")]
        public void Username_Convert_To_ByteArray(string name, string expect)
        {
            var username = new Username(name);
            byte[] byteArray = username.ToBytes();
            string byteArrayString = BitConverter.ToString(byteArray);
            Assert.Equal(expect, byteArrayString);
        }

        [Fact]
        public void Throw_Exception_If_Username_Is_Empty()
        {
            Assert.Throws<UsernameIsEmptyException>(() => new Username(""));
        }
    }
}