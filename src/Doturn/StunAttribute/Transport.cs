namespace Doturn.StunAttribute;

public enum Transport
{
    UDP = 0x11,
    TCP = 0x06
}
public static class TransportExtends
{
    public static byte[] ToBytes(this Transport transport)
    {
        byte[] res = { (byte)transport };
        return res;
    }
}
