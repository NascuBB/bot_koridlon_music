using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using bot_koridlon_music.Core.Managers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Victoria;


namespace bot_koridlon_music.Core
{
    public class Bot
    {
        internal static bool DebugMode = false;
        public static DiscordSocketClient _client;
        private CommandService _commandService;

        public static class Time
        {
            public static DateTime time;
        }
        public static class Count
        {
            public static int totalInvoke;
            public static int totalErrorsDuringWorktime;
        }

        public Bot()
        {

            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Debug,
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.All,
                UseInteractionSnowflakeDate = false
            });
            _commandService = new CommandService(new CommandServiceConfig()
            {
                LogLevel = LogSeverity.Info,
                CaseSensitiveCommands = true,
                DefaultRunMode = RunMode.Async,
                IgnoreExtraArgs = true
            });

            var collection = new ServiceCollection();

            collection.AddSingleton(_client)
                .AddSingleton(_commandService)
                .AddLavaNode(x =>
                {
                    x.SelfDeaf = true;
                });

            ServiceManager.SetProvider(collection);
        }

        public async Task MainAsync()
        {
            Console.ForegroundColor = ConsoleColor.White;
            var cmanager = new CommandManager();
            Time.time = DateTime.UtcNow;

            await cmanager.LoadCommandsAsync();
            await EventManager.LoadCommands();
            await _client.LoginAsync(TokenType.Bot, ConfigManager.Config.Token);
            await _client.StartAsync();
            //await AudioManager.StartLava();
            //await EventManager.Timr();

            await Task.Delay(-1);
        }
    }
}
