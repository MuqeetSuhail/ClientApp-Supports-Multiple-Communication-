using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientApp
{
    static class ClientClass
    {
        const int bufferSize = 1024 * 1024; // Around 2 Megabytes.

        public static async Task SendMessagetoServerAsync(string message, TcpClient tempclient)
        {
            await Task.Run(() =>
            {
                string messagetoSend = message;
                byte[] messageinBytes = Encoding.Unicode.GetBytes(messagetoSend);
                int lengthofMessageinbytes = messageinBytes.Length;
                NetworkStream Stream = tempclient.GetStream();
                Stream.Write(messageinBytes, 0, lengthofMessageinbytes); // 0 Represents the location of buffer we want to start writing data from.
            });
        }

        public static async Task<string> ReceiveMessagefromServerAsync(byte[] buffer, TcpClient tempclient)
        {
            return await Task.Run(() =>
            {
                NetworkStream Stream = tempclient.GetStream();
                int noofbytesread = Stream.Read(buffer, 0, bufferSize); // 0 Represents the location of buffer we want start storing read data from
                string messageReceived = Encoding.Unicode.GetString(buffer, 0, noofbytesread);
                return messageReceived;
            });
        }

        public static async Task SendMessagetoSpecificClientAsync(string numbertosendto, string message, TcpClient tempSenderclient)
        {
            await SendMessagetoServerAsync(numbertosendto, tempSenderclient); // Firstly We Send Number
            await Task.Delay(1000);
            await SendMessagetoServerAsync(message, tempSenderclient); // Then Message.
        }
    }
}
