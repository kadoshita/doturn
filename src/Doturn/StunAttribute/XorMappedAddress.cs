using System;
using System.Net;

namespace Doturn.StunAttribute
{
    public class XorMappedAddress : StunAttributeBase
    {
        public readonly Type type = Type.XOR_MAPPED_ADDRESS;
        public readonly IPEndPoint endpoint;
        private byte[] magicCookie = BitConverter.GetBytes((Int32)0x2112a442);
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

        public override byte[] ToByte()
        {
            var typeByteArray = this.type.ToByte();
            var addressByteArray = this.endpoint.Address.GetAddressBytes();
            var portByteArray = BitConverter.GetBytes((Int16)this.endpoint.Port);
            var xorAddressByteArray = new byte[addressByteArray.Length];
            var xorPortByteArray = new byte[portByteArray.Length];
            for (var i = 0; i < addressByteArray.Length; i++)
            {
                xorAddressByteArray[i] = (byte)(addressByteArray[i] ^ this.magicCookie[i]);
            }
            for (var i = 0; i < portByteArray.Length; i++)
            {
                xorPortByteArray[i] = (byte)(portByteArray[i] ^ this.magicCookie[i]);
            }

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