using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class NonceTest
    {
        [Theory]
        [InlineData("ho2ydw5qeeqsgasz", "00-15-00-10-68-6F-32-79-64-77-35-71-65-65-71-73-67-61-73-7A")]
        [InlineData("nfbd05c6kaas8oxa", "00-15-00-10-6E-66-62-64-30-35-63-36-6B-61-61-73-38-6F-78-61")]
        public void Nonce_Convert_To_ByteArray(string nonce, string expect)
        {
            var nonceObj = new Nonce(nonce);
            var byteArray = nonceObj.ToBytes();
            var byteArrayString = BitConverter.ToString(byteArray);
            Assert.Equal(expect, byteArrayString);
        }

        [Fact]
        public void Random_Nonce_Convert_To_ByteArray()
        {
            var nonceObj = new Nonce();
            var byteArray = nonceObj.ToBytes();
            var byteArrayString = BitConverter.ToString(byteArray);
            Assert.Equal(20, byteArray.Length);
            Assert.StartsWith("00-15-00-10", byteArrayString);
        }
    }
}