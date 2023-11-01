using bot_koridlon_music.Core.Managers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_koridlon_music.Core.Commands
{
    public class SlashCommands
    {
        private static DiscordSocketClient _client = ServiceManager.GetService<DiscordSocketClient>();
        public static async Task DeploySlashCommands()
        {
            SlashCommandBuilder globalPing = new SlashCommandBuilder()
                .WithName("ping")
                .WithDescription("показывает задержку бота");
            SlashCommandBuilder globalPlay = new SlashCommandBuilder()
                .WithName("play")
                .WithDescription("начать возпроизведене")
                .AddOption(new SlashCommandOptionBuilder()
                .WithName("query")
                .WithType(ApplicationCommandOptionType.String)
                .WithDescription("запрос")
                .WithRequired(true));
            SlashCommandBuilder globalStop = new SlashCommandBuilder()
                .WithName("stop")
                .WithDescription("остановить возпроизведение и очистить очередь");
            SlashCommandBuilder globalJoin = new SlashCommandBuilder()
                .WithName("join")
                .WithDescription("присоеденится в голосовой канал");
            SlashCommandBuilder globalLeave = new SlashCommandBuilder()
                .WithName("leave")
                .WithDescription("покинуть голосовой канал");
            SlashCommandBuilder globalList = new SlashCommandBuilder()
                .WithName("queue")
                .WithDescription("показывает очередь треков");
            SlashCommandBuilder globalSeek = new SlashCommandBuilder()
                .WithName("seek")
                .WithDescription("перемотать трек")
                .AddOption("minutes",ApplicationCommandOptionType.Integer,"на какую минуту перемотать",true)
                .AddOption("seconds", ApplicationCommandOptionType.Integer, "на какую секунду перемотать",false);
            SlashCommandBuilder globalLoop = new SlashCommandBuilder()
                .WithName("loop")
                .WithDescription("включает/выключает повторное возпроизведение трека");
            SlashCommandBuilder globalQLoop = new SlashCommandBuilder()
                .WithName("loop-queue")
                .WithDescription("включает/выключает повторное возпроизведение очереди");
            SlashCommandBuilder globalSkip = new SlashCommandBuilder()
                .WithName("skip")
                .WithDescription("пропустить трек");
            SlashCommandBuilder globalShuffle = new SlashCommandBuilder()
                .WithName("shuffle")
                .WithDescription("перемешать очередь");
            SlashCommandBuilder globalRemoveTrack = new SlashCommandBuilder()
                .WithName("remove-track")
                .WithDescription("убрать определённый трек из очереди")
                .AddOption("index", ApplicationCommandOptionType.Integer, "номер трека из очереди для удаления", true);
            SlashCommandBuilder globalRemoveTracks = new SlashCommandBuilder()
                .WithName("remove-tracks")
                .WithDescription("убрать несколько треков из очереди")
                .AddOption("index", ApplicationCommandOptionType.Integer, "номер от какого трека удалять треки",true)
                .AddOption("amount", ApplicationCommandOptionType.Integer, "количество треков для удаления из очереди (0 чтобы очистить до самого конца очереди)",false);
            SlashCommandBuilder globalSkipTo = new SlashCommandBuilder()
                .WithName("skip-to")
                .WithDescription("пропустить несколько треков")
                .AddOption("amount", ApplicationCommandOptionType.Integer, "количество треков для пропуска",true);
            SlashCommandBuilder globalPause = new SlashCommandBuilder()
                .WithName("pause")
                .WithDescription("приостонавливает/продолжает возпроизведение трека");
            SlashCommandBuilder globalVolume = new SlashCommandBuilder()
                .WithName("volume")
                .WithDescription("установить громкость бота")
                .AddOption(new SlashCommandOptionBuilder()
                .WithDescription("громкость от 1 до 150 (по умолчанию 100)")
                .WithName("volume")
                .WithRequired(true)
                .WithMinValue(1)
                .WithMaxValue(150)
                .WithType(ApplicationCommandOptionType.Integer));
            SlashCommandBuilder globalHelp = new SlashCommandBuilder()
                .WithName("help")
                .WithDescription("сравка");
            CommandManager.Debug("Slash Commands Deployed");

            try
            {
                    await _client.Rest.DeleteAllGlobalCommandsAsync();
                    await _client.CreateGlobalApplicationCommandAsync(globalPlay.Build());
                    await _client.CreateGlobalApplicationCommandAsync(globalVolume.Build());
                    await _client.CreateGlobalApplicationCommandAsync(globalHelp.Build());
                    await _client.CreateGlobalApplicationCommandAsync(globalSeek.Build());
                    await _client.CreateGlobalApplicationCommandAsync(globalPing.Build());
                    await _client.CreateGlobalApplicationCommandAsync(globalStop.Build());
                    await _client.CreateGlobalApplicationCommandAsync(globalJoin.Build());
                    await _client.CreateGlobalApplicationCommandAsync(globalLeave.Build());
                    await _client.CreateGlobalApplicationCommandAsync(globalList.Build());
                    await _client.CreateGlobalApplicationCommandAsync(globalLoop.Build());
                    await _client.CreateGlobalApplicationCommandAsync(globalQLoop.Build());
                    await _client.CreateGlobalApplicationCommandAsync(globalSkip.Build());
                    await _client.CreateGlobalApplicationCommandAsync(globalShuffle.Build());
                    await _client.CreateGlobalApplicationCommandAsync(globalRemoveTrack.Build());
                    await _client.CreateGlobalApplicationCommandAsync(globalRemoveTracks.Build());
                    await _client.CreateGlobalApplicationCommandAsync(globalSkipTo.Build());
                    await _client.CreateGlobalApplicationCommandAsync(globalPause.Build());
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now}]\t(Error)\t\t{ex.Message}\n{ex}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static async Task SlashInteractionCreated(SocketInteraction interaction)
        {
            try
            {
                
                SocketGuildUser user = interaction.User as SocketGuildUser;
                IGuild guild = user.Guild;
                if(interaction is SocketSlashCommand command)
                {
                    Console.WriteLine($"[{DateTime.Now}]\t(SlashCommand)\tGot {command.Data.Name} slashCommand from {command.Channel}");
                    switch (command.Data.Name)
                    {
                        case "ping":
                            await command.RespondAsync("",embed: new EmbedBuilder().WithColor(Color.Green).WithTitle(":ping_pong:Pong!").WithDescription($":clock10: **{EventManager._client.Latency}ms**").Build());
                            break;
                        case "play":
                            string search = command.Data.Options.First().Value as string;
                            await command.RespondAsync(await AudioManager.PlayAsync(user, guild, search, user, command.Channel as ITextChannel));
                            break;
                        case "stop":
                            await command.RespondAsync(await AudioManager.StopAsync(guild,user));
                            break;
                        case "join":
                            await command.RespondAsync(await AudioManager.JoinAsync(guild, user,command.Channel as ITextChannel));
                            break;
                        case "leave":
                            await command.RespondAsync(await AudioManager.LeaveAsync(guild, user));
                            break;
                        case "list":
                            await command.RespondAsync(await AudioManager.ListAsync(guild, user));
                            break;
                        case "loop":
                            await command.RespondAsync(await AudioManager.LoopTrackAsync(guild, user));
                            break;
                        case "loop-queue":
                            await command.RespondAsync(await AudioManager.LoopQueueAsync(guild, user));
                            break;
                        case "skip":
                            await command.RespondAsync(await AudioManager.SkipTrackAsync(guild, 0, user));
                            break;
                        case "shuffle":
                            await command.RespondAsync(await AudioManager.ShuffleQueueAsync(guild, user));
                            break;
                        case "remove-track":
                            int indexT = (int)(Int64)command.Data.Options.First().Value;
                            await command.RespondAsync(await AudioManager.RemoveTrackQueueAsync(guild, indexT,user));
                            break;
                        case "remove-tracks":
                            int indexTs = (int)(Int64)command.Data.Options.First(x => x.Name == "index").Value;
                            int amountTs = 0;
                            try { amountTs = (int)(Int64)command.Data.Options.First(x => x.Name == "amount").Value; }
                            catch { }
                            await command.RespondAsync(await AudioManager.RemoveTracksAsync(guild, indexTs,user, amountTs));
                            break;
                        case "seek":
                            int minutes = (int)(Int64)command.Data.Options.First(x => x.Name == "minutes").Value;
                            int seconds = 0;
                            try { seconds = (int)(Int64)command.Data.Options.First(x => x.Name == "seconds").Value; }
                            catch{ }
                            await command.RespondAsync(await AudioManager.SeekAsync(minutes, seconds, guild, user));
                            break;
                        case "help":
                            await command.RespondAsync(embed: await HelpVariations.helpsAsync(1,guild),components: new ComponentBuilder().WithButton("←", "disabled", disabled: true) .WithButton("→", "nextHelp").WithButton("🗑", "delete", ButtonStyle.Danger).Build());
                            break;
                        case "skip-to":
                            int amountS = (int)(Int64)command.Data.Options.First().Value;
                            await command.RespondAsync(await AudioManager.SkipTrackAsync(guild, amountS, user));
                            break;
                        case "pause":
                            await command.RespondAsync(await AudioManager.PauseAsync(guild, user));
                            break;
                        case "queue":
                            await command.RespondAsync(await AudioManager.ListAsync(guild, user));
                            break;
                        case "volume":
                            int volume = (int)(Int64)command.Data.Options.First().Value;
                            await command.RespondAsync(await AudioManager.SetVolumeAsync(guild, volume, user));
                            break;
                    }
                }
                
            }
            catch (Exception ex)
            {
                CommandManager.DebugError("Slash error\t" + ex.Message);
            }
        }
    }
}
