using System;

namespace Doturn.StunMessage
{
    public enum Type
    {
        BINDING = 0x0001,
        BINDING_SUCCESS = 0x0101,
        ALLOCATE = 0x0003,
        REFRESH = 0x0004,
        CREATE_PERMISSION = 0x0008,
        REFRESH_SUCCESS = 0x0104,
        ALLOCATE_SUCCESS = 0x0103,
        CREATE_PERMISSION_SUCCESS = 0x0108,
        ALLOCATE_ERROR = 0x0113,
    }
    public static class StunMessageTypeExtends
    {
        public static byte[] ToByte(this Type stunMessageType)
        {
            var arr = BitConverter.GetBytes((Int16)stunMessageType);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(arr);
            }
            return arr;
        }
    }
}