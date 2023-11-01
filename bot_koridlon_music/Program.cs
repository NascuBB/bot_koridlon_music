using bot_koridlon_music.Core;

namespace bot_koridlon_music
{
    class Program
    {
        static void Main(string[] args) => new Bot().MainAsync().GetAwaiter().GetResult();
    }
}
