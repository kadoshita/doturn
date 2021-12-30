using System;

namespace Doturn.StunAttribute
{
    public class UsernameIsEmptyException : Exception
    {
        public UsernameIsEmptyException() : base() { }
    }
    public class Username : StunAttributeBase
    {
        public readonly Type type = Type.USERNAME;
        public readonly string username;
        public override Type Type => this.type;

        public Username(string username)
        {
            if (String.IsNullOrEmpty(username))
            {
                throw new UsernameIsEmptyException();
            }
            this.username = username;
        }

        public override byte[] ToByte()
        {
            var typeByteArray = this.type.ToByte();
            byte[] usernameByteArray = System.Text.Encoding.ASCII.GetBytes(this.username);
            var length = usernameByteArray.Length;
            var lengthByteArray = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }
            var res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, usernameByteArray);
            return res;
        }
    }
}