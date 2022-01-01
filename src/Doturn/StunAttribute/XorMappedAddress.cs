using System;
using System.Net;

namespace Doturn.StunAttribute
{
    public class XorMappedAddress : StunAttributeBase
    {
        public readonly Type type = Type.XOR_MAPPED_ADDRESS;
        public readonly IPEndPoint endpoint;
        public override Type Type => this.type;
        public XorMappedAddress(IPAddress address, Int32 port)
        {
            this.endpoint = new IPEndPoint(address, port);
        }
        public XorMappedAddress(string address, Int32 port)
        {
            this.endpoint = new IPEndPoint(IPAddress.Parse(address), port);
        }
        public XorMappedAddress(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;
        }

        public override byte[] ToBytes()
        {
            var typeByteArray = this.type.ToBytes();
            var addressByteArray = this.endpoint.Address.GetAddressBytes();
            var portByteArray = BitConverter.GetBytes((Int16)this.endpoint.Port);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(portByteArray);
            }
            var xorAddressByteArray = ByteArrayUtils.XorAddress(addressByteArray);
            var xorPortByteArray = ByteArrayUtils.XorPort(portByteArray);

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
        public static XorMappedAddress Parse(byte[] data)
        {
            var reservedByteArray = data[0..1];
            var protocolFamilyByteArray = data[1..2];
            var portByteArray = data[2..4];
            var addressByteArray = data[4..data.Length];
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(portByteArray);
            }
            var port = (Int32)BitConverter.ToInt16(portByteArray);
            var address = new IPAddress(addressByteArray);
            return new XorMappedAddress(address, port);
        }
    }
}