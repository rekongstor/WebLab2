using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebLab2
{
    class UsrResponse
    {
        public Usr[] response;
    }

    class Usr
    {
        public string first_name;
        public int id;
        public string last_name;
        public bool can_access_closed;
        public bool is_closed;
    }

    class TokenResponse
    {
        public string access_token;
        public int expires_in;
        public int user_id;
    }


    class Program
    {
        static IPAddress localhost = IPAddress.Parse("127.0.0.1");
        static int port = 80;
        static TcpListener serverListener;
        static private bool ready = false;
        private static Socket socket;
        public static string token;

        static async void ServerListener()
        {
            serverListener = new TcpListener(localhost, port);
            serverListener.Start();
            Console.WriteLine($"Server: Listening on {localhost}:{port}");
            ready = true;
            while (true)
            {
            }
        }

        static void RequestName()
        {
            WebRequest request = WebRequest.Create("https://api.vk.com/method/users.get?v=5.124&access_token=" + token);

            try
            {
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
                string userResponse = streamReader.ReadToEnd();

                UsrResponse usrResponse = JsonConvert.DeserializeObject<UsrResponse>(userResponse);
                Console.WriteLine("Welcome, " + usrResponse.response[0].first_name + " " +
                                  usrResponse.response[0].last_name);
            }
            catch
            {
                Console.WriteLine("Something went wrong!");
            }

            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            Task.Run(() => ServerListener());

            while (token == null)
            {
                if (!ready)
                {
                    continue;
                }

                socket = serverListener.AcceptSocketAsync().Result;
                //Console.WriteLine("Connected: " + socket.LocalEndPoint);

                Task.Run(() => ReadStream());
            }
            RequestName();
        }

        static async void ReadStream()
        {
            while (socket.Connected)
            {
                NetworkStream stream = new NetworkStream(socket);
                StreamReader streamReader = new StreamReader(stream);
                String data = await streamReader.ReadLineAsync();
                //Console.WriteLine(data);
                try
                {
                    token = "";
                    if (data != null && data.Contains("code="))
                    {
                        String code = data.Remove(0, 11);
                        code = code.Split(' ')[0];
                        socket.Send(System.Text.Encoding.ASCII.GetBytes("OK".ToCharArray()));
                        WebRequest request = WebRequest.Create("https://oauth.vk.com/access_token?client_id=7650154&client_secret=QxAZHgO0H7W4XadDVw2T&redirect_uri=http://localhost:80&code=" + code);
                        WebResponse response = request.GetResponse();
                        Stream streamToken = response.GetResponseStream();
                        StreamReader streamReaderToken = new StreamReader(streamToken);
                        string tokenResponse = streamReaderToken.ReadToEnd();
                        token = JsonConvert.DeserializeObject<TokenResponse>(tokenResponse).access_token;
                        socket.Close();
                    }
                }
                catch
                {
                    Console.WriteLine("Can't parse the token or code");
                }
            }
        }
    }
}

