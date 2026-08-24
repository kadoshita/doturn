using System.Buffers.Binary;

namespace Doturn.StunAttribute;

public enum Type : ushort
{
    MappedAddress = 0x0001,
    Username = 0x0006,
    MessageIntegrity = 0x0008,
    ErrorCode = 0x0009,
    ChannelNumber = 0x000C,
    Lifetime = 0x000D,
    XorPeerAddress = 0x0012,
    Data = 0x0013,
    Realm = 0x0014,
    Nonce = 0x0015,
    XorRelayedAddress = 0x0016,
    RequestedTransport = 0x0019,
    XorMappedAddress = 0x0020,
    Software = 0x8022,
    AlternateServer = 0x8023,
    Fingerprint = 0x8028
}
public static class StunAttributeTypeExtensions
{
    public static byte[] ToBytes(this Type stunAttributeType)
    {
        byte[] arr = new byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(arr, (ushort)stunAttributeType);
        return arr;
    }
}
