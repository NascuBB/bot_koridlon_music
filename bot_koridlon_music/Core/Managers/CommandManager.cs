using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Discord.WebSocket;
using bot_koridlon_music.Core.Commands;

namespace bot_koridlon_music.Core.Managers
{
    public class CommandManager
    {
        private CommandService _commandService = ServiceManager.GetService<CommandService>();
        private static DiscordSocketClient _client = ServiceManager.GetService<DiscordSocketClient>();
        public async Task LoadCommandsAsync()
        {

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceManager.Provider);
            foreach (var command in _commandService.Commands)
            {
                Console.WriteLine($"[{DateTime.Now}]\t(Discord)\tCommand {command.Name} was loaded.");
            }
        }
        public static void Debug(object debugLog)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[{DateTime.Now}]\t(Debug)\t\t{debugLog}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void DebugError(object errorLog)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now}]\t(Error)\t\t{errorLog}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
