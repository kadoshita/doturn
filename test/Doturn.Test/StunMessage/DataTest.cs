using System;
using System.Net;
using Doturn.StunAttribute;
using Xunit;

namespace Doturn.StunMessage.Test;

public class DataTest
{
    private readonly byte[] _payload = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

    // DATA attribute TLV: type 0x0013, length 8, value = _payload (already 8-byte aligned, no padding)
    private readonly byte[] _expectedDataAttributeByteArray = new byte[] {
        0x00, 0x13, 0x00, 0x08, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
    };

    [Fact]
    public void Convert_To_ByteArray_Data()
    {
        var data = new Data(_payload);
        byte[] result = data.ToBytes();
        Assert.Equal(_expectedDataAttributeByteArray, result);
    }

    [Fact]
    public void CreateDataIndication_Has_Deterministic_Header_And_Body()
    {
        var data = new Data(_payload);
        var peer = new IPEndPoint(IPAddress.Loopback, 20000);

        byte[] result = data.CreateDataIndication(peer);

        byte[] xorPeerAddressBytes = new XorPeerAddress(peer).ToBytes();
        int bodyLength = _expectedDataAttributeByteArray.Length + xorPeerAddressBytes.Length;
        Assert.Equal(20 + bodyLength + 8, result.Length);

        // Type: DATA_INDICATION (0x0017)
        Assert.Equal(new byte[] { 0x00, 0x17 }, result[0..2]);

        // Message length = body + fingerprint(8)
        byte[] messageLengthBytes = result[2..4];
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(messageLengthBytes);
        }
        Assert.Equal((short)(bodyLength + 8), BitConverter.ToInt16(messageLengthBytes));

        // Magic cookie
        Assert.Equal(new byte[] { 0x21, 0x12, 0xA4, 0x42 }, result[4..8]);

        // Body: DATA attribute followed by XorPeerAddress attribute
        byte[] expectedBody = new byte[bodyLength];
        ByteArrayUtils.MergeByteArray(ref expectedBody, 0, _expectedDataAttributeByteArray, xorPeerAddressBytes);
        Assert.Equal(expectedBody, result[20..(20 + bodyLength)]);

        // CreateDataIndication computes the fingerprint over the response buffer before it is
        // populated, so it is always over a zero-filled buffer of this length (existing
        // behavior; not something this test is trying to fix).
        byte[] zeroFilled = new byte[result.Length - 8];
        byte[] expectedFingerprint = Fingerprint.CreateFingerprint(zeroFilled).ToBytes();
        Assert.Equal(expectedFingerprint, result[^8..]);
    }
}
