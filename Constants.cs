
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

    public enum StunAttr
    {
        MAPPED_ADDRESS = 0x0001,
        XOR_MAPPED_ADDRESS = 0x0020
    }
    public static class StunAttrExt
    {
        public static byte[] ToByte(this StunAttr stunAttr)
        {
            var arr = BitConverter.GetBytes((Int16)stunAttr);
            Array.Reverse(arr);
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