using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class SoftwareTest
    {
        [Theory]
        [InlineData("None", "80-22-00-04-4E-6F-6E-65-00-00-00-00")]
        [InlineData("Doturn", "80-22-00-06-44-6F-74-75-72-6E-00-00")]
        [InlineData("Doturn TURN Server", "80-22-00-12-44-6F-74-75-72-6E-20-54-55-52-4E-20-53-65-72-76-65-72-00-00-00-00-00-00")]
        public void software_Convert_To_ByteArray(string software, string expect)
        {
            var softwareObj = new Software(software);
            var byteArray = softwareObj.ToBytes();
            var byteArrayString = BitConverter.ToString(byteArray);
            Assert.Equal(expect, byteArrayString);
        }
    }
}