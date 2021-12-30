
using System;
using System.Net;

namespace Doturn.StunAttribute
{
    public class MappedAddress : StunAttributeBase
    {
        public readonly Type type = Type.MAPPED_ADDRESS;
        public readonly IPEndPoint endpoint;
        private readonly byte[] magicCookie;
        public override Type Type => this.type;

        public MappedAddress(IPAddress address, Int32 port, byte[] magicCookie)
        {
            this.endpoint = new IPEndPoint(address, port);
            this.magicCookie = magicCookie;
        }
        public MappedAddress(string address, Int32 port, byte[] magicCookie)
        {
            this.endpoint = new IPEndPoint(IPAddress.Parse(address), port);
            this.magicCookie = magicCookie;
        }
        public MappedAddress(IPEndPoint endpoint, byte[] magicCookie)
        {
            this.endpoint = endpoint;
            this.magicCookie = magicCookie;
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
    }
}