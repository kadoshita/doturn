﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace doturn
{
    class Program
    {
        private static void Start(IConfigurationRoot configuration)
        {
            var username = configuration["Username"];
            var password = configuration["Password"];
            var realm = configuration["Realm"];
            var externalIPAddress = configuration["ExternalIPAddress"];

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
                        var portByte = BitConverter.GetBytes(endpoint.Port);
                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(portByte);
                        }
                        var addressByte = endpoint.Address.GetAddressBytes();
                        byte[] res;
                        if (BitConverter.ToInt32(stunHeader.magicCookie) == 0)
                        {
                            var mappedAddress = new MappedAddress(addressByte, portByte, stunHeader);
                            res = mappedAddress.ToByte();
                        }
                        else
                        {
                            var xorMappedAddress = new XorMappedAddress(addressByte, portByte, stunHeader);
                            res = xorMappedAddress.ToByte();
                        }

                        listener.Send(res, res.Length, endpoint);
                    }
                    else if (stunHeader.messageType == StunMessage.ALLOCATE)
                    {
                        var r = new Random();
                        var rn = r.Next(10);
                        Int16 relayPort = Convert.ToInt16(20000 + rn);
                        var allocateRequest = new AllocateRequest(stunHeader, buffer[20..buffer.Length], username, password, realm);
                        Task t = Task.Run(() =>
                        {
                            Console.WriteLine($"relay {relayPort}");
                            var l = new UdpClient(relayPort);
                            var e = new IPEndPoint(IPAddress.Any, relayPort);
                            while (true)
                            {
                                Console.WriteLine("relay wait");
                                var b = l.Receive(ref e);
                                if (b.Length < 12)
                                {
                                    continue;
                                }
                                var stunHeader = StunHeader.FromRawHeader(b);
                                if (BitConverter.ToInt32(stunHeader.transactionId) == 0)
                                {
                                    continue;
                                }
                                Console.WriteLine(stunHeader.messageType);
                                if (stunHeader.messageType == StunMessage.BINDING)
                                {
                                    var portByte = BitConverter.GetBytes(e.Port);
                                    if (BitConverter.IsLittleEndian)
                                    {
                                        Array.Reverse(portByte);
                                    }
                                    var addressByte = e.Address.GetAddressBytes();
                                    byte[] res;
                                    if (BitConverter.ToInt32(stunHeader.magicCookie) == 0)
                                    {
                                        var mappedAddress = new MappedAddress(addressByte, portByte, stunHeader);
                                        res = mappedAddress.ToByte();
                                    }
                                    else
                                    {
                                        var xorMappedAddress = new XorMappedAddress(addressByte, portByte, stunHeader);
                                        res = xorMappedAddress.ToByte();
                                    }

                                    listener.Send(res, res.Length, e);
                                }
                            }
                        });
                        if (allocateRequest.isValid())
                        {
                            var portByte = BitConverter.GetBytes(endpoint.Port);
                            if (BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(portByte);
                            }
                            var addressByte = endpoint.Address.GetAddressBytes();
                            var allocateSuccessResponse = new AllocateSuccessResponse(stunHeader, portByte, addressByte, externalIPAddress, relayPort, username, password, realm);
                            var res = allocateSuccessResponse.ToByte();
                            listener.Send(res, res.Length, endpoint);
                        }
                        else
                        {
                            var allocateErrorResponse = new AllocateErrorResponse(stunHeader);
                            var res = allocateErrorResponse.ToByte();
                            listener.Send(res, res.Length, endpoint);
                        }
                    }
                    else if (stunHeader.messageType == StunMessage.REFRESH)
                    {

                        var refreshRequest = new RefreshRequest(stunHeader, buffer[20..buffer.Length], username, password, realm);
                        if (refreshRequest.isValid())
                        {
                            var portByte = BitConverter.GetBytes(endpoint.Port);
                            if (BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(portByte);
                            }
                            var addressByte = endpoint.Address.GetAddressBytes();
                            var refreshSuccessResponse = new RefreshSuccessResponse(stunHeader, username, password, realm);
                            var res = refreshSuccessResponse.ToByte();
                            listener.Send(res, res.Length, endpoint);
                        }
                        else
                        {
                            var allocateErrorResponse = new AllocateErrorResponse(stunHeader);
                            var res = allocateErrorResponse.ToByte();
                            listener.Send(res, res.Length, endpoint);
                        }
                    }
                    else if (stunHeader.messageType == StunMessage.CREATE_PERMISSION)
                    {
                        var createPermissionRequest = new CreatePermissionRequest(stunHeader, buffer[20..buffer.Length]);
                        var createPermissionSuccessResponse = new CreatePermissionSuccessResponse(stunHeader, username, password, realm);
                        var res = createPermissionSuccessResponse.ToByte();
                        listener.Send(res, res.Length, endpoint);
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
            var configureation = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(path: "application.json").Build();
            Start(configureation);
        }
    }
}
