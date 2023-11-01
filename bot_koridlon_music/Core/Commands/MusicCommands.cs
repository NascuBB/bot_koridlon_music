using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using bot_koridlon_music.Core.Managers;
using bot_koridlon_music.Core;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;

namespace bot_koridlon_music.Core.Commands
{
    public class MusicCommands : ModuleBase<SocketCommandContext>
    {
        [Command("duplicate")]
        [Alias("dup", "d")]
        public async Task DupCommand() => await Context.Message.ReplyAsync(await AudioManager.DuplicateAsync(Context.Guild, Context.User as SocketGuildUser, Context.Channel as ITextChannel), false, null, AllowedMentions.None);
        [Command("join")]
        [Summary("Makes bot join to vc that you are in")]
        public async Task JoinCommand() => await Context.Message.ReplyAsync(await AudioManager.JoinAsync(Context.Guild, Context.User as IVoiceState, Context.Channel as ITextChannel), false, null, AllowedMentions.None);

        [Command("play")]
        [Alias("p")]
        [Summary("Plays video from Youtube")]
        public async Task PlayCommand([Remainder] string search) => await Context.Message.ReplyAsync(await AudioManager.PlayAsync(Context.User as SocketGuildUser, Context.Guild, search, Context.User as IVoiceState, Context.Channel as ITextChannel), false, null, AllowedMentions.None);

        [Command("leave")]
        [Summary("leaves the vc")]
        public async Task LeaveCommand() => await Context.Message.ReplyAsync(await AudioManager.LeaveAsync(Context.Guild, Context.User as SocketGuildUser), false, null, AllowedMentions.None);

        [Command("list")]
        [Alias("queue")]
        [Summary("gives a list of queued songs")]
        public async Task ListCommand() => await Context.Message.ReplyAsync(await AudioManager.ListAsync(Context.Guild, Context.User as SocketGuildUser), false, null, AllowedMentions.None);

        [Command("loop")]
        [Summary("loops a track")]
        public async Task LoopCommand() => await Context.Message.ReplyAsync(await AudioManager.LoopTrackAsync(Context.Guild, Context.User as SocketGuildUser), false, null, AllowedMentions.None);
        [Command("queueloop")]
        [Alias("qloop")]
        [Summary("loops a queue")]
        public async Task LoopQueueCommand() => await Context.Message.ReplyAsync(await AudioManager.LoopQueueAsync(Context.Guild, Context.User as SocketGuildUser), false, null, AllowedMentions.None);


        [Command("skip")]
        [Summary("skips current track")]
        public async Task SkipAsync() => await Context.Message.ReplyAsync(await AudioManager.SkipTrackAsync(Context.Guild, 0, Context.User as SocketGuildUser), false, null, AllowedMentions.None);

        [Command("shuffle")]
        [Summary("shuffles queue")]
        public async Task ShuffleAsync() => await Context.Message.ReplyAsync(await AudioManager.ShuffleQueueAsync(Context.Guild, Context.User as SocketGuildUser), false, null, AllowedMentions.None);

        [Command("removetrack")]
        [Alias("deltr", "remtr")]
        [Summary("Removes track by its index")]
        public async Task RemoveTrackAsync(int index) => await Context.Message.ReplyAsync(await AudioManager.RemoveTrackQueueAsync(Context.Guild, index, Context.User as SocketGuildUser), false, null, AllowedMentions.None);

        [Command("removefew")]
        [Alias("rmf", "remfew")]
        [Summary("removes some trakcs from queue")]
        public async Task RemoveTracksAsync(int index, int amount = 0) => await Context.Message.ReplyAsync(await AudioManager.RemoveTracksAsync(Context.Guild, index, Context.User as SocketGuildUser, amount), false, null, AllowedMentions.None);

        [Command("skipto")]
        [Summary("skips some amount of tracks")]
        public async Task SkipToAsync(int amount) => await Context.Message.ReplyAsync(await AudioManager.SkipTrackAsync(Context.Guild, amount, Context.User as SocketGuildUser), false, null, AllowedMentions.None);

        [Command("stop")]
        [Summary("stops playback")]
        public async Task StopAsync() => await Context.Message.ReplyAsync(await AudioManager.StopAsync(Context.Guild, Context.User as SocketGuildUser), false, null, AllowedMentions.None);

        [Command("pause")]
        [Alias("resume")]
        [Summary("pauses/resumes track")]
        public async Task PauseAsync() => await Context.Message.ReplyAsync(await AudioManager.PauseAsync(Context.Guild, Context.User as SocketGuildUser), false, null, AllowedMentions.None);

        [Command("volume")]
        [Summary("changes volume to a player")]
        public async Task VolumeAsync(int volume) => await Context.Message.ReplyAsync(await AudioManager.SetVolumeAsync(Context.Guild, volume, Context.User as SocketGuildUser), false, null, AllowedMentions.None);

        [Command("seek")]
        [Summary("seeks to a time")]
        public async Task SeekAsync(int timeSpanM, int seconds = 0) => await Context.Message.ReplyAsync(await AudioManager.SeekAsync(timeSpanM, seconds, Context.Guild, Context.User as SocketGuildUser), false, null, AllowedMentions.None);
        [Command("lyrics")]
        [Summary("shows lyrics to a song")]
        public async Task LyricsAsync() => await Context.Message.ReplyAsync(await AudioManager.GetLyricsAsync(Context.Guild, Context.User as SocketGuildUser), false, null, AllowedMentions.None);
        [Command("ping")]
        [Summary("shows bot Latency")]
        public async Task PingAsync() => await Context.Message.ReplyAsync(embed: new EmbedBuilder().WithColor(Color.Green).WithTitle(":ping_pong:Pong!").WithDescription($":clock10: **{EventManager._client.Latency}ms**").Build(), allowedMentions: AllowedMentions.None);
        [Command("reconect")]
        public async Task ReconectAsync() => await Context.Message.ReplyAsync(await AudioManager.ReconectPlayerAsync(Context.Guild, Context.User as SocketGuildUser), false, null, AllowedMentions.None);
        [Command("help")]
        public async Task Help()
        {
            var message = Context.Message;
            try
            {
                var builder = new ComponentBuilder()
                .WithButton("←", "disabled", disabled: true)
                .WithButton("→", "nextHelp")
                .WithButton("🗑", "delete", ButtonStyle.Danger);
                await message.ReplyAsync("", false, await HelpVariations.helpsAsync(1, Context.Guild), AllowedMentions.None, components: builder.Build());
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now}]\t(Error)\t\t{ex.Message}");
                Console.ForegroundColor = ConsoleColor.White;
                var errorKickEmbedBuilder = new EmbedBuilder()
            .WithColor(new Color(255, 0, 0))
            .WithTitle(":no_entry: Error!")
            .WithDescription($"**{ex.Message}**")
            .WithCurrentTimestamp();
            }
        }
        [Command("setprefix")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task Setprefix(string prefix)
        {
            try
            {
                await ShablePrefics.Prefix.SetPrefix(Context.Guild, prefix);
                await ReplyAsync(":white_check_mark: Готово!");
            }
            catch (Exception ex)
            {
                await ReplyAsync($":no_entry:Error\t{ex.Message}");
            }
        }
        [Command("defaultprefix")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task Setdefaultprefix()
        {
            try
            {
                await ShablePrefics.Prefix.RemovePrefix(Context.Guild);
                await ReplyAsync(":white_check_mark: Готово!");
            }
            catch (Exception ex)
            {
                await ReplyAsync($":no_entry:Error\t{ex.Message}");
            }
        }
        [Command("reloadSlash")]
        public async Task ReloadSlash()
        {
            if (Context.User.Id != 734449219892412476)
                return;
            try
            {
                Context.Channel.SendMessageAsync(":arrows_counterclockwise: перегружаю");
                await SlashCommands.DeploySlashCommands();
                Context.Channel.SendMessageAsync(":white_check_mark: Готово, перезапускаюсь");
                string file = Assembly.GetExecutingAssembly().Location;
                System.Diagnostics.Process.Start(file);
                Environment.Exit(0);
            }
            catch(Exception e)
            {
                Context.Channel.SendMessageAsync(e.Message);
            }
            
        }
    }
}
