using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

class Server
{
    static void Main(string[] args)
    {
        int port = 11000;
        UdpClient server = new UdpClient(port);

        Console.WriteLine($"Сервер запущен на порту {port}.");
        IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);

        try
        {
            byte[] receivedData = server.Receive(ref clientEndpoint);
            string tempFilePath = "temp_points.txt";
            File.WriteAllBytes(tempFilePath, receivedData);

            Console.WriteLine("Файл получен, начинается обработка...");

            // Запуск MPI
            Process mpiProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "mpiexec",
                    Arguments = $"-n 4 MPIProcessor.exe {tempFilePath}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            mpiProcess.Start();
            string mpiOutput = mpiProcess.StandardOutput.ReadToEnd();
            mpiProcess.WaitForExit();

            byte[] response = Encoding.UTF8.GetBytes(mpiOutput);
            server.Send(response, response.Length, clientEndpoint);

            Console.WriteLine("Результаты отправлены клиенту.");
        }
        finally
        {
            server.Close();
        }
    }
}
