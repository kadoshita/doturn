using System;
using System.Net;

namespace Doturn.StunAttribute
{
    public class XorPeerAddress : StunAttributeBase
    {
        public readonly Type type = Type.XOR_PEER_ADDRESS;
        public readonly IPEndPoint endpoint;
        public readonly IPEndPoint realEndpoint;
        public override Type Type => type;
        /// <summary>
        /// Create XorPeerAddress from IP Address and Port
        /// </summary>
        /// <param name="address">Real address</param>
        /// <param name="port">Real port</param>
        public XorPeerAddress(IPAddress address, ushort port)
        {
            realEndpoint = new IPEndPoint(address, port);
            endpoint = endpointXor(address.ToString(), port);
        }
        /// <summary>
        /// Create XorPeerAddress from IP Address and Port
        /// </summary>
        /// <param name="address">Real address</param>
        /// <param name="port">Real port</param>
        public XorPeerAddress(string address, ushort port)
        {
            realEndpoint = new IPEndPoint(IPAddress.Parse(address), port);
            endpoint = endpointXor(address, port);
        }
        /// <summary>
        /// Create XorPeerAddress from IP Address and Port
        /// </summary>
        /// <param name="endpoint">Real IP endpoint</param>
        public XorPeerAddress(IPEndPoint endpoint)
        {
            realEndpoint = endpoint;
            this.endpoint = endpointXor(endpoint.Address.ToString(), (ushort)endpoint.Port);
        }
        /// <summary>
        /// Create XorPeerAddress from IP Address ByteArray and Port ByteArray
        /// </summary>
        /// <param name="addressByteArray">XOR address byte array</param>
        /// <param name="portByteArray">XOR address byte array</param>
        public XorPeerAddress(byte[] addressByteArray, byte[] portByteArray)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(portByteArray);
            }
            var address = new IPAddress(addressByteArray);
            ushort port = BitConverter.ToUInt16(portByteArray);
            var endpoint = new IPEndPoint(address, port);
            this.endpoint = endpoint;
            realEndpoint = endpointXor(address.ToString(), port);
        }

        private static IPEndPoint endpointXor(string address, ushort port)
        {
            byte[] addressByteArray = IPAddress.Parse(address).GetAddressBytes();
            byte[] portByteArray = BitConverter.GetBytes((ushort)port);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(portByteArray);
            }
            byte[] xorAddressByteArray = ByteArrayUtils.XorAddress(addressByteArray);
            byte[] xorPortByteArray = ByteArrayUtils.XorPort(portByteArray);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(xorPortByteArray);
            }
            return new IPEndPoint(new IPAddress(xorAddressByteArray), BitConverter.ToUInt16(xorPortByteArray));
        }

        public override byte[] ToBytes()
        {
            byte[] typeByteArray = type.ToBytes();
            byte[] addressByteArray = endpoint.Address.GetAddressBytes();
            byte[] portByteArray = BitConverter.GetBytes((short)endpoint.Port);

            byte[] reserved = { 0x00 };
            byte[] addressFamilyByte = { 0x01 };
            int length = reserved.Length + addressFamilyByte.Length + portByteArray.Length + addressByteArray.Length;
            byte[] lengthByteArray = BitConverter.GetBytes((short)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(portByteArray);
                Array.Reverse(lengthByteArray);
            }
            byte[] res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, reserved, addressFamilyByte, portByteArray, addressByteArray);
            return res;
        }

        public static XorPeerAddress Parse(byte[] data)
        {
            _ = data[0..1];
            _ = data[1..2];
            byte[] portByteArray = data[2..4];
            byte[] addressByteArray = data[4..data.Length];

            return new XorPeerAddress(addressByteArray, portByteArray);
        }
    }
}