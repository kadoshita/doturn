using System;
using Doturn.StunMessage;

namespace Doturn
{
    public class StunHeader
    {
        public readonly StunMessage.Type type;
        public readonly short messageLength;
        public readonly byte[] magicCookie;
        public readonly byte[] transactionId;
        public StunHeader(StunMessage.Type type, short messageLength, byte[] transactionId)
        {
            this.type = type;
            this.messageLength = messageLength;
            this.transactionId = transactionId;
            byte[] _magicCookie = BitConverter.GetBytes((int)0x2112a442);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(_magicCookie);
            }
            magicCookie = _magicCookie;
        }
        public StunHeader(StunMessage.Type type, short messageLength, byte[] magicCookie, byte[] transactionId)
        {
            this.type = type;
            this.messageLength = messageLength;
            this.transactionId = transactionId;
            this.magicCookie = magicCookie;
        }
        public StunHeader(byte[] data)
        {
            byte[] messageTypeByteArray = data[0..2];
            byte[] messageLengthByteArray = data[2..4];
            magicCookie = data[4..8];
            transactionId = data[8..20];
            byte[] _magicCookie = BitConverter.GetBytes((int)0x2112a442);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(_magicCookie);
                Array.Reverse(messageTypeByteArray);
                Array.Reverse(messageLengthByteArray);
            }
            magicCookie = _magicCookie;
            type = (StunMessage.Type)Enum.ToObject(typeof(StunMessage.Type), BitConverter.ToInt16(messageTypeByteArray));
            messageLength = BitConverter.ToInt16(messageLengthByteArray);
        }

        public byte[] ToBytes()
        {
            byte[] typeByteArray = type.ToBytes();
            byte[] messageLengthByteArray = BitConverter.GetBytes(messageLength);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(messageLengthByteArray);
            }
            byte[] res = new byte[20];
            ByteArrayUtils.MergeByteArray(ref res, typeByteArray, messageLengthByteArray, magicCookie, transactionId);
            return res;
        }
    }
}