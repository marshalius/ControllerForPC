﻿using ControllerForPC.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerForPC.Services
{
    public class ConnectionManager() : IConnectionManager
    {
        private readonly TcpService _tcpService = new ();
        //readonly List<(string address, int port)> Servers;
        public async Task<(string address, int port, string name)> ScanServerAsync(int minPort, int maxPort)
        {
#if ANDROID
            string sendMessage = $"controller:{Android.OS.Build.Manufacturer}";
#else
            string sendMessage = $"controller";
#endif
            
            while (true)
            {
                for (int port = minPort; port <= maxPort; port++)
                {
                    UdpService _udpService = new(port);
                    try
                    {
                        Console.WriteLine($"{port} deneniyor...");
                        while (true)
                        {
                            await _udpService.BroadcastAsync(sendMessage);
                            CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(100));
                            var received = await _udpService.ReceiveAsync(cts.Token);
                            Console.WriteLine($"{received.message}");
                            if (received.message.StartsWith("controller"))
                            {
                                continue;
                            }
                            else
                            {
                                return (received.address, received.port, received.message);
                            }

                        }

                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Zaman aşımı oluştu, port taraması devam ediyor.");
                        continue;
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"An error occured while scanning the server: {e}");
                    }
                    finally
                    {
                        _udpService.Dispose();
                    }


                }
            }
            

        }
        public async Task ConnectAsync(string host, int port)
        {
            await _tcpService.ConnectAsync(host, port);
            
        }
        public async Task<string> ReceiveAsync()
        {
            string message = await _tcpService.ReceiveAsync();
            return message;
        }
        public async Task SendAsync(string message)
        {
            await _tcpService.SendAsync(message + "\n");
        }
        public void Disconnect()
        {
            _tcpService.Disconnect();
        }
        public bool IsConnected()
        {
            return _tcpService.IsConnected();
        }

        
    }
}
