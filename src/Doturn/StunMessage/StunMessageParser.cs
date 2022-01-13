using System;

namespace Doturn.StunMessage
{
    public class StunMessageParseException : Exception
    {
        public StunMessageParseException() : base() { }
    }
    public static class StunMessageParser
    {
        public static IStunMessage Parse(byte[] data, AppSettings appSettings)
        {
            byte[] messageTypeBytes = data[0..2]; // 16bit
            byte[] messageLengthBytes = data[2..4]; // 16bit
            byte[] magicCookieBytes = data[4..8]; // 32bit
            byte[] transactionIdBytes = data[8..20]; // 96bit
            byte[] messageBytes = data[20..data.Length];
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(messageTypeBytes);
                Array.Reverse(messageLengthBytes);
            }
            var messageType = (Type)Enum.ToObject(typeof(Type), BitConverter.ToInt16(messageTypeBytes));
            short messageLength = BitConverter.ToInt16(messageLengthBytes);
            var header = new StunHeader(messageType, messageLength, magicCookieBytes, transactionIdBytes);

            if (messageType == Type.BINDING)
            {
                return new Binding(header.magicCookie, header.transactionId, appSettings);
            }
            else if (messageType == Type.ALLOCATE)
            {
                return new Allocate(header.magicCookie, header.transactionId, messageBytes, appSettings);
            }
            else if (messageType == Type.CREATE_PERMISSION)
            {
                return new CreatePermission(header.magicCookie, header.transactionId, messageBytes, appSettings);
            }
            else if (messageType == Type.REFRESH)
            {
                return new Refresh(header.magicCookie, header.transactionId, messageBytes, appSettings);
            }
            else
            {
                throw new StunMessageParseException();
            }
        }
    }
}