using System;
using System.Collections.Generic;
using System.Net;

namespace Doturn.StunAttribute
{
    public static class StunAttributeParser
    {
        public static List<IStunAttribute> Parse(byte[] data)
        {
            var attributes = new List<IStunAttribute>();
            var endPos = 0;
            for (; data.Length > endPos;)
            {
                var attrTypeByteArray = data[(endPos)..(2 + endPos)];
                endPos += attrTypeByteArray.Length;
                var attrLengthByteArray = data[endPos..(2 + endPos)];
                endPos += attrLengthByteArray.Length;
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(attrTypeByteArray);
                    Array.Reverse(attrLengthByteArray);
                }
                var attrType = (Type)Enum.ToObject(typeof(Type), BitConverter.ToUInt16(attrTypeByteArray));
                var attrLength = BitConverter.ToUInt16(attrLengthByteArray);

                if (attrType == Type.REQUESTED_TRANSPORT)
                {
                    var transportByteArray = data[endPos..(attrLength + endPos)];
                    endPos += transportByteArray.Length;
                    var requestedTransport = RequestedTransport.Parse(transportByteArray);
                    attributes.Add(requestedTransport);
                }
                else if (attrType == Type.USERNAME)
                {
                    var usernameByteArray = data[endPos..(attrLength + endPos)];
                    endPos += usernameByteArray.Length;
                    var username = Username.Parse(usernameByteArray);
                    attributes.Add(username);
                }
                else if (attrType == Type.REALM)
                {
                    var realmByteArray = data[endPos..(attrLength + endPos)];
                    endPos += realmByteArray.Length;
                    var paddingLength = 8 - ((2 + 2 + attrLength) % 8);
                    endPos += paddingLength;
                    var realm = Realm.Parse(realmByteArray);
                    attributes.Add(realm);
                }
                else if (attrType == Type.NONCE)
                {
                    var nonceByteArray = data[endPos..(attrLength + endPos)];
                    endPos += nonceByteArray.Length;
                    var nonce = Nonce.Parse(nonceByteArray);
                    attributes.Add(nonce);
                }
                else if (attrType == Type.MESSAGE_INTEGRITY)
                {
                    var messageIntegrityByteArray = data[endPos..(attrLength + endPos)];
                    endPos += messageIntegrityByteArray.Length;
                    var messageIntegrity = MessageIntegrity.Parse(messageIntegrityByteArray);
                    attributes.Add(messageIntegrity);
                }
                else if (attrType == Type.ERROR_CODE)
                {
                    var errorCodeByteArray = data[endPos..(attrLength + endPos)];
                    endPos += errorCodeByteArray.Length;
                    var errorCode = ErrorCode.Parse(errorCodeByteArray);
                    attributes.Add(errorCode);

                }
                else if (attrType == Type.FINGERPRINT)
                {
                    var fingerprintByteArray = data[endPos..(attrLength + endPos)];
                    endPos += fingerprintByteArray.Length;
                    var fingerprint = Fingerprint.Parse(fingerprintByteArray);
                    attributes.Add(fingerprint);
                }
                else if (attrType == Type.LIFETIME)
                {
                    var lifetimeByteArray = data[endPos..(attrLength + endPos)];
                    endPos += lifetimeByteArray.Length;
                    var lifetime = Lifetime.Parse(lifetimeByteArray);
                    attributes.Add(lifetime);
                }
                else if (attrType == Type.MAPPED_ADDRESS)
                {
                    var mappedAddressByteArray = data[endPos..(attrLength + endPos)];
                    endPos += mappedAddressByteArray.Length;
                    var mappedAddress = MappedAddress.Parse(mappedAddressByteArray);
                    attributes.Add(mappedAddress);
                }
                else if (attrType == Type.SOFTWARE)
                {
                    var softwareByteArray = data[endPos..(attrLength + endPos)];
                    endPos += softwareByteArray.Length;
                    var software = Software.Parse(softwareByteArray);
                    attributes.Add(software);
                }
                else if (attrType == Type.XOR_MAPPED_ADDRESS)
                {
                    var xorMappedAddressByteArray = data[endPos..(attrLength + endPos)];
                    endPos += xorMappedAddressByteArray.Length;
                    var xorMappedAddress = XorMappedAddress.Parse(xorMappedAddressByteArray);
                    attributes.Add(xorMappedAddress);
                }
                else if (attrType == Type.XOR_PEER_ADDRESS)
                {
                    var xorPeerAddressByteArray = data[endPos..(attrLength + endPos)];
                    endPos += xorPeerAddressByteArray.Length;
                    var xorPeerAddress = XorPeerAddress.Parse(xorPeerAddressByteArray);
                    attributes.Add(xorPeerAddress);
                }
                else if (attrType == Type.XOR_RELAYED_ADDRESS)
                {
                    var xorRelayedAddressByteArray = data[endPos..(attrLength + endPos)];
                    endPos += xorRelayedAddressByteArray.Length;
                    var xorRelayedAddress = XorRelayedAddress.Parse(xorRelayedAddressByteArray);
                    attributes.Add(xorRelayedAddress);
                }
                else
                {
                    break;
                }
            }

            return attributes;
        }
    }
}