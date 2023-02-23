using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace bruh
{
    internal class Program
    {
        //Defining public variables
        public DiscordChannel logs;
        public static string echoVRPath = @"C:\Program Files\Oculus\Software\Software\ready-at-dawn-echo-arena\bin\win10\echovr.exe";
        public static string sparkLink = "";
        public static void Main(string[] args)
        {
            //Creates a new local isntance of the Program class...
            var prog = new Program();
            //This runs the RunAsync method in the program class...
            prog.RunAsync().GetAwaiter().GetResult();

        }
        public DiscordClient Client { get; private set; }

        public async Task RunAsync()
        {
            //Defining discord configuration variable...
            var config = new DiscordConfiguration
            {
                Token = "TOKEN HERE",
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.All,
                MinimumLogLevel = LogLevel.Debug
            };

            //Setting discord client configuration
            Client = new DiscordClient(config);

            //Sets on ready listener for the client
            Client.Ready += OnClientReady;
            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            //Creating the service for slash commands
            var services = new ServiceCollection().AddSingleton<Random>().BuildServiceProvider();

            //Defining slashcommand configuration
            var slash = Client.UseSlashCommands(new SlashCommandsConfiguration()
            {
                Services = services
            });

            //Registering slash command configuration
            slash.RegisterCommands<commandClass>();

            //Connects to Discord bot trhough Token
            await Client.ConnectAsync();

            //Keeps program alive after intitialization 
            await Task.Delay(-1);


        }
        public async Task OnClientReady(DiscordClient client, ReadyEventArgs e)
        {
            //On ready command showing the amount of servers bot is connected to 'cause why not
            var guildList = Client.Guilds;
            Console.WriteLine("Connected to: " + guildList.Count + " servers right now!");

        }
    }
}
