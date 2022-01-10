using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class LifetimeTest
    {
        [Theory]
        [InlineData(600, "00-0D-00-04-00-00-02-58")]
        [InlineData(3600, "00-0D-00-04-00-00-0E-10")]
        [InlineData(86400, "00-0D-00-04-00-01-51-80")]
        public void Lifetime_Convert_To_ByteArray(int lifetime, string expect)
        {
            var lifetimeObj = new Lifetime(lifetime);
            var byteArray = lifetimeObj.ToBytes();
            var byteArrayString = BitConverter.ToString(byteArray);
            Assert.Equal(expect, byteArrayString);
        }
    }
}