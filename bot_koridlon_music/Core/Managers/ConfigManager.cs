using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace bot_koridlon_music.Core.Managers
{
    public static class ConfigManager
    {
        private static string ConfigFolder = "Resourses";
        private static string ConfigFile = "Config.json";
        private static string ConfigPath = ConfigFolder + "/" + ConfigFile;
        public static BotConfig Config { get; private set; }

        static ConfigManager()
        {
            if (!Directory.Exists(ConfigFolder))
            {
                Directory.CreateDirectory(ConfigFolder);
            }
            if (!File.Exists(ConfigPath))
            {
                Config = new BotConfig();
                var json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                File.WriteAllText(ConfigPath, contents: json);
            }
            else
            {
                var json = File.ReadAllText(ConfigPath);
                Config = JsonConvert.DeserializeObject<BotConfig>(json);

            }
        }
    }

    public struct BotConfig
    {
        [JsonProperty("tocken")]
        public string Token { get; private set; }
        [JsonProperty("prefix")]
        public string Prefix { get; set; }
    }
}

