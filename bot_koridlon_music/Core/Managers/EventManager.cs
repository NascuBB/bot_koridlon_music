using System;
using System.Linq;
using Discord;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Victoria;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Discord.Net;
using Newtonsoft.Json;
using bot_koridlon_music.Core.Commands;
using System.Threading;
using ShablePrefics;

namespace bot_koridlon_music.Core.Managers
{
    public class EventManager
    {
        private static LavaNode _lavaNode = ServiceManager.Provider.GetRequiredService<LavaNode>();
        public static DiscordSocketClient _client = ServiceManager.GetService<DiscordSocketClient>();
        private static CommandService _commandService = ServiceManager.GetService<CommandService>();
        private static int skip = 0;
        private static string _prefix = ConfigManager.Config.Prefix;

        public static Task LoadCommands()
        {


            _client.Log += message =>
            {
                if (message.Source.Contains("Rest") || message.Source.Contains("Rest"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    Console.WriteLine($"[{DateTime.Now}]\t({message.Source})\t\t{message.Message}");
                    Console.ForegroundColor = ConsoleColor.White;
                }


                //if (message.Message == "Received InvalidSession" || message.Message == "Heartbeat Stopped")
                //{
                //    //if (skip != 0)
                //    //    (_client.GetChannel(897523148168245298) as IMessageChannel).SendMessageAsync($"[{DateTime.Now}]:no_entry: <@507619646325653505> Connection Error");
                //    Thread.Sleep(5);
                //    _ = ReconnectAsync();

                //}

                Console.WriteLine($"[{DateTime.Now}]\t({message.Source})\t{message.Message}");
                return Task.CompletedTask;
            };

            _commandService.Log += message =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"[{DateTime.Now}]\t({message.Source})\t{message.Message}");
                Console.ForegroundColor = ConsoleColor.White;
                return Task.CompletedTask;
            };


            _client.Ready += OnReady;
            _client.MessageReceived += OnMessageReceived;





            return Task.CompletedTask;
        }

        private static async Task OnMessageReceived(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if(message is null)
                return;
            var context = new SocketCommandContext(_client, message);

            if (context == null)
                return;

            if (message.Author.IsBot || message.Channel is IDMChannel)
            {
                return;
            }
            if (message.Content == "<@YOUR BOT ID>" || message.Content == "YOUR BOT ID" || message.Content == "<@!YOUR BOT ID>")
            {
                await message.Channel.SendMessageAsync($"Мой префикс на этом сервере: `{await Prefix.GetPrefix(context.Guild, _prefix)}`");
            }

            var argPos = 0;

            if (!(message.HasStringPrefix(await Prefix.GetPrefix(context.Guild, _prefix), ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;
            var result = await _commandService.ExecuteAsync(context, argPos, ServiceManager.Provider);
        }
        private static async Task OnReady()
        {
            if (skip == 0)
            {
                try
                {


                    _client.ButtonExecuted += InterationManager.MyButtonHandler;
                    //_lavaNode.OnPlayerUpdated += AudioManager.OnPlayerUpdated;
                    Music.TracksPrewarm();
                    await Prefix.StartShable();

                    //LAVALINK--------------------------------------------------------------------------------------------------------------------------------------------------------------LAVALINK
                    _lavaNode.OnLog += OnLavaDebug;
                    _lavaNode.OnTrackStarted += AudioManager.OnTrackStarted;
                    _lavaNode.OnWebSocketClosed += AudioManager.OnPlayerDestroyed;
                    _lavaNode.OnTrackEnded += AudioManager.OnTrackEnded;
                    _lavaNode.OnTrackException += AudioManager.OnTrackException;
                    _lavaNode.OnTrackStuck += AudioManager.OnTrackStuck;
                    await _lavaNode.ConnectAsync();
                    //SLASH--------------------------------------------------------------------------------------------------------------------------------------------------------------SLASH
                    _client.InteractionCreated += SlashCommands.SlashInteractionCreated;

                    //await SlashCommands.DeploySlashCommands();
                   
                    

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"[{DateTime.Now}]\t(Ready)\t\tReady event started, reconect stable");
                    Console.ForegroundColor = ConsoleColor.White;
                    skip++;
                    return;
                }
                catch (Exception ex)
                {
                    CommandManager.DebugError(ex.Message);
                }
                
            }
        }


        private static async Task OnLavaDebug(LogMessage arg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{DateTime.Now}]\t({arg.Source})\t|{arg.Severity}|\n{arg.Message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        //private static async Task ReconnectAsync()
        //{
        //    //await (_client.GetChannel(897523148168245298) as IMessageChannel).SendMessageAsync($"[{DateTime.Now}] :arrows_counterclockwise: Attempting to reconect automatically...");
        //    Console.ForegroundColor = ConsoleColor.Red;
        //    Console.WriteLine($"[{DateTime.Now}]\t(Gateway)\tReceived InvalidSession");
        //    Console.WriteLine($"[{DateTime.Now}]\t(Application)\tRequested restart\n------------------------------------------------------------------------------------------------------------------------");
        //    await _client.LogoutAsync();
        //    await _client.StopAsync();
        //    Console.ForegroundColor = ConsoleColor.Cyan;
        //    Console.WriteLine($"[{DateTime.Now}]\t(Application)\tRestarting");
        //    Thread.Sleep(1000);
        //    Console.WriteLine($"[{DateTime.Now}]\t(Application)\tStarting");
        //    Console.ForegroundColor = ConsoleColor.White;
        //    await _client.LoginAsync(TokenType.Bot, ConfigManager.Config.Token);
        //    await _client.StartAsync();
        //    //await (_client.GetChannel(897523148168245298) as IMessageChannel).SendMessageAsync($"[{DateTime.Now}] :white_check_mark: <@507619646325653505> Reconnect successful");

        //}
    }
}
