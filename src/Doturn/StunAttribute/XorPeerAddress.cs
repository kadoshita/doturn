using System;
using System.Net;

namespace Doturn.StunAttribute
{
    public class XorPeerAddress : StunAttributeBase
    {
        public readonly Type type = Type.XOR_PEER_ADDRESS;
        public readonly IPEndPoint endpoint;
        private readonly byte[] magicCookie;
        public override Type Type => this.type;
        public XorPeerAddress(IPAddress address, Int32 port, byte[] magicCookie)
        {
            this.endpoint = new IPEndPoint(address, port);
            this.magicCookie = magicCookie;
        }
        public XorPeerAddress(string address, Int32 port, byte[] magicCookie)
        {
            this.endpoint = new IPEndPoint(IPAddress.Parse(address), port);
            this.magicCookie = magicCookie;
        }
        public XorPeerAddress(IPEndPoint endpoint, byte[] magicCookie)
        {
            this.endpoint = endpoint;
            this.magicCookie = magicCookie;
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
    }
}