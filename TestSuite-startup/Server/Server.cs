
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;
using System;
using System.Text.Json;


public class Server
{
    private readonly int _port;
    private static List<Category> categories = new List<Category>();
    private static int nextCategoryId = 1;

    public Server(int port)
    {
        _port = port;

        // Initialize the categories list with sample data
        categories.Add(new Category { Id = nextCategoryId++, Name = "Beverages" });
        categories.Add(new Category { Id = nextCategoryId++, Name = "Condiments" });
        categories.Add(new Category { Id = nextCategoryId++, Name = "Confections" });
        categories.Add(new Category { Id = nextCategoryId++, Name = "Condiments" });
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
                    Status = "missing method, missing date",
                    Body = "skibbidi"
                };

                var jason = ToJson(response);
                WriteToStream(stream, jason);
                Console.WriteLine(response.Status);
            }
            else
            {
                var request = FromJson(msg);

                    string[] validMehods = ["create", "read", "update", "delete", "echo"];


                if (!validMehods.Contains(request.Method))
                {
                    var response = new Response
                    {
                        Status = "4 Illegal method"
                        //Body = "skibidi"
                    };
                    Console.WriteLine(response.Status);
                    var json = ToJson(response);
                    WriteToStream(stream, json);
                }
                else
                if (string.IsNullOrEmpty(request.Date))
                {
                    var response = new Response
                    {
                        Status = "4 missing date"
                        //Body = "skibidi"
                    };
                    Console.WriteLine(response.Status);
                    var json = ToJson(response);
                    WriteToStream(stream, json);
                }
                else
                if (request.Method != "echo" && String.IsNullOrEmpty(request.Path)) 
                {
                    var response = new Response
                    {
                        Status = "4 missing path"
                        //Body = "skibidi"
                    };
                    Console.WriteLine(response.Status);
                    var json = ToJson(response);
                    WriteToStream(stream, json);
                }

     
                switch (request.Method)
                {
                    case "create":
                        Console.WriteLine("create called");
                        CreateRequestHandle(request, stream);
                        break;
                    case "read":
                        Console.WriteLine("read called");
                        ReadRequestHandle(request, stream);
                        break;
                    case "update":
                        Console.WriteLine("update called");
                        UpdateRequestHandle(request, stream);
                        break;
                    case "delete":
                        Console.WriteLine("delete called");
                        DeleteRequestHandle(request, stream);
                        break;
                    case "echo":
                        Console.WriteLine("echo called");                 
                        EchoRequestHandle(request, stream);
                        break;
                    default:
                        // This should not happen due to earlier validation
                        break;
                }
            }


        }
        catch { }
    }

    // Request handlers
    private void EchoRequestHandle(Request request, NetworkStream stream){
        var response = new Response
        {
            Status = "1 Ok",
            Body = request.Body
        };
        var jason = ToJson(response);
        WriteToStream(stream, jason);
    }
    private void CreateRequestHandle(Request request, NetworkStream stream)
    {
        var response = new Response
        {
            Status = "2 Created",
            Body = request.Body
        };
        var jason = ToJson(response);
        WriteToStream(stream, jason);
    }
    private void ReadRequestHandle(Request request, NetworkStream stream)
    {
        var response = new Response
        {
            Status = "1 Ok",
            Body = request.Body
        };
        var jason = ToJson(response);
        WriteToStream(stream, jason);
    }
    private void DeleteRequestHandle(Request request, NetworkStream stream)
    {
        var response = new Response
        {
            Status = "3 Updated",
            Body = request.Body
        };
        var jason = ToJson(response);
        WriteToStream(stream, jason);
    }
    private void UpdateRequestHandle(Request request, NetworkStream stream)
    {
        var response = new Response
        {
            Status = "3 Updated",
            Body = request.Body
        };
        var jason = ToJson(response);
        WriteToStream(stream, jason);
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
