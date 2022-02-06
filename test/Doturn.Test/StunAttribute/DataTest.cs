using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class DataTest
    {
        [Fact]
        public void Data_Convert_To_ByteArray()
        {
            var dataObj = new Data(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            byte[] byteArray = dataObj.ToBytes();
            byte[] expect = new byte[] { 0x00, 0x13, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00 };
            Assert.Equal(expect, byteArray);
        }
    }
}