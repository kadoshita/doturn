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
        public override Type Type => type;

        public Username(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new UsernameIsEmptyException();
            }
            this.username = username;
        }

        public override byte[] ToBytes()
        {
            byte[] typeByteArray = type.ToBytes();
            byte[] usernameByteArray = System.Text.Encoding.ASCII.GetBytes(username);
            int length = usernameByteArray.Length;
            byte[] lengthByteArray = BitConverter.GetBytes((short)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }
            byte[] res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, usernameByteArray);
            return res;
        }
        public static Username Parse(byte[] data)
        {
            string usernameStr = System.Text.Encoding.ASCII.GetString(data);
            return new Username(usernameStr);
        }
    }
}