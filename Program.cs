using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;

namespace doturn
{
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
                    var stunHeader = StunHeader.FromRawHeader(buffer);
                    if (BitConverter.ToInt32(stunHeader.transactionId) == 0)
                    {
                        continue;
                    }
                    if (stunHeader.messageType == StunMessage.BINDING)// Binding request
                    {
                        var portBytes = BitConverter.GetBytes(endpoint.Port);
                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(portBytes);
                        }
                        var addressBytes = endpoint.Address.GetAddressBytes();
                        byte[] res;
                        if (BitConverter.ToInt32(stunHeader.magicCookie) == 0)
                        {
                            var mappedAddress = new MappedAddress(addressBytes, portBytes, stunHeader);
                            res = mappedAddress.ToByte();
                        }
                        else
                        {
                            var xorMappedAddress = new XorMappedAddress(addressBytes, portBytes, stunHeader);
                            res = xorMappedAddress.ToByte();
                        }

                        listener.Send(res, res.Length, endpoint);
                    }
                    else if (stunHeader.messageType == StunMessage.ALLOCATE)
                    {
                        var allocateRequest = new AllocateRequest(stunHeader, buffer[20..buffer.Length]);
                        if (allocateRequest.isValid())
                        {
                        }
                        else
                        {
                            var allocateResponse = new AllocateResponse(stunHeader, false);
                            var res = allocateResponse.ToByte();
                            listener.Send(res, res.Length, endpoint);
                        }
                    }
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
