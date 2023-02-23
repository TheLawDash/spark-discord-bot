using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static bruh.commandClass;
using static bruh.Program;

namespace bruh
{
    internal class sparkLink
    {
        //Defining public enum JoinType
        public enum JoinType
        {
            Choose,
            Player,
            Spectator
        }
        //Defining public struct instance, recording member, the link, and process ID of echo
        public struct instance
        {
            public DiscordMember member;
            public string lnk;
            public int pid;
        }

        //Public list of instance
        public static List<instance> link = new List<instance>();
        public static async Task startServer(DiscordMember member, ulong guildId, long index)
        {
            //Checks to see if echoVR is installed                            
            if (!string.IsNullOrEmpty(echoVRPath))
            {
                try
                {
                    //Creating local Random class instance
                    Random r = new Random();

                    //Randomly generating port from 0 to 65535 for EchoVR
                    int port = r.Next(0, 65535);

                    //Defining string array of regions for user to choose
                    string[] arenas = { "uscn", "us-central-2", "us-central-3", "use", "usw", "euw", "jp", "aus", "sin" };

                    //Instantiating a process array for existingEcho so it can be referenced throughout 
                    Process[] existingEcho = null;

                    //Instantiating an integer list of PIDs
                    List<int> PIDs = new List<int>();
                    
                    //Try catch to see if there is already an instance of echo open
                    try
                    {
                        //If there is an instance of echo open it is added to the list
                        existingEcho = Process.GetProcessesByName("echovr");
                        foreach(Process process in existingEcho)
                        {
                            PIDs.Add(process.Id);
                        }

                    }
                    catch (Exception ex)
                    {
                        //Exception is thrown if there are no instances...
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine("No Instances...");
                    }
                    
                    //Calls the StartEchoVR Method, passing through the jointype, noovr bool, level type, port, and the region selected...
                    StartEchoVR(
                        JoinType.Spectator,
                        noovr: true,
                        level: "mpl_arena_a",
                        port: port,
                        region: arenas[index]);
                    
                    //Defines the EchoAPI URL
                    string apiURL = "http://127.0.0.1:" + port + "/session";

                    //Prints the EchoAPI URL incase of debugging
                    Console.WriteLine("This is this session's apiUrl : " + apiURL);

                    //Calls apiRequest function to gather the lobby-ID
                    await apiRequest(member, apiURL, PIDs);

                    //Finds the index where member == member
                    int place = link.FindIndex(x => x.member == member);

                    //Creates local instance of echoBot struct
                    echoBot e = new echoBot();

                    //Populates guildID for echoBot instance
                    e.guildId = guildId;

                    //Populates link for echoBot instance
                    e.link = link[place].lnk;

                    //Populates member for echoBot instance
                    e.member = member;

                    //Populates apiURL for echoBot instance
                    e.apiUrl = apiURL;

                    //Populates PID for echoBot instance
                    e.pid = link[place].pid;

                    //Adds the local instance to the public list
                    echoGuild.Add(e);

                    //Removes the old instance from the list link
                    link.RemoveAt(place);
                }
                catch (Exception ex)
                {

                }
            }
        }
        public static bool StartEchoVR(JoinType joinType, int port = 0, bool noovr = false, string session_id = null, string level = null, string region = null)
        {
            //Ensures that echo VR Exists
            if (!string.IsNullOrEmpty(echoVRPath))
            {
                //Setting spectating bool
                bool spectating = joinType == JoinType.Spectator;

                //Starts server by passing port, level, and region
                var startingServer = Process.Start(echoVRPath,
                    ("-spectatorstream ") +
                    ("-noovr ") +
                    ($"-httpport {port} ") +
                    ($"-level {level} ") +
                    ($"-region {region} ")
                );
            }
            else
            {
                return false;
            }

            return true;
        }
        public static async Task apiRequest(DiscordMember member, string apiUrl, List<int> old)
        {
            //Makes a local instance of the class instance
            instance i = new instance();
            Thread.Sleep(1000);
            //Makes a proccess array for echo vr
            Process[] newer = Process.GetProcessesByName("echovr");

            //Makes an integer list for the new EchoVR instance PID
            List<int> newerOne = new List<int>();

            foreach(Process process in newer)
            {
                //Adds the new instance to the list...
                newerOne.Add(process.Id);
            }
            //Compares the new echoVR PID list to the old echoVR PID list
            var different = newerOne.Except(old).ToList();
            while (true)
            {
                //Defines client for API Request
                HttpClient client = new HttpClient();
                try
                {
                    //Trying response for GET request
                    var response = await client.GetAsync(apiUrl);

                    //Loops until 200 response
                    if (response.IsSuccessStatusCode)
                    {
                        using (HttpContent content = response.Content)
                        {
                            //Response saved to string result
                            string result = await content.ReadAsStringAsync();

                            //split is parsing the result
                            List<string> split = result.Split('"').ToList();

                            //Records the index of sessionID
                            int index = split.IndexOf("sessionid");

                            //Saves sessionID to string
                            string sessionId = split[index + 2];

                            //Populates the previously defined local instance of the instance class
                            i.member = member;
                            
                            //Formats the link to be compatible with spark
                            i.lnk = "<spark://c/" + sessionId + ">";

                            //Records the PID of the new echoVR instance
                            i.pid = different[0];

                            //Adds this instance to the list
                            link.Add(i);

                            //Prints the link in console...
                            Console.WriteLine("Found the link: " + "<spark://c/" + sessionId + ">");
                        }
                        break;
                    }
                }
                catch
                {
                    //Hits this is the game is not fully loaded
                    Console.WriteLine("Not Loaded...");
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
