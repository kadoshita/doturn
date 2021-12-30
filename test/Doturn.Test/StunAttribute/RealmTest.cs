using System;
using Xunit;

namespace Doturn.StunAttribute.Test
{
    public class RealmTest
    {
        [Theory]
        [InlineData("example.com", "00-14-00-0B-65-78-61-6D-70-6C-65-2E-63-6F-6D-00")]
        [InlineData("turn.example.com", "00-14-00-10-74-75-72-6E-2E-65-78-61-6D-70-6C-65-2E-63-6F-6D-00-00-00-00")]
        public void Realm_Convert_To_ByteArray(string realm, string expect)
        {
            var realmObj = new Realm(realm);
            var byteArray = realmObj.ToByte();
            var byteArrayString = BitConverter.ToString(byteArray);
            Assert.Equal(expect, byteArrayString);
        }
    }
}