
using System;

namespace Doturn.StunAttribute
{
    public enum Transport
    {
        UDP = 0x11,
        TCP = 0x06
    }
    public static class TransportExtends
    {
        public static byte[] ToByte(this Transport transport)
        {
            byte[] res = { BitConverter.GetBytes((byte)transport)[0] };
            return res;
        }
    }
}