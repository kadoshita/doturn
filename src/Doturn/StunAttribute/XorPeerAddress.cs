using System;
using System.Net;

namespace Doturn.StunAttribute
{
    public class XorPeerAddress : StunAttributeBase
    {
        public readonly Type type = Type.XOR_PEER_ADDRESS;
        public readonly IPEndPoint endpoint;
        public readonly IPEndPoint xorEndpoint;
        public override Type Type => this.type;
        public XorPeerAddress(IPAddress xorAddress, UInt16 xorPort)
        {
            var _xorEndpoint = new IPEndPoint(xorAddress, xorPort);
            this.xorEndpoint = _xorEndpoint;
            this.endpoint = this.xorEndpointToEndpoint(_xorEndpoint);
        }
        public XorPeerAddress(string xorAddress, UInt16 xorPort)
        {
            var _xorEndpoint = new IPEndPoint(IPAddress.Parse(xorAddress), xorPort);
            this.xorEndpoint = _xorEndpoint;
            this.endpoint = this.xorEndpointToEndpoint(_xorEndpoint);
        }
        public XorPeerAddress(IPEndPoint xorEndpoint)
        {
            this.xorEndpoint = xorEndpoint;
            this.endpoint = this.xorEndpointToEndpoint(xorEndpoint);
        }

        private IPEndPoint xorEndpointToEndpoint(IPEndPoint xorEndpoint)
        {
            var xorAddressByteArray = xorEndpoint.Address.GetAddressBytes();
            var xorPortByteArray = BitConverter.GetBytes((UInt16)xorEndpoint.Port);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(xorPortByteArray);
            }
            var addressByteArray = ByteArrayUtils.XorAddress(xorAddressByteArray);
            var portByteArray = ByteArrayUtils.XorPort(xorPortByteArray);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(portByteArray);
            }
            return new IPEndPoint(new IPAddress(addressByteArray), BitConverter.ToUInt16(portByteArray));
        }

        public override byte[] ToBytes()
        {
            var typeByteArray = this.type.ToBytes();
            var xorAddressByteArray = this.xorEndpoint.Address.GetAddressBytes();
            var xorPortByteArray = BitConverter.GetBytes((UInt16)this.xorEndpoint.Port);

            byte[] reserved = { 0x00 };
            byte[] addressFamilyByte = { 0x01 };
            var length = reserved.Length + addressFamilyByte.Length + xorPortByteArray.Length + xorAddressByteArray.Length;
            var lengthByteArray = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }
            var res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, reserved, addressFamilyByte, xorPortByteArray, xorAddressByteArray);
            return res;
        }

        public static XorPeerAddress Parse(byte[] data)
        {
            var reservedByteArray = data[0..1];
            var protocolFamilyByteArray = data[1..2];
            var portByteArray = data[2..4];
            var addressByteArray = data[4..data.Length];
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(portByteArray);
            }
            var port = BitConverter.ToUInt16(portByteArray);
            var address = new IPAddress(addressByteArray);
            return new XorPeerAddress(address, port);
        }
    }
}