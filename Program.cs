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

            //spread sheet link. 
            cService.CreateCommand("numbers")
                .Description("Sends a link to a useful paragon spreadsheet")
                .Do(async (e) =>
                {
                    await e.Channel.SendMessage("https://docs.google.com/spreadsheets/d/1_J9pKPGp1ddmSpNB_jcFhjjq_OPSILDmjdwzqre3i3U/edit#gid=1224517746");
                    
                });

            //send file based on parameter
            cService.CreateCommand("elochart")
                .Description("Shows a snippet of the elo breakdown for agora.gg to the channel")
                .Do(async (e) =>
                {
                    await e.Channel.SendFile("EloBrackets.png");
                    //await e.Channel.SendMessage("file sent?");
                });

            //basic code for the agora.gg website retrieve elo based on parameter.
            //endpoint to see if a user exists: https://api.agora.gg/players/search/wallaby32
            //then use this to call the api to get stats: https://api.agora.gg/players/921041
            //Maybe create another elo command that is able to search on psnID?
            cService.CreateCommand("elo")
                .Description("this will retrieve the paragon elo for specified user. Usage !elo username")
                .Parameter("user", Discord.Commands.ParameterType.Unparsed)
                
                .Do(async (e) =>
                {
                    
                    var userName = e.GetArg("user"); //you can use spaces so removed the replace method.                    
                    var isValid= $"https://api.agora.gg/players/search/{userName}";

                    WebClient client = new WebClient();
                    string url = client.DownloadString(isValid);
                    client.Dispose();

                    dynamic agoraData = JObject.Parse(url);
                    int agoraPlayerId = agoraData.data[0].id;                    
                    string name = agoraData.data[0].name;                                     
                    //account for pc names
                    if (String.IsNullOrEmpty(name))
                    {
                        agoraPlayerId = agoraData.data[1].id;
                    }
                    else
                    {
                        agoraPlayerId = agoraData.data[0].id;
                    }

                        //Console.WriteLine(agoraPlayerId.ToString());
                        //Console.WriteLine(agoraPcId.ToString());

                        //using player id grab stats from api
                        var playerStatsPage = $"https://api.agora.gg/players/{agoraPlayerId}";
                    WebClient statsClient = new WebClient();
                    string statsUrl = client.DownloadString(playerStatsPage);
                    client.Dispose();

                    dynamic agoraPlayerStatsData = JObject.Parse(statsUrl);                     
                    //store the actual elo
                    decimal elo = agoraPlayerStatsData.data.stats[0].elo;
                    //store wins/losses/etc for other stats
                    double wins = agoraPlayerStatsData.data.stats[0].wins;
                    double gamesplayed = agoraPlayerStatsData.data.stats[0].gamesPlayed;
                    double winPercentage = (wins / gamesplayed);
                    double kills = agoraPlayerStatsData.data.stats[0].kills; double deaths = agoraPlayerStatsData.data.stats[0].deaths; double assists = agoraPlayerStatsData.data.stats[0].assists;
                    double kd = (kills + assists);
                    double kda = (kd / deaths);

                    //Console.WriteLine(kd);
                    //Console.WriteLine(kda);


                    //Console.WriteLine(agoraPlayerStatsData.data.stats[0].elo);

                    //await e.Channel.SendMessage("");
                    await e.Channel.SendMessage("Agora Elo: " + elo.ToString() + "\n" +
                                                "Wins: " + wins.ToString() + "\n" +
                                                "Games Played: " + gamesplayed.ToString() + "\n" +
                                                "Win Percentage:" + winPercentage.ToString() + "\n" +
                                                "KDA:" + kda.ToString());


                });

        }

        public void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine($"[{e.Severity}] [{e.Source}] {e.Message}");
        }
    }
}