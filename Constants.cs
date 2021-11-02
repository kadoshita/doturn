
using System;

namespace doturn
{
    public enum StunMessage
    {
        BINDING = 0x0001,
        BINDING_SUCCESS = 0x0101,
        ALLOCATE = 0x0003,
        ALLOCATE_SUCCESS = 0x0103,
        ALLOCATE_ERROR = 0x0113,
    }
    public static class StunMessageExt
    {
        public static byte[] ToByte(this StunMessage stunMessage)
        {
            var arr = BitConverter.GetBytes((Int16)stunMessage);
            Array.Reverse(arr);
            return arr;
        }
    }

    public enum StunAttrType
    {
        MAPPED_ADDRESS = 0x0001,
        ERROR_CODE = 0x0009,
        REALM = 0x0014,
        NONCE = 0x0015,
        REQUESTED_TRANSPORT = 0x0019,
        XOR_MAPPED_ADDRESS = 0x0020,
        SOFTWARE = 0x8022,
        ALTERNATE_SERVER = 0x8023,
        FINGERPRINT = 0x8028
    }
    public static class StunAttrExt
    {
        public static byte[] ToByte(this StunAttrType stunAttr)
        {
            var arr = BitConverter.GetBytes((Int16)stunAttr);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(arr);
            }
            return arr;
        }
    }

    public enum Transport
    {
        UDP = 0x11,
        TCP = 0x06
    }
    public static class TransportExt
    {
        public static byte[] ToByte(this Transport transport)
        {
            var arr = BitConverter.GetBytes((char)transport);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(arr);
            }
            return arr;
        }
    }

    public enum Stun
    {
        MAGIC_COOKIE = 0x2112a442
    }
    public static class StunExt
    {
        public static byte[] ToByte(this Stun stun)
        {
            var arr = BitConverter.GetBytes((Int32)stun);
            Array.Reverse(arr);
            return arr;
        }
    }
}