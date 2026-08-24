using System;

namespace Doturn.StunMessage;

public class StunMessageParseException : Exception
{
    public StunMessageParseException() : base() { }
}
public static class StunMessageParser
{
    public static IStunMessage Parse(byte[] data, IAppSettings appSettings)
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

        if (messageType == Type.Binding)
        {
            return new Binding(header.MagicCookie, header.TransactionId, appSettings);
        }
        else if (messageType == Type.Allocate)
        {
            return new Allocate(header.MagicCookie, header.TransactionId, messageBytes, appSettings);
        }
        else if (messageType == Type.CreatePermission)
        {
            return new CreatePermission(header.MagicCookie, header.TransactionId, messageBytes, appSettings);
        }
        else if (messageType == Type.Refresh)
        {
            return new Refresh(header.MagicCookie, header.TransactionId, messageBytes, appSettings);
        }
        else if (messageType == Type.SendIndication)
        {
            return new Send(messageBytes);
        }
        else if (messageType == Type.ChannelBind)
        {
            return new ChannelBind(header.MagicCookie, header.TransactionId, messageBytes, appSettings);
        }
        else
        {
            throw new StunMessageParseException();
        }
    }
}
