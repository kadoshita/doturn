namespace Doturn.StunAttribute;

public enum Transport : byte
{
    Udp = 0x11,
    Tcp = 0x06
}
public static class TransportExtensions
{
    public static byte[] ToBytes(this Transport transport)
    {
        byte[] res = { (byte)transport };
        return res;
    }
}
