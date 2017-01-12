using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RestSharp;

namespace Discord01
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Start();
        }

        private DiscordClient _client;

        public void Start()
        {

            _client = new DiscordClient(x =>
            {
                //use this url with the bots client ID to add to server. 
                //https://discordapp.com/oauth2/authorize?client_id=268541960086749185&scope=bot&permissions=0

                x.AppName = "Discord Test Bot"; //actual bot name
                x.AppUrl = ""; //what is this exactly? how does it connect
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;
               

            });

            _client.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
                x.HelpMode = HelpMode.Public;
            });

            var token = "MjY4NTQxOTYwMDg2NzQ5MTg1.C1cmTw.9IZS6AIWT6irllB6GXTzQgS2e54";

            CreateCommands();

            _client.ExecuteAndWait(async () =>
            {
                await _client.Connect(token, TokenType.Bot);
            });

        }
        
        public class Info
        {
            public string data { get; set; }
            
        }

        public class data
        {
            public string name { get; set; }
        }

        public class ResponseContent
        {
            public Info Info { get; set; }
            public List<data> Data { get; set; }
        }

        public void CreateCommands()
        {
            var cService = _client.GetService<CommandService>();

            cService.CreateCommand("ping")
                .Description("Returns some random shit")
                .Do(async (e) =>
                {
                    await e.Channel.SendMessage("I'm a bot and you like dicks");
                });

            cService.CreateCommand("hello")
                .Description("says hello to a user")
                .Parameter("user", Discord.Commands.ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    var toReturn = $"Hello {e.GetArg("user")}";
                    await e.Channel.SendMessage(toReturn);
                });

            //send file based on parameter
            cService.CreateCommand("send file")
                .Description("Sends a file to the channel")
                .Do(async (e) =>
                {
                    await e.Channel.SendFile("5e7.png");
                    await e.Channel.SendMessage("file sent?");
                });

            //basic code for the agora.gg website retrieve elo based on parameter.
            //endpoint to see if a user exists: https://api.agora.gg/players/search/wallaby32
            //then use this to call the api to get stats: https://api.agora.gg/players/921041
            cService.CreateCommand("elo")
                .Description("this will retrieve the paragon elo for specified user. Usage !elo username")
                .Parameter("user", Discord.Commands.ParameterType.Unparsed)
                
                .Do(async (e) =>
                {
                    
                    var ValidUserName = e.GetArg("user");
                    var isValid= $"https://api.agora.gg/players/search/{ValidUserName}";
                    
                    //use RestSharp to get api data
                    var client = new RestClient(isValid);
                    var request = new RestRequest(Method.GET);//need specific elements here
                    Console.WriteLine(isValid);

                    //RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();
                    //IRestResponse<Info> response = client.Execute<Info>(request);

                   
                    //IRestResponse<Info> response2 = client.Execute<Info>(request);

                    //IRestResponse<subitems> response3 = client.Execute<subitems>(request);
                    //IRestResponse<Items> response2 = deserial.Deserialize<Items>(request);

                    //var name = deserial.Deserialize<data>(response);
                    //var name = responseContent.Data;

                    Console.WriteLine(name);
                    //Console.WriteLine(id);
                    //outputs: [{"id":921041,"name":"Wallaby32","namePSN":"Wallaby32","namePreference":"Wallaby32","date":"2017-01-11T00:00:00Z"}]

                    //Console.WriteLine(content);

                    //await e.Channel.SendMessage(isValid);
                    await e.Channel.SendMessage("");             


                });

        }

        public void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine($"[{e.Severity}] [{e.Source}] {e.Message}");
        }
    }
}