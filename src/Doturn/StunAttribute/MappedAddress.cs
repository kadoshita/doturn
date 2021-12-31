using System;
using System.Net;

namespace Doturn.StunAttribute
{
    public class MappedAddress : StunAttributeBase
    {
        public readonly Type type = Type.MAPPED_ADDRESS;
        public readonly IPEndPoint endpoint;
        public override Type Type => this.type;

        public MappedAddress(IPAddress address, Int32 port)
        {
            this.endpoint = new IPEndPoint(address, port);
        }
        public MappedAddress(string address, Int32 port)
        {
            this.endpoint = new IPEndPoint(IPAddress.Parse(address), port);
        }
        public MappedAddress(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;
        }

        public override byte[] ToByte()
        {
            var typeByteArray = this.type.ToByte();
            byte[] addressByteArray = this.endpoint.Address.GetAddressBytes();
            byte[] portByteArray = BitConverter.GetBytes((Int16)this.endpoint.Port);
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

        public static IStunAttribute Parse(byte[] data)
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
            return new MappedAddress(address, port);
        }
    }
}