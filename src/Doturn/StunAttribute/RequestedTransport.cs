using System;

namespace Doturn.StunAttribute
{
    public class RequestedTransport : StunAttributeBase
    {
        public readonly Type type = Type.REQUESTED_TRANSPORT;
        public readonly Transport transport;
        private readonly byte[] reserved;
        public override Type Type => this.type;

        public RequestedTransport()
        {
            this.transport = Transport.UDP;
            this.reserved = new byte[] { 0x00, 0x00, 0x00 };
        }
        public RequestedTransport(Transport transport, byte[] reserved)
        {
            this.transport = transport;
            this.reserved = reserved;
        }
        public override byte[] ToByte()
        {
            var typeByteArray = this.type.ToByte();
            var transportByteArray = this.transport.ToByte();
            var length = transportByteArray.Length + this.reserved.Length;
            var lengthByteArray = BitConverter.GetBytes((Int16)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }
            var res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, transportByteArray, this.reserved);
            return res;
        }
        public static RequestedTransport Parse(byte[] data)
        {
            var reserved = data[1..data.Length];
            var transport = (Transport)Enum.ToObject(typeof(Transport), (byte)data[0]);
            return new RequestedTransport(transport, reserved);
        }
    }
}