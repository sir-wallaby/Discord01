using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;


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
                    await e.Channel.SendMessage("I'm a bot and I like dicks");
                });

            cService.CreateCommand("hello")
                .Description("says hello to a user")
                .Parameter("user", ParameterType.Unparsed)
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

        }

        public void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine($"[{e.Severity}] [{e.Source}] {e.Message}");
        }
    }
}