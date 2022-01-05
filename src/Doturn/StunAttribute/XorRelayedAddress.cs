using System;
using System.Net;

namespace Doturn.StunAttribute
{
    public class XorRelayedAddress : StunAttributeBase
    {
        public readonly Type type = Type.XOR_RELAYED_ADDRESS;
        public readonly IPEndPoint endpoint;
        public readonly IPEndPoint xorEndpoint;
        public override Type Type => this.type;
        public XorRelayedAddress(IPAddress address, UInt16 port)
        {
            this.endpoint = new IPEndPoint(address, port);
            this.xorEndpoint = this.endpointXor(address.ToString(), port);
        }
        public XorRelayedAddress(string address, UInt16 port)
        {
            this.endpoint = new IPEndPoint(IPAddress.Parse(address), port);
            this.xorEndpoint = this.endpointXor(address, port);
        }
        public XorRelayedAddress(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;
            this.xorEndpoint = this.endpointXor(endpoint.Address.ToString(), (UInt16)endpoint.Port);
        }
        public XorRelayedAddress(byte[] xorAddressByteArray, byte[] xorPortByteArray)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(xorPortByteArray);
            }
            var xorAddress = new IPAddress(xorAddressByteArray);
            var xorPort = BitConverter.ToUInt16(xorPortByteArray);
            var xorEndpoint = new IPEndPoint(xorAddress, xorPort);
            this.xorEndpoint = xorEndpoint;
            this.endpoint = endpointXor(xorAddress.ToString(), xorPort);
        }
        private IPEndPoint endpointXor(string address, UInt16 port)
        {
            var addressByteArray = IPAddress.Parse(address).GetAddressBytes();
            var portByteArray = BitConverter.GetBytes((UInt16)port);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(portByteArray);
            }
            var xorAddressByteArray = ByteArrayUtils.XorAddress(addressByteArray);
            var xorPortByteArray = ByteArrayUtils.XorPort(portByteArray);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(xorPortByteArray);
            }
            return new IPEndPoint(new IPAddress(xorAddressByteArray), BitConverter.ToUInt16(xorPortByteArray));
        }

        public override byte[] ToBytes()
        {
            var typeByteArray = this.type.ToBytes();
            var xorAddressByteArray = this.xorEndpoint.Address.GetAddressBytes();
            var xorPortByteArray = BitConverter.GetBytes((Int16)this.xorEndpoint.Port);

            byte[] reserved = { 0x00 };
            byte[] addressFamilyByte = { 0x01 };
            var length = reserved.Length + addressFamilyByte.Length + xorPortByteArray.Length + xorAddressByteArray.Length;
            var lengthByteArray = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(xorPortByteArray);
                Array.Reverse(lengthByteArray);
            }
            var res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, reserved, addressFamilyByte, xorPortByteArray, xorAddressByteArray);
            return res;
        }

        public static XorRelayedAddress Parse(byte[] data)
        {
            var reservedByteArray = data[0..1];
            var protocolFamilyByteArray = data[1..2];
            var portByteArray = data[2..4];
            var addressByteArray = data[4..data.Length];

            return new XorRelayedAddress(addressByteArray, portByteArray);
        }
    }
}