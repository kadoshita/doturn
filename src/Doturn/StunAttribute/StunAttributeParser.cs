using System;
using System.Collections.Generic;

namespace Doturn.StunAttribute;

public static class StunAttributeParser
{
    public static List<IStunAttribute> Parse(byte[] data)
    {
        var attributes = new List<IStunAttribute>();
        int endPos = 0;
        for (; data.Length > endPos;)
        {
            byte[] attrTypeByteArray = data[endPos..(2 + endPos)];
            endPos += attrTypeByteArray.Length;
            byte[] attrLengthByteArray = data[endPos..(2 + endPos)];
            endPos += attrLengthByteArray.Length;
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(attrTypeByteArray);
                Array.Reverse(attrLengthByteArray);
            }
            var attrType = (Type)Enum.ToObject(typeof(Type), BitConverter.ToUInt16(attrTypeByteArray));
            ushort attrLength = BitConverter.ToUInt16(attrLengthByteArray);

            if (attrType == Type.RequestedTransport)
            {
                byte[] transportByteArray = data[endPos..(attrLength + endPos)];
                endPos += transportByteArray.Length;
                var requestedTransport = RequestedTransport.Parse(transportByteArray);
                attributes.Add(requestedTransport);
            }
            else if (attrType == Type.Username)
            {
                byte[] usernameByteArray = data[endPos..(attrLength + endPos)];
                endPos += usernameByteArray.Length;
                var username = Username.Parse(usernameByteArray);
                attributes.Add(username);
            }
            else if (attrType == Type.Realm)
            {
                byte[] realmByteArray = data[endPos..(attrLength + endPos)];
                endPos += realmByteArray.Length;
                int paddingLength = 8 - ((2 + 2 + attrLength) % 8);
                endPos += paddingLength;
                var realm = Realm.Parse(realmByteArray);
                attributes.Add(realm);
            }
            else if (attrType == Type.Nonce)
            {
                byte[] nonceByteArray = data[endPos..(attrLength + endPos)];
                endPos += nonceByteArray.Length;
                var nonce = Nonce.Parse(nonceByteArray);
                attributes.Add(nonce);
            }
            else if (attrType == Type.MessageIntegrity)
            {
                byte[] messageIntegrityByteArray = data[endPos..(attrLength + endPos)];
                endPos += messageIntegrityByteArray.Length;
                var messageIntegrity = MessageIntegrity.Parse(messageIntegrityByteArray);
                attributes.Add(messageIntegrity);
            }
            else if (attrType == Type.ErrorCode)
            {
                byte[] errorCodeByteArray = data[endPos..(attrLength + endPos)];
                endPos += errorCodeByteArray.Length;
                var errorCode = ErrorCode.Parse(errorCodeByteArray);
                attributes.Add(errorCode);

            }
            else if (attrType == Type.Fingerprint)
            {
                byte[] fingerprintByteArray = data[endPos..(attrLength + endPos)];
                endPos += fingerprintByteArray.Length;
                var fingerprint = Fingerprint.Parse(fingerprintByteArray);
                attributes.Add(fingerprint);
            }
            else if (attrType == Type.Lifetime)
            {
                byte[] lifetimeByteArray = data[endPos..(attrLength + endPos)];
                endPos += lifetimeByteArray.Length;
                var lifetime = Lifetime.Parse(lifetimeByteArray);
                attributes.Add(lifetime);
            }
            else if (attrType == Type.MappedAddress)
            {
                byte[] mappedAddressByteArray = data[endPos..(attrLength + endPos)];
                endPos += mappedAddressByteArray.Length;
                IStunAttribute mappedAddress = MappedAddress.Parse(mappedAddressByteArray);
                attributes.Add(mappedAddress);
            }
            else if (attrType == Type.Software)
            {
                byte[] softwareByteArray = data[endPos..(attrLength + endPos)];
                endPos += softwareByteArray.Length;
                var software = Software.Parse(softwareByteArray);
                attributes.Add(software);
            }
            else if (attrType == Type.XorMappedAddress)
            {
                byte[] xorMappedAddressByteArray = data[endPos..(attrLength + endPos)];
                endPos += xorMappedAddressByteArray.Length;
                var xorMappedAddress = XorMappedAddress.Parse(xorMappedAddressByteArray);
                attributes.Add(xorMappedAddress);
            }
            else if (attrType == Type.XorPeerAddress)
            {
                byte[] xorPeerAddressByteArray = data[endPos..(attrLength + endPos)];
                endPos += xorPeerAddressByteArray.Length;
                var xorPeerAddress = XorPeerAddress.Parse(xorPeerAddressByteArray);
                attributes.Add(xorPeerAddress);
            }
            else if (attrType == Type.XorRelayedAddress)
            {
                byte[] xorRelayedAddressByteArray = data[endPos..(attrLength + endPos)];
                endPos += xorRelayedAddressByteArray.Length;
                var xorRelayedAddress = XorRelayedAddress.Parse(xorRelayedAddressByteArray);
                attributes.Add(xorRelayedAddress);
            }
            else if (attrType == Type.Data)
            {
                byte[] dataByteArray = data[endPos..(attrLength + endPos)];
                endPos += dataByteArray.Length;
                int paddingLength = 8 - ((2 + 2 + attrLength) % 8);
                endPos += paddingLength;
                var dataAttribute = Data.Parse(dataByteArray);
                attributes.Add(dataAttribute);
            }
            else if (attrType == Type.ChannelNumber)
            {
                byte[] channelNumberByteArray = data[endPos..(attrLength + endPos)];
                endPos += channelNumberByteArray.Length;
                var channelNumber = ChannelNumber.Parse(channelNumberByteArray);
                attributes.Add(channelNumber);
            }
            else
            {
                break;
            }
        }

        return attributes;
    }
}
