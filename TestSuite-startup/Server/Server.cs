
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
        Console.WriteLine($".::.:::....::  Server is live  ::....:::.::. \non port : {_port}\nIP      : {IPAddress.Loopback}");

        while (true)
        {
            var client = server.AcceptTcpClient();
            Console.WriteLine("Client connected");

            Task.Run(() => HandleClient(client));
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
                    Status = "missing method",
                    Body = "skibbidi"
                };

                var jason = ToJson(response);
                WriteToStream(stream, jason);

            }
            else
            {
                var request = FromJson(msg);

                if(request == null)
                {
                  
                }

                string[] validMehods = ["create", "read", "update", "delete", "echo"];

                if (!validMehods.Contains(request.Method))
                {
                    var response = new Response
                    {
                        Status = "illegal method",
                        Body = "skibidi"
                    };
                    var json = ToJson(response);
                    WriteToStream(stream, json);
                }
                switch (request.Method)
                {
                    case "create":
                        Console.WriteLine("create called");

                        //HandleCreateRequest(request, stream);
                        break;
                    case "read":
                        Console.WriteLine("read called");
                        // HandleReadRequest(request, stream);
                        break;
                    case "update":
                        Console.WriteLine("update called");
                        //HandleUpdateRequest(request, stream);
                        break;
                    case "delete":
                        Console.WriteLine("delete called");
                        //HandleDeleteRequest(request, stream);
                        break;
                    case "echo":
                        Console.WriteLine("echo called");
                        var response = new Response
                        {
                            Status = "1 Ok",
                            Body = request.Body
                        };
                        var jason = ToJson(response);
                        WriteToStream(stream, jason);
                        //HandleEchoRequest(request, stream);
                        break;
                    default:
                        // This should not happen due to earlier validation
                        break;
                }
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
