using System;
using System.IO;
using System.Net;
using System.Security;
using System.Text;
using Newtonsoft.Json;
using VkNet;
using VkNet.Enums;
using VkNet.Enums.Filters;
using VkNet.Model;

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
    class Program
    {
        static string ReadMaskedInput(string preview, bool masked)
        {
            StringBuilder passwordBuilder = new StringBuilder();
            bool pwdEnters = true;
            Console.WriteLine(preview);
            while (pwdEnters)
            {
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);
                switch (consoleKeyInfo.Key)
                {
                    case ConsoleKey.Backspace:
                        if (passwordBuilder.Length > 0)
                        {
                            passwordBuilder.Remove(passwordBuilder.Length - 1, 1);
                            Console.Clear();
                            Console.WriteLine(preview);
                            if (masked)
                            {
                                for (int i = 0; i < passwordBuilder.Length - 1; ++i)
                                    Console.Write('*');
                            }
                            else
                            {
                                Console.Write(passwordBuilder.ToString());
                            }
                        }
                        else
                        {
                            Console.Clear();
                            Console.WriteLine(preview);
                        }
                        break;
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        pwdEnters = false;
                        break;
                    default:
                        if (masked)
                        {
                            Console.Write('*');
                        }
                        else
                        {
                            Console.Write(consoleKeyInfo.KeyChar);
                        }
                        passwordBuilder.Append(consoleKeyInfo.KeyChar);
                        break;
                }
            }
            Console.Clear();
            return passwordBuilder.ToString();
        }

        static void Main(string[] args)
        {
            bool success = false;
            string token = "";
            if (token.Length > 0)
                success = true;
            while (!success)
            {
                VkApi api = new VkApi();
                ApiAuthParams authParams = new ApiAuthParams();
                authParams.ApplicationId = 7650154;
                authParams.Login = ReadMaskedInput("Enter login:", false);
                authParams.Password = ReadMaskedInput("Enter password:", true);
                Console.Clear();
                authParams.Settings = Settings.All;
                authParams.Code = 425249.ToString();
                try
                {
                    authParams.TwoFactorAuthorization = () =>
                    {
                        return ReadMaskedInput("Enter code:", true);
                    };
                    api.Authorize(authParams);
                    token = api.Token;
                    success = true;
                    Console.Clear();
                }
                catch (Exception)
                {
                    Console.WriteLine("Authorization went wrong!");
                }
            }

            WebRequest request = WebRequest.Create("https://api.vk.com/method/users.get?v=5.124&access_token=" + token);

            try
            {
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
                string userResponse = streamReader.ReadToEnd();

                UsrResponse r = new UsrResponse();
                r.response = new Usr[1];
                r.response[0] = new Usr();
                r.response[0].can_access_closed = true;
                r.response[0].first_name = "Павел";
                r.response[0].last_name = "Спецаков";
                r.response[0].is_closed = false;
                r.response[0].id = 32470000;

                string json = JsonConvert.SerializeObject(r);
                UsrResponse usrResponse = JsonConvert.DeserializeObject<UsrResponse>(userResponse);
                Console.WriteLine("Welcome, " + usrResponse.response[0].first_name + " " +
                                  usrResponse.response[0].last_name);
            }
            catch (Exception)
            {
                Console.WriteLine("Something went wrong!");
            }

            Console.ReadKey();
        }
    }
}
