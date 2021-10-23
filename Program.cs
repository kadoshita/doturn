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

        ref: https://datatracker.ietf.org/doc/html/rfc5389#section-15
         0                   1                   2                   3
         0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |         Type                  |            Length             |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                         Value (variable)                ....
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

        MAPPED-ADDRESS
         0                   1                   2                   3
         0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |0 0 0 0 0 0 0 0|    Family     |           Port                |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                                                               |
        |                 Address (32 bits or 128 bits)                 |
        |                                                               |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

        XOR-MAPPED-ADDRESS
         0                   1                   2                   3
         0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |x x x x x x x x|    Family     |         X-Port                |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                X-Address (Variable)
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
                    var transactionId = buffer[8..20];
                    var messageTypeStr = BitConverter.ToString(messageType).Replace("-", string.Empty);
                    var messageLengthStr = BitConverter.ToString(messageLength).Replace("-", string.Empty);
                    var magicCookieStr = BitConverter.ToString(magicCookie).Replace("-", string.Empty);
                    var transactionIdStr = BitConverter.ToString(transactionId).Replace("-", string.Empty);
                    Console.WriteLine($"[{DateTime.Now}] {endpoint.Address}:{endpoint.Port}> length: {buffer.Length}, messageType: 0x{messageTypeStr}, messageLength: 0x{messageLengthStr}, magicCookie: 0x{magicCookieStr}, transactionId: 0x{transactionIdStr}");
                    var portBytes = BitConverter.GetBytes(endpoint.Port);
                    Array.Reverse(portBytes);
                    var portBytesXor = new byte[2];
                    portBytesXor[0] = (byte)(portBytes[2] ^ magicCookie[0]);
                    portBytesXor[1] = (byte)(portBytes[3] ^ magicCookie[1]);
                    var addressBytes = endpoint.Address.GetAddressBytes();
                    var addressBytesXor = new byte[addressBytes.Length];
                    for (int i = 0; i < addressBytes.Length; i++)
                    {
                        addressBytesXor[i] = (byte)(addressBytes[i] ^ magicCookie[i]);
                    }
                    var resMessageType = stringToByteArray("0101");
                    var resLength = stringToByteArray("000c");
                    var resType = BitConverter.ToInt32(magicCookie) == 0 ? stringToByteArray("0010") : stringToByteArray("0020");
                    var resDataLength = stringToByteArray("0008");
                    var resDataAddressType = stringToByteArray("0001");
                    var res = new byte[32];
                    int endPos = 0;
                    Array.Copy(resMessageType, 0, res, endPos, resMessageType.Length);
                    endPos += resMessageType.Length;
                    Array.Copy(resLength, 0, res, endPos, resLength.Length);
                    endPos += resLength.Length;
                    Array.Copy(magicCookie, 0, res, endPos, magicCookie.Length);
                    endPos += magicCookie.Length;
                    Array.Copy(transactionId, 0, res, endPos, transactionId.Length);
                    endPos += transactionId.Length;
                    Array.Copy(resType, 0, res, endPos, resType.Length);
                    endPos += resType.Length;
                    Array.Copy(resDataLength, 0, res, endPos, resDataLength.Length);
                    endPos += resDataLength.Length;
                    Array.Copy(resDataAddressType, 0, res, endPos, resDataAddressType.Length);
                    endPos += resDataAddressType.Length;
                    Array.Copy(portBytesXor, 0, res, endPos, portBytesXor.Length);
                    endPos += portBytesXor.Length;
                    Array.Copy(addressBytesXor, 0, res, endPos, addressBytesXor.Length);
                    endPos += addressBytesXor.Length;
                    listener.Send(res, endPos, endpoint);
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
        private static byte[] stringToByteArray(string input)
        {
            var res = new byte[input.Length / 2];
            int j = 0;
            for (int i = 0; i < input.Length; i += 2)
            {
                string w = input.Substring(i, 2);
                res[j] = Convert.ToByte(w, 16);
                j++;
            }
            return res;
        }
        static void Main(string[] args)
        {
            Start();
        }
    }
}
