using System;
using System.Buffers.Binary;

namespace Doturn.StunMessage;

public class StunMessageParseException : Exception
{
    public StunMessageParseException() : base() { }
}
public static class StunMessageParser
{
    public static IStunMessage Parse(byte[] data, IAppSettings appSettings) => Parse(data.AsSpan(), appSettings);

    public static IStunMessage Parse(ReadOnlySpan<byte> data, IAppSettings appSettings)
    {
        if (data.Length < 20)
        {
            throw new StunMessageParseException();
        }
        var messageType = (Type)BinaryPrimitives.ReadUInt16BigEndian(data[0..2]);
        byte[] magicCookieBytes = data.Slice(4, 4).ToArray();
        byte[] transactionIdBytes = data.Slice(8, 12).ToArray();
        byte[] messageBytes = data[20..].ToArray();

        return messageType switch
        {
            Type.Binding => new Binding(magicCookieBytes, transactionIdBytes, appSettings),
            Type.Allocate => new Allocate(magicCookieBytes, transactionIdBytes, messageBytes, appSettings),
            Type.CreatePermission => new CreatePermission(magicCookieBytes, transactionIdBytes, messageBytes, appSettings),
            Type.Refresh => new Refresh(magicCookieBytes, transactionIdBytes, messageBytes, appSettings),
            Type.SendIndication => new Send(messageBytes),
            Type.ChannelBind => new ChannelBind(magicCookieBytes, transactionIdBytes, messageBytes, appSettings),
            _ => throw new StunMessageParseException(),
        };
    }
}
