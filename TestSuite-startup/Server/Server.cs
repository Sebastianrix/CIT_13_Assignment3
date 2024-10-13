using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


public class Server
{
    private readonly int _port;
    private static List<Category> categories = new List<Category>();
    private static int nextCategoryId = 1;

    // JSON options to use camelCase property names
    private static readonly JsonSerializerOptions options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase};



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
            string[] validMethods = ["create", "read", "update", "delete", "echo"];


            if (string.IsNullOrWhiteSpace(msg))
            {
                var response = new Response { Status = "missing method" };
                WriteToStream(stream, ToJson(response));
                Console.WriteLine(response.Status);
                return;
            }

            if (msg.Trim() == "{}") 
            {
                var response = new Response { Status = "missing method, missing date" };
                WriteToStream(stream, ToJson(response));
                Console.WriteLine(response.Status);
                return;
            }

            var request = FromJson(msg);

            // Validate Date
            if (string.IsNullOrEmpty(request.Date))
            {
                var response = new Response { Status = "4 missing date" };
                WriteToStream(stream, ToJson(response));
                Console.WriteLine(response.Status);
                return;
            }
            // Validate Method
            if (string.IsNullOrWhiteSpace(request.Method))
            {
                var response = new Response { Status = "missing method" };
                WriteToStream(stream, ToJson(response));
                Console.WriteLine(response.Status);
                return;
            }

            if (!validMethods.Contains(request.Method.ToLower()))
            {
                var response = new Response { Status = "4 Illegal method" };
                WriteToStream(stream, ToJson(response));
                Console.WriteLine(response.Status);
                return;
            }
            // Validate if Path is empty
            if (request.Method.ToLower() != "echo" && string.IsNullOrEmpty(request.Path))
            {
                var response = new Response { Status = "4 missing resource" };
                WriteToStream(stream, ToJson(response));
                Console.WriteLine(response.Status);
                return;
            }
            // Validate Body for methods that require it
            if (request.Method.ToLower() != "read" && request.Method.ToLower() != "delete" && string.IsNullOrEmpty(request.Body))
            {
                var response = new Response { Status = "4 missing body" };
                WriteToStream(stream, ToJson(response));
                Console.WriteLine(response.Status);
                return;
            }
            // Validate body for Jason in methods that require it
            if ((request.Method.ToLower() == "create" || request.Method.ToLower() == "update") && !IsValidJason(request.Body))
            {
                var response = new Response { Status = "4 illegal body" };
                WriteToStream(stream, ToJson(response));
                Console.WriteLine(response.Status);
                return;
            }
      

            if (!IsValidUnixTime(request.Date))
            {
                var response = new Response { Status = "4 Illegal date" };
                WriteToStream(stream, ToJson(response));
                Console.WriteLine(response.Status);
                return;
            }


            

            // Validate if Path is correct
            if (request.Method.ToLower() !="echo" && !IsValidPath(request.Path))
            {
                var response = new Response { Status = "4 Bad Request" };
                WriteToStream(stream, ToJson(response));
                Console.WriteLine(response.Status);
                return;
            }
            // Validate that create doesn't have ID
            if (request.Method.ToLower() == "create" && IsValidID(request.Path))
            {
                var response = new Response { Status = "4 Bad Request" };
                WriteToStream(stream, ToJson(response));
                Console.WriteLine(response.Status);
                return;
            }
            if (request.Method.ToLower() == "read" && !IsValidLastSegment(request.Path))
            {
                var response = new Response { Status = "4 Bad Request" };
                WriteToStream(stream, ToJson(response));
                Console.WriteLine(response.Status);
                return;
            }
            // Validate ID for Read, Update and Delete
            if ((/*request.Method.ToLower() == "read" ||*/ request.Method.ToLower() == "update" || request.Method.ToLower() == "delete") && !IsValidID(request.Path))
            {
                var response = new Response { Status = "4 Bad Request" };
                WriteToStream(stream, ToJson(response));
                Console.WriteLine(response.Status);
                return;
            }
            // Validate ID if its within range of the ID list
            if ((/*request.Method.ToLower() == "read" ||*/ request.Method.ToLower() == "update" || request.Method.ToLower() == "delete") && !IsValidInt(request.Path))
            {
                var response = new Response { Status = "5 Not found" };
                WriteToStream(stream, ToJson(response));
                Console.WriteLine(response.Status);
                return;
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
                    break;
            }



        }
        catch { }
    }

    // Request handlers
    private void EchoRequestHandle(Request request, NetworkStream stream)
    {
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
        var newCategory = JsonSerializer.Deserialize<Category>(request.Body, options);

        newCategory.Id = nextCategoryId++;
        categories.Add(newCategory);

        var response = new Response
        {
            Status = "2 Created",
            Body = JsonSerializer.Serialize(newCategory, options)
        };
        WriteToStream(stream, ToJson(response));
    }


    private void ReadRequestHandle(Request request, NetworkStream stream)
    {
        Response response;

        // Check if the path ends with an ID
        if (IsValidInt(request.Path))
        {
            // Fetch the specific category by ID
            var category = categories[GetIdFromPath(request.Path)];
            response = new Response
            {
                Status = "1 Ok",
                Body = JsonSerializer.Serialize(category, options)
            };
            Console.WriteLine(response.Status + " " + response.Body);
        }
        else
        {
            // Return all categories
            response = new Response
            {
                Status = "1 Ok",
                Body = JsonSerializer.Serialize(categories, options)

            };
            Console.WriteLine(response.Status + " " + response.Body);
        }
        var jsonResponse = ToJson(response);
        WriteToStream(stream, jsonResponse);
    }
    private void DeleteRequestHandle(Request request, NetworkStream stream)
    {
        var response = new Response
        {
            Status = "1 Ok",
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
    private bool IsValidUnixTime(string date)
    {
        return long.TryParse(date, out long timestamp) && timestamp > 0;
    }

    private bool IsValidJason(string body) //inspired by : https://code-maze.com/csharp-how-to-ensure-a-string-is-valid-json/
    {
        try
        {
            JsonDocument.Parse(body);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
    private static bool IsValidLastSegment(string path) //Validates there is 3 segments and last is a number or not
    {
        string[] SegmentsPath = path.Split('/');
        if (SegmentsPath.Length == 4 && !int.TryParse(SegmentsPath[3], out _))
        {
            return false;
        }
        else
        {
            return true;
        }

    }
    private bool IsValidPath(string path)
    {
        if (path.StartsWith("/api/categories"))
        {
            return true;
        }
        else return false;
    }
    private bool IsValidInt(string path)  //Validates if the ID is a number and if it is in the list
    {
        string[] SegmentsPath = path.Split('/');
        if (int.TryParse(SegmentsPath[^1], out int id) && id <= nextCategoryId)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private bool IsValidID(string path) //Validates if the ID is a number
    {
        string[] SegmentsPath = path.Split('/');
        if (int.TryParse(SegmentsPath[^1],out _))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private static int GetIdFromPath(string path)
    {
        string[] segments = path.Split('/');
        return int.Parse(segments[^1]);
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
