using System;
using System.Net;

namespace Doturn.StunAttribute
{
    public class XorMappedAddress : StunAttributeBase
    {
        public readonly Type type = Type.XOR_MAPPED_ADDRESS;
        public readonly IPEndPoint endpoint;
        public readonly IPEndPoint realEndpoint;
        public override Type Type => this.type;
        /// <summary>
        /// Create XorMappedAddress from IP Address and Port
        /// </summary>
        /// <param name="address">XOR address</param>
        /// <param name="port">XOR port</param>
        public XorMappedAddress(IPAddress address, UInt16 port)
        {
            this.endpoint = new IPEndPoint(address, port);
            this.realEndpoint = this.endpointXor(address.ToString(), port);
        }
        /// <summary>
        /// Create XorMappedAddress from IP Address and Port
        /// </summary>
        /// <param name="address">XOR address</param>
        /// <param name="port">XOR address</param>
        public XorMappedAddress(string address, UInt16 port)
        {
            this.endpoint = new IPEndPoint(IPAddress.Parse(address), port);
            this.realEndpoint = this.endpointXor(address, port);
        }
        /// <summary>
        /// Create XorMappedAddress from IP Address and Port
        /// </summary>
        /// <param name="endpoint">XOR IP endpoint</param>
        public XorMappedAddress(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;
            this.realEndpoint = this.endpointXor(endpoint.Address.ToString(), (UInt16)endpoint.Port);
        }
        public XorMappedAddress(byte[] addressByteArray, byte[] portByteArray)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(portByteArray);
            }
            var address = new IPAddress(addressByteArray);
            var port = BitConverter.ToUInt16(portByteArray);
            var endpoint = new IPEndPoint(address, port);
            this.endpoint = endpoint;
            this.realEndpoint = endpointXor(address.ToString(), port);
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
            var addressByteArray = this.endpoint.Address.GetAddressBytes();
            var portByteArray = BitConverter.GetBytes((Int16)this.endpoint.Port);

            byte[] reserved = { 0x00 };
            byte[] addressFamilyByte = { 0x01 };
            var length = reserved.Length + addressFamilyByte.Length + portByteArray.Length + addressByteArray.Length;
            var lengthByteArray = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(portByteArray);
                Array.Reverse(lengthByteArray);
            }
            var res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, reserved, addressFamilyByte, portByteArray, addressByteArray);
            return res;
        }
        public static XorMappedAddress Parse(byte[] data)
        {
            var reservedByteArray = data[0..1];
            var protocolFamilyByteArray = data[1..2];
            var portByteArray = data[2..4];
            var addressByteArray = data[4..data.Length];

            return new XorMappedAddress(addressByteArray, portByteArray);
        }
    }
}