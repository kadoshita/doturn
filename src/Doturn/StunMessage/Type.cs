using System;

namespace Doturn.StunMessage
{
    public enum Type
    {
        BINDING = 0x0001,
        BINDING_SUCCESS = 0x0101,
        BINDING_ERROR = 0x0111,
        ALLOCATE = 0x0003,
        ALLOCATE_SUCCESS = 0x0103,
        ALLOCATE_ERROR = 0x0113,
        REFRESH = 0x0004,
        REFRESH_SUCCESS = 0x0104,
        REFRESH_ERROR = 0x0114,
        CREATE_PERMISSION = 0x0008,
        CREATE_PERMISSION_SUCCESS = 0x0108,
        CREATE_PERMISSION_ERROR = 0x0118,
    }
    public static class StunMessageTypeExtends
    {
        public static byte[] ToBytes(this Type stunMessageType)
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