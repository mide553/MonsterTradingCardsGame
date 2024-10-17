using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MCTG
{
    class Program
    {
        static List<User> users = new List<User>();
        static List<Card> cards = new List<Card>();

        static void Main(string[] args)
        {
            int port = 10001;
            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Console.WriteLine($"Server started on port {port}");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine(request);

                // Parse the request
                string[] requestLines = request.Split("\r\n");
                string[] requestLine = requestLines[0].Split(" ");
                string method = requestLine[0];
                string path = requestLine[1];


                /*  endpoints   */

                // register 
                if (method == "POST" && path == "/register")
                {
                    HandleRegister(requestLines, stream);
                }
                // login 
                else if (method == "POST" && path == "/login")
                {
                    HandleLogin(requestLines, stream);
                }
                // cards  POST
                else if (method == "POST" && path == "/cards")
                {
                    HandleAddCard(requestLines, stream);
                }
                // cards  GET
                else if (method == "GET" && path == "/cards")
                {
                    HandleGetCards(stream);
                }
                else
                {
                    // invalid endpoint
                    string response = "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nEndpoint not found!";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                }

                client.Close();
            }
        }

        // Registration
        static void HandleRegister(string[] requestLines, NetworkStream stream)
        {
            string body = requestLines[requestLines.Length - 1]; 
            var newUser = JsonConvert.DeserializeObject<User>(body);

            // Add user to in-memory list
            users.Add(new User(newUser.Username, newUser.Password));

            // successful
            string registerResponse = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nUser registered!";
            byte[] responseBytes = Encoding.UTF8.GetBytes(registerResponse);
            stream.Write(responseBytes, 0, responseBytes.Length);
        }

        // Login
        static void HandleLogin(string[] requestLines, NetworkStream stream)
        {
            string body = requestLines[requestLines.Length - 1]; 
            var loginUser = JsonConvert.DeserializeObject<User>(body);

            // if user exists and password matches
            var existingUser = users.Find(u => u.Username == loginUser.Username && u.Password == loginUser.Password);

            if (existingUser != null)
            {
                // Generate a token
                string token = Guid.NewGuid().ToString();
                existingUser.Token = token;

                // Send the token in the response
                string loginResponse = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{{\"token\": \"{token}\"}}";
                byte[] responseBytes = Encoding.UTF8.GetBytes(loginResponse);
                stream.Write(responseBytes, 0, responseBytes.Length);
            }
            else
            {
                // invalid credentials
                string errorResponse = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nInvalid username or password!";
                byte[] responseBytes = Encoding.UTF8.GetBytes(errorResponse);
                stream.Write(responseBytes, 0, responseBytes.Length);
            }
        }

        // New card
        static void HandleAddCard(string[] requestLines, NetworkStream stream)
        {
            string body = requestLines[requestLines.Length - 1];
            var newCard = JsonConvert.DeserializeObject<Card>(body);

            // Add card to in-memory list
            cards.Add(newCard);

            // successful
            string addCardResponse = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nCard added!";
            byte[] responseBytes = Encoding.UTF8.GetBytes(addCardResponse);
            stream.Write(responseBytes, 0, responseBytes.Length);
        }

        // get all cards
        static void HandleGetCards(NetworkStream stream)
        {
            string jsonResponse = JsonConvert.SerializeObject(cards);
            string response = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}";
            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            stream.Write(responseBytes, 0, responseBytes.Length);
        }
    }
}