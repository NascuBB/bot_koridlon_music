using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bot_koridlon_music.Core;
using Newtonsoft.Json;


namespace bot_koridlon_music.Core
{
    public class Music : Decoding
    {
        public static string[] preGamingUrls;
        public static string[] prePoprockUrls;
        public static string[] preRelaxUrls;
        public static void TracksPrewarm()
        {
            try
            {
                preGamingUrls = Decode(Genre.gaming);
                prePoprockUrls = Decode(Genre.poprock);
                preRelaxUrls = Decode(Genre.relax);
            }
            catch (Exception ex)
            {
                var json = JsonConvert.SerializeObject(ex.Message, Formatting.Indented);

                Console.ForegroundColor = ConsoleColor.Red;
                // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                Console.WriteLine(json);
                Console.ForegroundColor = ConsoleColor.White;
            }

        }

        public class Spotify
        {
            public static List<string> gamingURLs = preGamingUrls.ToList();

            public static List<string> poprockURLs = prePoprockUrls.ToList();

            public static List<string> music_studyURLs = preRelaxUrls.ToList();
        }
    }
}