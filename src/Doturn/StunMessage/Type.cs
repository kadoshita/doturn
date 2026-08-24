using System.Buffers.Binary;

namespace Doturn.StunMessage;

public enum Type : ushort
{
    Binding = 0x0001,
    BindingSuccess = 0x0101,
    BindingError = 0x0111,
    Allocate = 0x0003,
    AllocateSuccess = 0x0103,
    AllocateError = 0x0113,
    Refresh = 0x0004,
    RefreshSuccess = 0x0104,
    RefreshError = 0x0114,
    Send = 0x0006,
    SendIndication = 0x0016,
    Data = 0x0007,
    DataIndication = 0x0017,
    CreatePermission = 0x0008,
    CreatePermissionSuccess = 0x0108,
    CreatePermissionError = 0x0118,
    ChannelBind = 0x0009,
    ChannelBindSuccess = 0x0109,
    ChannelBindError = 0x0119
}
public static class StunMessageTypeExtensions
{
    public static byte[] ToBytes(this Type stunMessageType)
    {
        byte[] arr = new byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(arr, (ushort)stunMessageType);
        return arr;
    }
}
