using System;
using System.Net;

namespace Doturn.StunAttribute
{
    public class MappedAddress : StunAttributeBase
    {
        public readonly Type type = Type.MAPPED_ADDRESS;
        public readonly IPEndPoint endpoint;
        public override Type Type => type;

        public MappedAddress(IPAddress address, int port)
        {
            endpoint = new IPEndPoint(address, port);
        }
        public MappedAddress(string address, int port)
        {
            endpoint = new IPEndPoint(IPAddress.Parse(address), port);
        }
        public MappedAddress(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;
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

        public static IStunAttribute Parse(byte[] data)
        {
            _ = data[0..1];
            _ = data[1..2];
            byte[] portByteArray = data[2..4];
            byte[] addressByteArray = data[4..data.Length];
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(portByteArray);
            }
            int port = (int)BitConverter.ToInt16(portByteArray);
            var address = new IPAddress(addressByteArray);
            return new MappedAddress(address, port);
        }
    }
}