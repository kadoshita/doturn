using Doturn.StunMessage;

using System;

namespace Doturn
{
    public class StunHeader
    {
        public readonly StunMessage.Type type;
        public readonly Int16 messageLength;
        public readonly byte[] magicCookie;
        public readonly byte[] transactionId;
        public StunHeader(StunMessage.Type type, Int16 messageLength, byte[] transactionId)
        {
            this.type = type;
            this.messageLength = messageLength;
            this.transactionId = transactionId;
            var _magicCookie = BitConverter.GetBytes((Int32)0x2112a442);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(_magicCookie);
            }
            this.magicCookie = _magicCookie;
        }
        public StunHeader(byte[] data)
        {
            var messageTypeByteArray = data[0..2];
            var messageLengthByteArray = data[2..4];
            this.magicCookie = data[4..8];
            this.transactionId = data[8..20];
            var _magicCookie = BitConverter.GetBytes((Int32)0x2112a442);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(_magicCookie);
                Array.Reverse(messageTypeByteArray);
                Array.Reverse(messageLengthByteArray);
            }
            this.magicCookie = _magicCookie;
            this.type = (StunMessage.Type)Enum.ToObject(typeof(StunMessage.Type), BitConverter.ToInt16(messageTypeByteArray));
            this.messageLength = BitConverter.ToInt16(messageLengthByteArray);
        }

        public byte[] ToBytes()
        {
            var typeByteArray = this.type.ToBytes();
            var messageLengthByteArray = BitConverter.GetBytes(this.messageLength);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(messageLengthByteArray);
            }
            var res = new byte[20];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, messageLengthByteArray, this.magicCookie, this.transactionId);
            return res;
        }
    }
}