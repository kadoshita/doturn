using System;
using System.Buffers.Binary;
using Force.Crc32;

namespace Doturn.StunAttribute;

public class Fingerprint : StunAttributeBase
{
    private const uint FingerprintXor = 0x5354554E;

    private readonly byte[] _crc32;
    public override Type Type => Type.Fingerprint;

    public Fingerprint(byte[] crc32)
    {
        _crc32 = crc32;
    }
    public static Fingerprint CreateFingerprint(byte[] data)
    {
        uint crc32 = Crc32Algorithm.Compute(data, 0, data.Length);
        byte[] crc32XorByte = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crc32XorByte, crc32 ^ FingerprintXor);
        return new Fingerprint(crc32XorByte);
    }
    public override byte[] ToBytes()
    {
        int length = _crc32.Length;
        byte[] res = new byte[2 + 2 + length];
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(0, 2), (ushort)Type);
        BinaryPrimitives.WriteUInt16BigEndian(res.AsSpan(2, 2), (ushort)length);
        _crc32.CopyTo(res, 4);
        return res;
    }

    public static Fingerprint Parse(byte[] data)
    {
        return new Fingerprint(data);
    }
}
