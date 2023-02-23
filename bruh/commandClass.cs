using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using static bruh.sparkLink;
using System.Timers;
using System.Security.Cryptography;

namespace bruh
{
    internal class commandClass : ApplicationCommandModule
    {
        //Defining the public struct for echoBot
        public struct echoBot
        {
            public ulong guildId;
            public DiscordMember member;
            public string apiUrl;
            public string link;
            public int pid;
        }

        //Defining public list for struct echoBot
        public static List<echoBot> echoGuild = new List<echoBot>();
        public DiscordChannel channel;

        //Defining the slash command spark with the different options and choices
        [SlashCommand("spark", "This command is used to pull a server and produce a spark link for the user.")]
        public async Task pullLink(InteractionContext ctx, 
            [Choice("US Central North", 0)][Choice("Idfk", 1)][Choice("US Central South", 2)][Choice("NA East", 3)]
            [Choice("NA West", 4)][Choice("Eruope", 5)][Choice("Japan", 0)][Choice("Austrailia", 6)][Choice("Singapore", 7)][Option("region", "Choose server location")] long server = 0,
            [Choice("Public", "public")][Choice("Private", "private")][Option("privacy", "Choose to have it private or sent publicly")] string privacy = "public")
        {
            //Responds to the user telling them the spark link is being created
            await ctx.CreateResponseAsync("`Creating spark link now!`");

            //Gets server guild ID
            var guild = ctx.Guild.Id;

            //Defines member and sets it to the variable member
            var member = ctx.Member;

            //Runs startServer passing through the member, guild, and server choice
            await sparkLink.startServer(member, guild, server);

            //Pulls the index from echoGuild for the instance the discord member 
            int index = echoGuild.FindIndex(x => x.guildId == guild && x.member == member);

            //Sets local pid to desired PID instance
            int pid = echoGuild[index].pid;

            //Creates link message
            string linkStuff = member.Mention + "` Here is your spark link!` " + echoGuild[index].link;
            if (privacy == "public")
            {
                //Sends this message publicly
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder() { Content = linkStuff});
            }
            else
            {
                try
                {
                    //This will send as a ephemeral message -- notvisible to anyone else except the user
                    DiscordMessageBuilder message = new DiscordMessageBuilder();
                    message.Content = linkStuff;
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder(message).AsEphemeral(true));
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            //Calls for the bot to sit and wait for the user to join the match instance
            await checkJoin(pid, index);
        }
        public static async Task checkJoin(int PID, int ind)
        {
            //Sets a timer for 5 minutes waiting for the user to join
            System.Timers.Timer t = new System.Timers.Timer();
            t.Interval = 300000;
            //At the 5 minute mark it will run the function closeServer, which will use the PID and close that instance
            t.Elapsed += (sender, e) => closeServer(sender, e, PID);
            t.Enabled = true;
            //Loop to check API
            while (true)
            {
                //Defining HttpClient for apiRequest
                HttpClient client = new HttpClient();
                try
                {
                    //Sets echo process based off of PID
                    Process echo = Process.GetProcessById(PID);
                    
                    //If echo is closed it will break this loop
                    if (echo.MainWindowTitle == "")
                        break;
                    
                    //Saves response to variable response
                    var response = await client.GetAsync(echoGuild[ind].apiUrl);

                    //Waits for code 200 before moving in
                    if (response.IsSuccessStatusCode)
                    {
                        using (HttpContent content = response.Content)
                        {
                            //Saves API response to string
                            string result = await content.ReadAsStringAsync();

                            //Parses string to see if member joins
                            List<string> split = result.Split('"').ToList();

                            //Instantiates integer for parsing
                            int count = 0;

                            //Looking for string name
                            string param = "name";

                            //Foreach loop to itterate through the parsed string
                            foreach (string attribute in split)
                            {
                                if (attribute == param)
                                {
                                    count += 1;
                                }
                            }
                            if (count > 1)
                            {
                                //Closes the bot instance if a player joins and goes back to the waiting event
                                Process proc = Process.GetProcessById(PID);
                                proc.Kill();
                                t.Dispose();
                                echoGuild.RemoveAt(ind);
                                break;
                            }
                        }
                    }
                    Thread.Sleep(2000);
                }
                catch
                {
                    //Shouldn't hit this if echo is running and functional...
                    Console.WriteLine("Crashed(?)...");
                    Thread.Sleep(1000);
                }
            }
        }

        private static void closeServer(object? sender, ElapsedEventArgs e, int pID)
        {
            //Closes process based off of PID
            Process proc = Process.GetProcessById(pID);
            proc.Kill();
        }
    }
}
