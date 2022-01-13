using System;

namespace Doturn.StunAttribute
{
    public class RequestedTransport : StunAttributeBase
    {
        public readonly Type type = Type.REQUESTED_TRANSPORT;
        public readonly Transport transport;
        private readonly byte[] _reserved;
        public override Type Type => type;

        public RequestedTransport()
        {
            transport = Transport.UDP;
            _reserved = new byte[] { 0x00, 0x00, 0x00 };
        }
        public RequestedTransport(Transport transport, byte[] reserved)
        {
            this.transport = transport;
            _reserved = reserved;
        }
        public override byte[] ToBytes()
        {
            byte[] typeByteArray = type.ToBytes();
            byte[] transportByteArray = transport.ToBytes();
            int length = transportByteArray.Length + _reserved.Length;
            byte[] lengthByteArray = BitConverter.GetBytes((short)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }
            byte[] res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, transportByteArray, _reserved);
            return res;
        }
        public static RequestedTransport Parse(byte[] data)
        {
            byte[] reserved = data[1..data.Length];
            var transport = (Transport)Enum.ToObject(typeof(Transport), (byte)data[0]);
            return new RequestedTransport(transport, reserved);
        }
    }
}