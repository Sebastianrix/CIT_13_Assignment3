
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;
using System;
using System.Text.Json;


public class Server
{
    private readonly int _port;

    public Server(int port)
    {
        _port = port;
    }
    public void Run()
    {


        var server = new TcpListener(IPAddress.Loopback, _port);
        server.Start();

        Console.WriteLine($"Server is live on port : {_port}");

        while (true)
        {
            var client = server.AcceptTcpClient();
            Console.WriteLine("Client connected");
            HandleClient(client);
        }
    }
    private void HandleClient(TcpClient client)
    {
        try
        {

            var stream = client.GetStream(); //picks up stream from client
            string msg = ReadFromStream(stream); //calls read from stream method
                                             

            Console.WriteLine($"Message from client decoded is : {msg}");

            if (msg == "{}") 
            {
                var response = new Response
                {
                    Status = "missing method"
                };

                var jason = ToJson(response);
                WriteToStream(stream,jason);
           
            }


        }
        catch { }
    }

    private string ReadFromStream(NetworkStream stream) 
    {
        var buffer = new byte[1024];
       var readCount = stream.Read(buffer); // read the encoded data from client
        return Encoding.UTF8.GetString(buffer, 0, readCount);  //Decode
    }

    private void WriteToStream(NetworkStream stream, string msg)
    {
       var buffer = Encoding.UTF8.GetBytes(msg); //Encode again
        stream.Write(buffer);
    }

    public static string ToJson(Response response)
    {
        return JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public static Request? FromJson(string element)
    {
        return JsonSerializer.Deserialize<Request>(element, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}
