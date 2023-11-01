using System;
using System.IO;
using Newtonsoft.Json;

namespace bot_koridlon_music.Core
{
    public class Decoding
    {
        private static string ConfigFolder = "Resourses";
        private static string ConfigFile = "Music.json";
        private static string ConfigPath = ConfigFolder + "/" + ConfigFile;
        public static string[] Decode(Genre type)
        {
            string[] urls;
            var obj = JsonConvert.DeserializeObject<MusicDecode>(File.ReadAllText(ConfigPath));
            switch (type)
            {
                case Genre.gaming:
                    urls = obj.gamingUrl;
                    Debug("getting gaming music");
                    return urls;
                case Genre.poprock:
                    urls = obj.poprockUrl;
                    Debug("getting poprock music");
                    return urls;
                case Genre.relax:
                    urls = obj.relaxUrl;
                    Debug("getting study music");
                    return urls;
                default:
                    throw new Exception("Invalid type");
            }
        }
        private static void Debug(string log)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[{DateTime.Now}]\t(Music)\t{log}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
    public class MusicDecode
    {
        public string[] gamingUrl { get; set; }
        public string[] poprockUrl { get; set; }
        public string[] relaxUrl { get; set; }
    }
    public enum Genre
    {
        gaming,
        poprock,
        relax
    }
}