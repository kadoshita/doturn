using System;

namespace Doturn;

public static class ByteArrayUtils
{
    private static readonly byte[] s_magicCookie = new byte[] { 0x21, 0x12, 0xA4, 0x42 };

    public static ReadOnlySpan<byte> MagicCookie => s_magicCookie;

    public static void MergeByteArray(ref byte[] res, params byte[][] data)
    {
        MergeByteArray(res.AsSpan(), 0, data);
    }
    public static void MergeByteArray(ref byte[] res, int offset, params byte[][] data)
    {
        MergeByteArray(res.AsSpan(), offset, data);
    }

    private static void MergeByteArray(Span<byte> destination, int offset, byte[][] parts)
    {
        int endPos = offset;
        foreach (byte[] datum in parts)
        {
            datum.CopyTo(destination[endPos..]);
            endPos += datum.Length;
        }
    }

    public static byte[] XorPort(byte[] portByteArray)
    {
        byte[] xorPortByteArray = new byte[portByteArray.Length];
        XorPort(portByteArray, xorPortByteArray);
        return xorPortByteArray;
    }

    public static void XorPort(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        for (int i = 0; i < source.Length; i++)
        {
            destination[i] = (byte)(source[i] ^ s_magicCookie[i]);
        }
    }

    public static byte[] XorAddress(byte[] addressByteArray)
    {
        byte[] xorAddressByteArray = new byte[addressByteArray.Length];
        XorAddress(addressByteArray, xorAddressByteArray);
        return xorAddressByteArray;
    }

    public static void XorAddress(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        for (int i = 0; i < source.Length; i++)
        {
            destination[i] = (byte)(source[i] ^ s_magicCookie[i]);
        }
    }
}
