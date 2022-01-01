using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class FingerprintTest
    {
        [Theory]
        [InlineData(new byte[] { 0x00 }, "80-28-00-04-81-56-BA-C3")]
        [InlineData(new byte[] { 0x10, 0xfa, 0x49, 0xff }, "80-28-00-04-96-E3-30-0A")]
        public void Fingerprint_Set_ByteArray_In_Constructor(byte[] data, string expect)
        {
            var fingerprint = Fingerprint.CreateFingerprint(ref data);
            var byteArray = fingerprint.ToBytes();
            var byteArrayString = BitConverter.ToString(byteArray);
            Assert.Equal(expect, byteArrayString);
        }
    }
}