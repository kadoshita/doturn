using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace Doturn.StunAttribute;

public static class StunAttributeParser
{
    public static List<IStunAttribute> Parse(byte[] data) => Parse(data.AsSpan());

    public static List<IStunAttribute> Parse(ReadOnlySpan<byte> data)
    {
        var attributes = new List<IStunAttribute>();
        int endPos = 0;
        while (data.Length > endPos + 4)
        {
            var attrType = (Type)BinaryPrimitives.ReadUInt16BigEndian(data.Slice(endPos, 2));
            endPos += 2;
            ushort attrLength = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(endPos, 2));
            endPos += 2;

            if (endPos + attrLength > data.Length)
            {
                break;
            }

            byte[] valueBytes = data.Slice(endPos, attrLength).ToArray();
            endPos += attrLength;

            switch (attrType)
            {
                case Type.RequestedTransport:
                    attributes.Add(RequestedTransport.Parse(valueBytes));
                    break;
                case Type.Username:
                    attributes.Add(Username.Parse(valueBytes));
                    break;
                case Type.Realm:
                    endPos += PaddingLength(attrLength);
                    attributes.Add(Realm.Parse(valueBytes));
                    break;
                case Type.Nonce:
                    attributes.Add(Nonce.Parse(valueBytes));
                    break;
                case Type.MessageIntegrity:
                    attributes.Add(MessageIntegrity.Parse(valueBytes));
                    break;
                case Type.ErrorCode:
                    attributes.Add(ErrorCode.Parse(valueBytes));
                    break;
                case Type.Fingerprint:
                    attributes.Add(Fingerprint.Parse(valueBytes));
                    break;
                case Type.Lifetime:
                    attributes.Add(Lifetime.Parse(valueBytes));
                    break;
                case Type.MappedAddress:
                    attributes.Add(MappedAddress.Parse(valueBytes));
                    break;
                case Type.Software:
                    attributes.Add(Software.Parse(valueBytes));
                    break;
                case Type.XorMappedAddress:
                    attributes.Add(XorMappedAddress.Parse(valueBytes));
                    break;
                case Type.XorPeerAddress:
                    attributes.Add(XorPeerAddress.Parse(valueBytes));
                    break;
                case Type.XorRelayedAddress:
                    attributes.Add(XorRelayedAddress.Parse(valueBytes));
                    break;
                case Type.Data:
                    endPos += PaddingLength(attrLength);
                    attributes.Add(Data.Parse(valueBytes));
                    break;
                case Type.ChannelNumber:
                    attributes.Add(ChannelNumber.Parse(valueBytes));
                    break;
                default:
                    return attributes;
            }
        }

        return attributes;
    }

    private static int PaddingLength(ushort attrLength)
    {
        int padding = 8 - ((2 + 2 + attrLength) % 8);
        return padding == 8 ? 0 : padding;
    }
}
