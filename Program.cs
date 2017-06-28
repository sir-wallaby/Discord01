using System;
using System.Net;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;
using Discord01.Properties;
using PUBGSharp;
using System.Globalization;

namespace Discord01
{
    class Program
    {
        static void Main(string[] args)
        {
            //invoke start method
            new Program().Start(); 
        }

        private DiscordClient _client;        
        //start method - includes most areas needed to create the discordbot
        public void Start()
        {

            _client = new DiscordClient(x =>
            {
                //use this url with the bots client ID to add to server. 
                //https://discordapp.com/oauth2/authorize?client_id=268541960086749185&scope=bot&permissions=0
                //actual bot name
                x.AppName = "Discord Test Bot";
                //github link
                x.AppUrl = "https://github.com/sir-wallaby/Discord01"; 
                x.LogLevel = LogSeverity.Verbose;
                x.LogHandler = Log;             

            });

            _client.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
                x.HelpMode = HelpMode.Public;                
            });

            //declare the token from settings
            var token = Discord01.Properties.Settings.Default.Token;
            //invoking commands
            CreateCommands();

            _client.ExecuteAndWait(async () =>
            {
                await _client.Connect(token, TokenType.Bot);
            });


        }

        public void CreateCommands()
        {
            var cService = _client.GetService<CommandService>();
            //testing the create Command method from discord.net
            cService.CreateCommand("ping")
                .Description("Returns Pong")
                .Do(async (e) =>
                {
                    await e.Channel.SendMessage("Pong");
                });
            //hello ping command
            cService.CreateCommand("hello")
                .Description("says hello to a user")
                .Parameter("user", Discord.Commands.ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    var toReturn = $"Hello {e.GetArg("user")}";
                    await e.Channel.SendMessage(toReturn);
                });
            //spread sheet link for Paragon statistics 
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
            
            //TODO:make this a leaner code base. too large and too many IF blocks
            //basic code for the agora.gg website retrieve elo based on parameter.
            //endpoint to see if a user exists: https://api.agora.gg/players/search/wallaby32
            //then use this to call the api to get stats: https://api.agora.gg/players/921041
            cService.CreateCommand("elo")
                .Description("this will retrieve the paragon elo for specified user. Usage !elo username")
                .Parameter("user", Discord.Commands.ParameterType.Unparsed)

                .Do(async (e) =>
                {
                    await e.Channel.SendIsTyping();
                    var userName = e.GetArg("user"); //you can use spaces so removed the replace method.                    
                    var searchValue = $"https://api.agora.gg/players/search/{userName}";

                    //open webclient
                    WebClient client = new WebClient();

                    //download string into url variable
                    string url = client.DownloadString(searchValue);
                    client.Dispose();

                    //parsing Json Data from downloaded url
                    dynamic agoraData = JObject.Parse(url);
                    int agoraPlayerId = agoraData.data[0].id;
                    string name = agoraData.data[0].name;

                    //logic for pc names versus ps4 names.
                    //note: will default to PC name if they are different names, including cases.
                    if (String.IsNullOrEmpty(name))
                    {
                        agoraPlayerId = agoraData.data[1].id;
                    }
                    else
                    {
                        agoraPlayerId = agoraData.data[0].id;
                    }

                    //using player id grab stats from api
                    var playerStatsPage = $"https://api.agora.gg/players/{agoraPlayerId}";

                    //new web client
                    WebClient statsClient = new WebClient();

                    //download json data as a string into statsURL variable
                    string statsUrl = client.DownloadString(playerStatsPage);
                    client.Dispose();

                    //parse json data from statsurl
                    var agoraPlayerStatsData = JObject.Parse(statsUrl);
                    int numberOfStatItems = agoraPlayerStatsData.SelectToken("data.stats").Count();
                    var pvpData = agoraPlayerStatsData.SelectTokens("data.stats[*].mode");
                    var multiStatData = agoraPlayerStatsData.SelectTokens("data.stats[*]");
                    string multiStatDataDeserialed = JsonConvert.SerializeObject(multiStatData);

                    //second attempt
                    dynamic agoraPlayerStatsDataDynamic = JObject.Parse(statsUrl);

                    //if statement variables (I believe this has to do with a changing API results. need to check on.
                    dynamic agoraSingleItemStats = 0;
                    dynamic multipleValuesFromStats0 = 0;
                    dynamic multipleValuesFromStats1 = 0;
                    dynamic multipleValuesFromStats2 = 0;
                    dynamic multipleValuesFromStats3 = 0;
                    dynamic mutipleOutput0 = 0;
                    dynamic mutipleOutput1 = 0;
                    dynamic multpleOutput2 = 0;
                    dynamic finalPlayerStats = 0;

                    //if structure to help determine which stats to use.
                    if (numberOfStatItems == 1)
                    {
                        agoraSingleItemStats = agoraPlayerStatsDataDynamic.data.stats[0];
                        finalPlayerStats = agoraSingleItemStats;
                    }


                    if (numberOfStatItems == 2)
                    {
                        multipleValuesFromStats0 = agoraPlayerStatsDataDynamic.data.stats[0];
                        multipleValuesFromStats1 = agoraPlayerStatsDataDynamic.data.stats[1];

                        string modeValue0 = multipleValuesFromStats0.mode;
                        string modeValue1 = multipleValuesFromStats1.mode;

                        if (modeValue0 == "4")
                        {
                            mutipleOutput0 = multipleValuesFromStats0;
                        }
                        if (modeValue1 == "4")
                        {
                            mutipleOutput0 = multipleValuesFromStats1;
                        }

                        finalPlayerStats = mutipleOutput0;
                    }

                    if (numberOfStatItems == 3)
                    {
                        multipleValuesFromStats0 = agoraPlayerStatsDataDynamic.data.stats[0];
                        multipleValuesFromStats1 = agoraPlayerStatsDataDynamic.data.stats[1];
                        multipleValuesFromStats2 = agoraPlayerStatsDataDynamic.data.stats[2];

                        string modeValue0 = multipleValuesFromStats0.mode;
                        string modeValue1 = multipleValuesFromStats1.mode;
                        string modeValue2 = multipleValuesFromStats2.mode;

                        if (modeValue0 == "4")
                        {
                            mutipleOutput1 = multipleValuesFromStats0;
                        }
                        if (modeValue1 == "4")
                        {
                            mutipleOutput1 = multipleValuesFromStats1;
                        }
                        if (modeValue2 == "4")
                        {
                            mutipleOutput1 = multipleValuesFromStats2;
                        }

                        finalPlayerStats = mutipleOutput1;

                    }

                    if (numberOfStatItems == 4)
                    {
                        multipleValuesFromStats0 = agoraPlayerStatsDataDynamic.data.stats[0];
                        multipleValuesFromStats1 = agoraPlayerStatsDataDynamic.data.stats[1];
                        multipleValuesFromStats2 = agoraPlayerStatsDataDynamic.data.stats[2];
                        multipleValuesFromStats3 = agoraPlayerStatsDataDynamic.data.stats[3];

                        string modeValue0 = multipleValuesFromStats0.mode;
                        string modeValue1 = multipleValuesFromStats1.mode;
                        string modeValue2 = multipleValuesFromStats2.mode;
                        string modeValue3 = multipleValuesFromStats3.mode;

                        if (modeValue0 == "4")
                        {
                            mutipleOutput1 = multipleValuesFromStats0;
                        }
                        if (modeValue1 == "4")
                        {
                            mutipleOutput1 = multipleValuesFromStats1;
                        }
                        if (modeValue2 == "4")
                        {
                            mutipleOutput1 = multipleValuesFromStats2;
                        }
                        if (modeValue2 == "4")
                        {
                            mutipleOutput1 = multipleValuesFromStats3;
                        }

                        finalPlayerStats = mutipleOutput1;

                    }
                    //store the actual elo
                    decimal elo = finalPlayerStats.elo;
                    //store wins/ losses / etc for other stats
                    double wins = finalPlayerStats.wins;
                    double gamesplayed = finalPlayerStats.gamesPlayed;
                    double winPercentage = Math.Round((wins / gamesplayed),3);
                    double kills = finalPlayerStats.kills; double deaths = finalPlayerStats.deaths; double assists = finalPlayerStats.assists;
                    double kd = (kills + assists);
                    double kda = (kd / deaths);
                    
                    //OUTput of the entire BLOCK
                    await e.Channel.SendMessage("Agora Elo: " + elo.ToString() + "\n" +
                                                "Wins: " + wins.ToString() + "\n" +
                                                "Games Played: " + gamesplayed.ToString() + "\n" +
                                                "Win Percentage:" + winPercentage.ToString() + "\n" +
                                                "KDA:" + kda.ToString());
                });

            cService.CreateCommand("clearchat")
            .AddCheck((cmd, u, ch) => u.ServerPermissions.Administrator)
             .Do(async (e) =>
             {
                 Discord.Message[] messagesToDelete;
                 messagesToDelete = await e.Channel.DownloadMessages(100);

                 await e.Channel.DeleteMessages(messagesToDelete);

             });

            cService.CreateCommand("ud")
                .Description("Urban Dictonary Definitions")
                .Parameter("word", Discord.Commands.ParameterType.Unparsed)

                .Do(async (e) =>
                {
                    try
                    {
                        var searchTerm = e.GetArg("word"); //you can use spaces so removed the replace method.                    
                        var searchValue = $"http://api.urbandictionary.com/v0/define?term={searchTerm}";

                        //open webclient
                        WebClient client = new WebClient();

                        //download string into url variable
                        string url = client.DownloadString(searchValue);
                        client.Dispose();

                        //parsing Json Data from downloaded url
                        dynamic definition = JObject.Parse(url);
                        //output definition
                        string output = definition.list[0].definition;
                        JObject Json = JObject.Parse(url);
                        string noResult = Json.GetValue("result_type").ToString();


                        if (noResult == "exact")
                        {

                            await e.Channel.SendIsTyping();
                            await e.Channel.SendMessage("Definition: ```" + output.ToString() + "```");
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        await e.Channel.SendIsTyping();
                        await e.Channel.SendMessage("```"+ "No results found" + "```");
                    }               
                    

                    


                });


            cService.CreateCommand("sq")
                .Description("Grabs stats for PUBG online from PUBG Tracker")
                .Parameter("user", Discord.Commands.ParameterType.Unparsed)
            .Do(async (e) =>
            {
                await e.Channel.SendIsTyping();
                var pubgUsername_PC = e.GetArg("user").Replace(" ","");

                string apiKey = Discord01.Properties.Settings.Default.TRN_Api_Key;
                //create statsClient with proper api key
                var statsClient = new PUBGStatsClient(apiKey);
                //use wrapper to call the api
                var stats = await statsClient.GetPlayerStatsAsync(pubgUsername_PC);                            

                try
                {
                    var SquadRank = stats.Stats.Find(x =>
                        x.Mode == Mode.Squad && x.Region == PUBGSharp.Region.NA && x.Season == Season.EASeason2).Stats.Find(x =>
                            x.Stat == Stats.Rating).Rank;

                    var KDR = stats.Stats.Find(x =>
                        x.Mode == Mode.Squad && x.Region == PUBGSharp.Region.NA && x.Season == Season.EASeason2).Stats.Find(x =>
                            x.Stat == Stats.KDR).Value;

                    var kills = stats.Stats.Find(x =>
                        x.Mode == Mode.Squad && x.Region == PUBGSharp.Region.NA && x.Season == Season.EASeason2).Stats.Find(x =>
                            x.Stat == Stats.Kills).Value;

                    var wins = stats.Stats.Find(x =>
                        x.Mode == Mode.Squad && x.Region == PUBGSharp.Region.NA && x.Season == Season.EASeason2).Stats.Find(x =>
                            x.Stat == Stats.Wins).Value;

                    var win_percentage = stats.Stats.Find(x =>
                        x.Mode == Mode.Squad && x.Region == PUBGSharp.Region.NA && x.Season == Season.EASeason2).Stats.Find(x =>
                            x.Stat == Stats.WinPercentage).Value;

                    var roundsPlayed = stats.Stats.Find(x =>
                        x.Mode == Mode.Squad && x.Region == PUBGSharp.Region.NA && x.Season == Season.EASeason2).Stats.Find(x =>
                            x.Stat == Stats.RoundsPlayed).Value;

                    var KPG = stats.Stats.Find(x =>
                       x.Mode == Mode.Squad && x.Region == PUBGSharp.Region.NA && x.Season == Season.EASeason2).Stats.Find(x =>
                           x.Stat == Stats.KillsPerGame).Value;
                    
                    var longestkill = stats.Stats.Find(x =>
                      x.Mode == Mode.Squad && x.Region == PUBGSharp.Region.NA && x.Season == Season.EASeason2).Stats.Find(x =>
                          x.Stat == Stats.LongestKill).Value;

                    var KO = stats.Stats.Find(x =>
                      x.Mode == Mode.Squad && x.Region == PUBGSharp.Region.NA && x.Season == Season.EASeason2).Stats.Find(x =>
                          x.Stat == Stats.Knockouts).Value;

                    var rating = stats.Stats.Find(x =>
                      x.Mode == Mode.Squad && x.Region == PUBGSharp.Region.NA && x.Season == Season.EASeason2).Stats.Find(x =>
                          x.Stat == Stats.Rating).Value;

                    double knockoutsPerGame = Math.Round((double.Parse(KO) / double.Parse(roundsPlayed)),3);

                    await e.Channel.SendMessage("\n```Rank: " + SquadRank.ToString() + "\n" +
                                                "Rating: " + rating.ToString() + "\n" + "\n" +

                                                "KDR: " + KDR.ToString() + "\n"+
                                                "Kills: " + kills.ToString() + "\n" +
                                                "Kills/Game: " + KPG.ToString() + "\n" +
                                                "Knockouts: " + KO.ToString() + "\n" +
                                                "Knockouts/Game: " + knockoutsPerGame.ToString() + "\n" +
                                                "Longest Kill: " + longestkill.ToString() + " meters \n" + "\n" +

                                                "Wins: " + wins.ToString() + "\n" +
                                                "Win Percentage: " + win_percentage + "\n" +
                                                "Games Played: " + roundsPlayed + "\n" + "```");

                    await e.Channel.SendMessage("Stats last updated at: " + stats.LastUpdated);
                }

                catch (PUBGSharpException)
                {

                    throw;
                    //await e.Channel.SendMessage("Could not retrieve stats for " + pubgUsername_PC + " error: " + ex.Message);
                }

                catch (ArgumentNullException excep)
                {
                    await e.Channel.SendMessage("Could not retrieve stats for " + pubgUsername_PC + " error: " + excep.Message);
                }

            });


                }

        public void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine($"[{e.Severity}] [{e.Source}] {e.Message}");
        }


    }
}
