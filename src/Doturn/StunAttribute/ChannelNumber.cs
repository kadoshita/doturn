using System;

namespace Doturn.StunAttribute
{
    public class ChannelNumber : StunAttributeBase
    {
        public readonly Type type = Type.CHANNEL_NUMBER;
        public readonly byte[] channelNumber;
        private readonly byte[] _reserved;
        public override Type Type => type;

        public ChannelNumber()
        {

            _reserved = new byte[] { 0x00, 0x00 };
        }
        public ChannelNumber(byte[] channelNumber, byte[] reserved)
        {
            this.channelNumber = channelNumber;
            _reserved = reserved;
        }
        public override byte[] ToBytes()
        {
            byte[] typeByteArray = type.ToBytes();
            int length = channelNumber.Length + _reserved.Length;
            byte[] lengthByteArray = BitConverter.GetBytes((short)length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthByteArray);
            }
            byte[] res = new byte[2 + 2 + length];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, lengthByteArray, channelNumber, _reserved);
            return res;
        }
        public static ChannelNumber Parse(byte[] data)
        {
            byte[] channelNumber = data[0..2];
            byte[] reserved = data[2..data.Length];
            return new ChannelNumber(channelNumber, reserved);
        }
    }
}