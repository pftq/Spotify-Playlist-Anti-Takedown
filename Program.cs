using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.Windows.Forms;

namespace SpotifyPlaylistAntiTakedown
{
    class Program
    {


        private static string accountToken = "";
        private static Dictionary<string, Dictionary<string, string>> playlists = new Dictionary<string, Dictionary<string, string>>()
        {
            
        };

        [STAThread]
        static void Main(string[] args)
        {
            
            
                while (true)
                {
                    if (accountToken == "")
                    {
                        Console.WriteLine("Input Spotify OAUTH token to your account.  Copy from https://developer.spotify.com/console/put-playlist/");
                        if (Properties.Settings.Default.accountToken != "") Console.WriteLine("Hit enter to use previous token: " + Properties.Settings.Default.accountToken);
                        accountToken = Console.ReadLine();
                        if (accountToken == "" && Properties.Settings.Default.accountToken != "") accountToken = Properties.Settings.Default.accountToken;
                        else if (accountToken != "")
                        {
                            Properties.Settings.Default.accountToken = accountToken;
                            Properties.Settings.Default.Save();
                        }
                    }
                    if (accountToken != "")
                    {
                        try
                        {
                            GetPlaylists();
                            foreach (KeyValuePair<string, Dictionary<string, string>> playlist in playlists)
                            {
                                if (GetPlaylist(playlist.Key, playlist.Value["Name"]) == "" && playlist.Value["Name"]!="")
                                    UpdatePlaylist(playlist.Key, playlist.Value["Name"], playlist.Value["Description"]);
                            }
                            System.Threading.Thread.Sleep(15 * 1000);
                        }
                        catch (Exception e) { WriteLine("Error: " + e); accountToken = ""; }
                    }
                }
            

            Console.Read();
        }



        private static void GetPlaylists()
        {
            using (WebClient client = new System.Net.WebClient())
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                client.Headers[HttpRequestHeader.Accept] = "application/json";
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + accountToken;
                client.Headers.Add("Accept-Language", " en-US");
                client.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)");

                string result = client.DownloadString("https://api.spotify.com/v1/me/playlists");
                WriteLine("Pulling playlists...");
                if (result != "")
                {
                    string REGEX = "\"id\"[^\"]*:[^\"]*\"([^\"]+)\"[^\"]";
                    foreach (Match matches in Regex.Matches(result, REGEX, RegexOptions.IgnoreCase))
                    {
                        if (matches.Success)
                        {

                            string id = matches.Groups[1].Value.ToString();
                            if (id.Length < 20) continue;
                            if (!playlists.ContainsKey(id))
                            {
                                playlists[id] = new Dictionary<string, string>() { { "Name", "" }, { "Description", "" } };
                                WriteLine(" - New: " + id);
                            }

                        }
                    }
                }
            }
        }

        private static string GetPlaylist(string key, string expectedName)
        {
            using (WebClient client = new System.Net.WebClient())
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                client.Headers[HttpRequestHeader.Accept] = "application/json";
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + accountToken;
                client.Headers.Add("Accept-Language", " en-US");
                client.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)");

                string result = client.DownloadString("https://api.spotify.com/v1/playlists/" + key+"?fields=name,description,public");
                WriteLine("Checking " + (expectedName==""? key:expectedName)+": ");
                if (result != "")
                {
                    string REGEX = "\"description\"[^\"]*:[^\"]*\"([^\"]+)\"[^\"]*,[^\"]*\"name\"[^\"]*:[^\"]*\"([^\"]+)\"[^\"]*,[^\"]*\"public\"[^\"]*:[^\"]*true";
                    Match matches = Regex.Match(result, REGEX, RegexOptions.IgnoreCase);
                    if (matches.Success)
                    {
                        string name = matches.Groups[2].Value.ToString();
                        string description = matches.Groups[1].Value.ToString();
                        WriteLine(" - Name = " + name + " | Description = " + description);
                        if (name != "")
                        {
                            if (playlists[key]["Name"] != name || playlists[key]["Description"] != description)
                            {
                                playlists[key]["Name"] = name;
                                playlists[key]["Description"] = description;
                                WriteLine(" - Updated local script to new name/description.");
                            }
                            else WriteLine(" - No change.");
                            return name;
                        }
                        else WriteLine(" - Incorrect name: " + name);
                    }
                    else
                    {
                        WriteLine(" - Not public.");
                        return "Not public.";
                    }
                }
            }
            return "";
        }

        private static void UpdatePlaylist(string key, string name, string description)
        {
            using (WebClient client = new System.Net.WebClient())
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                client.Headers[HttpRequestHeader.Accept] = "application/json";
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + accountToken;
                client.Headers.Add("Accept-Language", " en-US");
                client.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)");
 
                string result = client.UploadString("https://api.spotify.com/v1/playlists/"+key, "PUT", "{\"name\":\""+name+"\",\"description\":\""+description+"\",\"public\":true}");
                WriteLine("Renamed " + name + ".");
            }
        }

        private static void WriteLine(string s)
        {
            Console.WriteLine(DateTime.Now + ": " + s);
            //File.AppendAllLines("SpotifyPlaylistAntiTakedown_log.txt", new string[] { DateTime.Now + ": " + s });
        }
    }
}
