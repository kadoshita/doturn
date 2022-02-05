using System;

namespace Doturn.StunAttribute
{
    public enum Type
    {
        MAPPED_ADDRESS = 0x0001,
        USERNAME = 0x0006,
        MESSAGE_INTEGRITY = 0x0008,
        ERROR_CODE = 0x0009,
        CHANNEL_NUMBER = 0x000C,
        LIFETIME = 0x000D,
        XOR_PEER_ADDRESS = 0x0012,
        DATA = 0x0013,
        REALM = 0x0014,
        NONCE = 0x0015,
        XOR_RELAYED_ADDRESS = 0x0016,
        REQUESTED_TRANSPORT = 0x0019,
        XOR_MAPPED_ADDRESS = 0x0020,
        SOFTWARE = 0x8022,
        ALTERNATE_SERVER = 0x8023,
        FINGERPRINT = 0x8028
    }
    public static class StunAttributeTypeExtends
    {
        public static byte[] ToBytes(this Type stunAttributeType)
        {
            byte[] arr = BitConverter.GetBytes((short)stunAttributeType);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(arr);
            }
            return arr;
        }
    }
}