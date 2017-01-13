using System;
using System.Net;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                    
                    var userName = e.GetArg("user").Replace(" ","");                    
                    var isValid= $"https://api.agora.gg/players/search/{userName}";

                    WebClient client = new WebClient();
                    string url = client.DownloadString(isValid);
                    client.Dispose();

                    dynamic agoraData = JObject.Parse(url);
                    int agoraPlayerId = agoraData.data[0].id;
                    //retrieve playerid and assign to int
                    //also if statment to find out if PC player or not if so, then return second set of results. 
                    if (agoraData.data[1].id != null)
                    {
                        agoraPlayerId = agoraData.data[1].id;
                    }
                    else
                    {
                        agoraPlayerId = agoraData.data[0].id;
                    }

                    //Console.WriteLine(agoraPlayerId.ToString());
                    
                    //using player id grab stats from api
                    var playerStatsPage = $"https://api.agora.gg/players/{agoraPlayerId}";
                    WebClient statsClient = new WebClient();
                    string statsUrl = client.DownloadString(playerStatsPage);
                    client.Dispose();

                    dynamic agoraPlayerStatsData = JObject.Parse(statsUrl);                     
                    //store the actual elo
                    decimal elo = agoraPlayerStatsData.data.stats[0].elo;

                    //Console.WriteLine(agoraData.data[0].id);
                    //Console.WriteLine(agoraData.data[1].id);

                    //Console.WriteLine(agoraPlayerStatsData.data.stats[0].elo);

                    //await e.Channel.SendMessage("");
                    await e.Channel.SendMessage("Agora Elo: " + elo.ToString());


                });

        }

        public void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine($"[{e.Severity}] [{e.Source}] {e.Message}");
        }
    }
}