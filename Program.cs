using System;
using System.Net;
using System.Net.Sockets;

namespace doturn
{

    /*
        ref: https://datatracker.ietf.org/doc/html/rfc5389#section-6
         0                   1                   2                   3
         0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |0 0|     STUN Message Type     |         Message Length        |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                         Magic Cookie                          |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                                                               |
        |                     Transaction ID (96 bits)                  |
        |                                                               |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    */
    class Program
    {
        private static void Start()
        {
            var listener = new UdpClient(3478);
            var endpoint = new IPEndPoint(IPAddress.Any, 3478);
            try
            {
                Console.WriteLine("Wait...");
                while (true)
                {
                    var buffer = listener.Receive(ref endpoint);
                    if (buffer.Length < 12)
                    {
                        continue;
                    }
                    var messageType = buffer[0..2];
                    var messageLength = buffer[2..4];
                    var magicCookie = buffer[4..8];
                    var transactionId = buffer[8..12];
                    var messageTypeStr = BitConverter.ToString(messageType).Replace("-", string.Empty);
                    var messageLengthStr = BitConverter.ToString(messageLength).Replace("-", string.Empty);
                    var magicCookieStr = BitConverter.ToString(magicCookie).Replace("-", string.Empty);
                    var transactionIdStr = BitConverter.ToString(transactionId).Replace("-", string.Empty);
                    Console.WriteLine($"{endpoint.Address}:{endpoint.Port}> length: {buffer.Length}, messageType: 0x{messageTypeStr}, messageLength: 0x{messageLengthStr}, magicCookie: 0x{magicCookieStr}, transactionId: 0x{transactionIdStr}");
                    listener.Send(buffer, buffer.Length, endpoint);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                listener.Close();
            }
        }
        static void Main(string[] args)
        {
            Start();
        }
    }
}
