using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

class Client
{
    static void Main(string[] args)
    {
        string filePath = "points.txt"; // Укажите путь к вашему файлу
        string serverIP = "127.0.0.1"; // IP сервера
        int serverPort = 11000; // Порт сервера

        UdpClient client = new UdpClient();
        try
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            client.Send(fileData, fileData.Length, serverIP, serverPort);

            IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] response = client.Receive(ref serverEndpoint);
            string result = Encoding.UTF8.GetString(response);

            Console.WriteLine("Ответ от сервера:");
            Console.WriteLine(result);
        }
        finally
        {
            client.Close();
        }
    }
}
