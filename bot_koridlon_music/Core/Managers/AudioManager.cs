using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using SpotifyAPI.Web;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using static bot_koridlon_music.Core.Bot;
using System.Security.Cryptography;
using System.Security.AccessControl;
using System.Timers;

namespace bot_koridlon_music.Core.Managers
{
    public class AudioManager
    {
        public static SpotifyClientConfig config = SpotifyClientConfig
     .CreateDefault()
     .WithAuthenticator(new ClientCredentialsAuthenticator("YOUR API KEY", "YOUR API KEY"));
        public static SpotifyClient spotify = new SpotifyClient(config);
        private static readonly LavaNode _lavaNode = ServiceManager.Provider.GetRequiredService<LavaNode>();
        private readonly ILogger _logger;
        public readonly HashSet<ulong> VoteQueue;
        public static readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;
        private static Dictionary<ulong, bool> isLooped  = new Dictionary<ulong, bool>();
        private static Dictionary<ulong, bool> isLoopedQueue = new Dictionary<ulong, bool>();
        public AudioManager(LavaNode lavaNode, ILoggerFactory loggerFactory)
        {

            _logger = loggerFactory.CreateLogger<LavaNode>();



            VoteQueue = new HashSet<ulong>();
        }
        static AudioManager()
        {
            _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
        }

        public static async Task<string> JoinAsync(IGuild guild, IVoiceState voiceState, ITextChannel channel)
        {
            if (_lavaNode.HasPlayer(guild)) return ":information_source:Я уже в голосовом канале";
            if (voiceState.VoiceChannel == null) return ":information_source:Ты должен быть в голосовом канале";

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, channel);
                return $":white_check_mark:Подключился в {voiceState.VoiceChannel.Name}";
            }
            catch (Exception ex)
            {
                return $":no_entry:Ошибка!\t{ex.Message}";
            }
        }

        //public static async Task<string> ReconectAsync(IGuild guild)
        //{
        //    try
        //    {
        //        var player = _lavaNode.GetPlayer(guild);
                
        //        return "";
        //    }
        //    catch (Exception ex)
        //    {
        //        return $":no_entry:Error!\t{ex.Message}";
        //    }
        //}
        public static async Task<string> SetVolumeAsync(IGuild guild, int volume, SocketGuildUser user)
        {
            if (user.VoiceChannel == null) return ":information_source: Ты должен быть в голосовом канале";
            if (volume > 150 || volume <= 0)
            {
                return $":information_source: Значение звука должно быть от 1 до 150.";
            }
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                await player.UpdateVolumeAsync((ushort)volume);
                Console.WriteLine($"[{DateTime.Now}]\t(Audio)\t\tBot Volume in {guild.Name} set to: {volume}");
                return $":white_check_mark: Я изменил громкость на {volume}";
            }
            catch (InvalidOperationException ex)
            {
                return $":no_entry:Ошибка! {ex.Message}";
            }
        }
        
        public static async Task<string> SeekAsync(int m,int s, IGuild guild, SocketGuildUser user)
        {
            if (user.VoiceChannel == null) return ":information_source: Ты должен быть в голосовом канале";
            if (!_lavaNode.TryGetPlayer(guild, out var player))
            {
                return ":information_source: Я не подключён в голосой канал";
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                return ":information_source: Я не могу перематывать воздух)";
            }

            try
            {
                TimeSpan timeSpan = new TimeSpan(0, m,s);
                await player.SeekAsync(timeSpan);
                return $":white_check_mark:Готово! Я перемотал трек `{player.Track.Title}` на [{timeSpan}].";
            }
            catch (Exception exception)
            {
                return $":no_entry:Ошибка! {exception.Message}";
            }
        }



        //public static async Task<string> BassAsync(IGuild guild)
        //{
        //    try
        //    {
        //        var player = _lavaNode.GetPlayer(guild);


        //        var playerEq = player.Equalizer;


        //        await player.EqualizerAsync();
        //        return "";

        //    }
        //    catch (Exception ex)
        //    {
        //        return $":no_entry:Я лохонулся!\t{ex.Message}";
        //    }
        //}
        public static async Task<string> GetLyricsAsync(IGuild guild, SocketGuildUser user)
        {
            try
            {
                if (user.VoiceChannel == null) return ":information_source: Ты должен быть в голосовом канале";
                string lyrics = null;
                var player = _lavaNode.GetPlayer(guild);

                if (player is null)
                    return $":no_entry:Вы в голосовом чате без меня(";

                if (player.PlayerState != PlayerState.Playing)
                    return $":information_source:Пока что я ничего не играю";

                try
                {
                    lyrics = await player.Track.FetchLyricsFromGeniusAsync();
                    if (string.IsNullOrWhiteSpace(lyrics))
                    {
                        try
                        {
                            lyrics = await player.Track.FetchLyricsFromOvhAsync();
                        }
                        catch (Exception ex)
                        {
                            return $":no_entry:Произошёл прикол\t{ex.Message}";
                        }


                    }
                    if (string.IsNullOrWhiteSpace(lyrics))
                        return $":no_entry:Я не нашел текст для {player.Track.Title}";

                    return $"Текст для {player.Track.Title}\n\n{lyrics}";
                }
                catch (Exception ex)
                {
                    return $":no_entry:Ошибка!\t{ex.Message}";
                }
            }
            catch (Exception ex)
            {
                return $":no_entry:Ой!\t {ex.Message}";
            }
        }
        public static async Task<string> DuplicateAsync(IGuild guild, SocketGuildUser user, ITextChannel channel)
        {
            var activities = user.Activities;
            List<IActivity> activity = activities.ToList();
            IActivity listerning = null;
            try
            {
                foreach(var act in activity)
                {
                    if(act.Type == ActivityType.Listening)
                    {
                        listerning = act;
                    }
                }
                if (listerning == null) return "you don't listerning anything rn";
                SpotifyGame track = listerning as SpotifyGame;
                string search = $"{track.Artists.First()} - {track.TrackTitle}";
                return await PlayAsync(user as SocketGuildUser, guild, search, user as IVoiceState, channel);
            }
            catch(Exception e)
            {
                return e.Message;
            }
        }

        public static async Task<string> PlayAsync(SocketGuildUser user, IGuild guild, string query, IVoiceState voiceState, ITextChannel channel)
        {
            bool joined = false;

            if (user.VoiceChannel == null) return ":information_source: Ты как минимум должен быть в голосовом канале";
            if (!_lavaNode.HasPlayer(guild))
            {
                try
                {
                    await _lavaNode.JoinAsync(voiceState.VoiceChannel, channel);
                    joined = true;

                }
                catch (Exception ex)
                {
                    return $":no_entry:Виноват!\t{ex.Message}";
                }
            }
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (query.Contains("https://open.spotify.com/playlist/"))
                {
                    try
                    {
                        int i = 0;
                        string url = query.Substring(34, 22);
                        var playlist = await spotify.Playlists.Get(url);
                        if (playlist.Tracks.Items.Count <= 0)
                            return ":information_source: Твой плейлист пуст";
                        List<PlaylistTrack<IPlayableItem>> dequeuePlaylist = playlist.Tracks.Items;
                        bool isPlaying = false;
                        LavaTrack trackFq = null;
                        if (!(player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused))
                        {
                            isPlaying = true;
                            FullTrack ftrack = playlist.Tracks.Items.First().Track as FullTrack;
                            string trackToPlayRN = ftrack.Artists.First().Name + " " + "-" + " " + ftrack.Name;
                            CommandManager.Debug(trackToPlayRN);

                            var searchFQ = Uri.IsWellFormedUriString(trackToPlayRN, UriKind.Absolute) ?
                                await _lavaNode.SearchAsync(Victoria.Responses.Search.SearchType.Direct, trackToPlayRN)
                                : await _lavaNode.SearchYouTubeAsync(trackToPlayRN);

                            trackFq = searchFQ.Tracks.FirstOrDefault();

                            await player.PlayAsync(trackFq);
                            TrackStartEventArgs args = new()
                            {
                                Player = player,
                                Track = trackFq
                            };

                            await OnTrackStarted(args);
                        }
                        if (isPlaying)
                        {
                            dequeuePlaylist.RemoveAt(0);
                        }
                        if (playlist.Tracks.Items.Count <= 0)
                            return ":information_source: Похоже в твоём плейлисте был всего 1 трек";

                        foreach (PlaylistTrack<IPlayableItem> item in dequeuePlaylist)
                        {
                            if (item.Track is FullTrack plTrack)
                            {
                                i++;
                                //FullTrack vftrack = playlist.Tracks.Items.First().Track as FullTrack;

                                string trackToPlay = plTrack.Artists.First().Name + " " + "-" + " " + plTrack.Name;

                                var searchQ = Uri.IsWellFormedUriString(trackToPlay, UriKind.Absolute) ?
                           await _lavaNode.SearchAsync(Victoria.Responses.Search.SearchType.Direct, trackToPlay)
                           : await _lavaNode.SearchYouTubeAsync(trackToPlay);

                                var trackQ = searchQ.Tracks.FirstOrDefault();
                                player.Queue.Enqueue(trackQ);
                                Thread.Sleep(25);
                            }
                        }
                        return $":white_check_mark: Я успешно добавил {i} треков с твоего плейлиста в очередь!{((isPlaying) ? $"\n:notes: Сейчас играет: `{trackFq.Title}` [{trackFq.Duration}]" : "")}";
                    }
                    catch (Exception ex)
                    {
                        return $":no_entry: Упс... \t{ex.Message}";
                    }
                }

                LavaTrack track;
                if (query.Contains("https://open.spotify.com/track/"))
                {
                    query = query.Substring(31, 22);
                    query = spotify.Tracks.Get(query).Result.Artists.First().Name + " " + "-" + " " + spotify.Tracks.Get(query).Result.Name;
                    Console.WriteLine($"[{DateTime.Now}]\t(Debug)\t\t{query}");
                }
                if (query == "rm" || query == "randmusic")
                {
                    Random rnd = new Random();
                    int randswitch = rnd.Next(1, 5);
                    int rndbot_koridlon_music = 0;
                    switch (randswitch)
                    {
                        case 1:
                            rndbot_koridlon_music = rnd.Next(0, Music.Spotify.gamingURLs.Count - 1);
                            query = Music.Spotify.gamingURLs[rndbot_koridlon_music];
                            break;
                        case 2:
                            rndbot_koridlon_music = rnd.Next(0, Music.Spotify.gamingURLs.Count - 1);
                            query = Music.Spotify.gamingURLs[rndbot_koridlon_music];
                            break;
                        case 3:
                            rndbot_koridlon_music = rnd.Next(0, Music.Spotify.poprockURLs.Count - 1);
                            query = Music.Spotify.poprockURLs[rndbot_koridlon_music];
                            break;
                        case 4:
                            rndbot_koridlon_music = rnd.Next(0, Music.Spotify.music_studyURLs.Count - 1);
                            query = Music.Spotify.music_studyURLs[rndbot_koridlon_music];
                            break;
                    }
                    query = query.Substring(31, 22);
                    query = spotify.Tracks.Get(query).Result.Artists.First().Name + " " + "-" + " " + spotify.Tracks.Get(query).Result.Name;
                    Console.WriteLine($"[{DateTime.Now}]\t(Debug) \t{query}");
                }

                var search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ?
                   await _lavaNode.SearchAsync(Victoria.Responses.Search.SearchType.Direct, query)
                   : await _lavaNode.SearchYouTubeAsync(query);

                if (search.Status == Victoria.Responses.Search.SearchStatus.NoMatches) return $":no_entry:По запросу `{query}` я ничего не накопал(";

                track = search.Tracks.FirstOrDefault();

                if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    player.Queue.Enqueue(track);
                    Console.WriteLine($"[{DateTime.Now}]\t(AUDIO)\t\tTrack was added to queue");
                    if (!track.IsStream)
                    {
                        return $":notepad_spiral:`{track.Title}` был добавлен в очередь [{track.Duration}]";
                    }
                    else
                    {
                        return $":notepad_spiral:`{track.Title}` был добавлен в очередь [*бесконечный*]";
                    }
                }
                if (track is null)
                    return $":no_entry: Хм, по твоему запросу я ничего не нашел. Возможно это даже не трек";
                if (!joined)
                {
                    await player.PlayAsync(track);
                    Console.WriteLine($"[{DateTime.Now}]\t(AUDIO)\t\tNow playing {track.Title} in {guild}");
                    TrackStartEventArgs args = new()
                    {
                        Player = player,
                        Track = track
                    };

                    await OnTrackStarted(args);
                    if (!track.IsStream)
                    {
                        return $":notes:Сейчас играет: `{track.Title}` [{track.Duration}]";
                    }
                    else
                    {

                        return $":notes:Сейчас играет: `{track.Title}` [*бесконечный*]";
                    }
                }
                else
                {
                    await player.PlayAsync(track);
                    Console.WriteLine($"[{DateTime.Now}]\t(AUDIO)\t\tNow playing {track.Title} in {guild}");
                    TrackStartEventArgs args = new()
                    {
                        Player = player,
                        Track = track
                    };

                    await OnTrackStarted(args);
                    if (!track.IsStream)
                    {
                        return $":white_check_mark: Подключён в {voiceState.VoiceChannel.Name}\n:notes:Сейчас играет: `{track.Title}` [{track.Duration}]";
                    }
                    else
                    {
                        return $":white_check_mark: Подключён в {voiceState.VoiceChannel.Name}\n:notes:Сейчас играет: `{track.Title}` [*бесконечный*]";
                    }
                }
            }
            catch (Exception ex)
            {
                return $":no_entry:Ошибка!\t{ex.Message}";

            }

        }
        public static async Task<string> StopAsync(IGuild guild, SocketGuildUser user)
        {
            try
            {
                if (user.VoiceChannel == null) return ":information_source: Ты должен быть в голосовом канале";
                var player = _lavaNode.GetPlayer(guild);

                if (player == null)
                    return ":information_source: Нечего останавливать, я ж ничего не ираю";

                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.StopAsync();
                    Console.WriteLine($"[{DateTime.Now}]\t(Audio)\tPlayback was ended in {guild.Name}");
                    return ":white_check_mark:Остановлено!";
                }
                else
                {
                    return ":information_source: Уже остановлено!";
                }


            }
            catch (Exception ex)
            {
                return $":no_entry:Ошибка!\t{ex.Message}";
            }
        }
        public static async Task<string> PauseAsync(IGuild guild, SocketGuildUser user)
        {
            try
            {
                if (user.VoiceChannel == null) return ":information_source: Ты должен быть в голосовом канале";
                var player = _lavaNode.GetPlayer(guild);
                if (!(player.PlayerState is PlayerState.Playing))
                {
                    await player.ResumeAsync();
                    return $":white_check_mark:Возобновлено!";
                }
                else
                {
                    await player.PauseAsync();
                    return $":white_check_mark:Приостановлено!";
                }
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
        public static async Task<string> ListAsync(IGuild guild, SocketGuildUser user)
        {
            try
            {
                if (user.VoiceChannel == null) return ":information_source: Ты должен быть в голосовом канале";
                var totalDuration = TimeSpan.Zero;
                var bot_koridlon_musicList = "";
                var returnText = "";
                if (!_lavaNode.HasPlayer(guild)) return ":information_source: Похоже я ничего не играю";
                LavaPlayer player = _lavaNode.GetPlayer(guild);
               string localHour, localMinute, localSeconds;

                if (player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    if (player.Queue.Count < 1 && player.Track != null)
                    {
                        return $":notes:Сейчас играет: `{player.Track.Title}` [{player.Track.Duration}]\n:information_source: Больше в очереди ничего нет.";
                    }
                    else
                    {
                        var trackNum = 2;
                        foreach (LavaTrack track in player.Queue)
                        {
                            if (!track.IsStream)
                            {
                                bot_koridlon_musicList += $"• `{track.Author} - {track.Title}` [{track.Duration}]\n\n";
                                totalDuration += track.Duration;
                            }
                            else
                            {
                                bot_koridlon_musicList += $"• `{track.Author} - {track.Title}` [*Бесконечный*]\n\n";
                            }
                            trackNum++;


                        }
                        if (isLoopedQueue.ContainsKey(guild.Id))
                        {
                            if (isLoopedQueue[guild.Id])
                                returnText += ":repeat: **Очередь зацыклена**\n\n";
                        }

                        if (player.Queue.Any(x => x.IsStream == false))
                        {
                            TimeSpan totalCusDuration = totalDuration + (player.Track.Duration - player.Track.Position);
                            if (totalCusDuration.Minutes < 10)
                            {
                                localMinute = "0" + $"{totalCusDuration.Minutes}";
                            }
                            else
                            {
                                localMinute = $"{totalCusDuration.Minutes}";
                            }
                            if (totalCusDuration.Hours < 10 || totalCusDuration.Hours == 0)
                            {
                                localHour = $"0{totalCusDuration.Hours}";
                            }
                            else
                            {
                                localHour = $"{totalCusDuration.Hours}";
                            }
                            if (totalCusDuration.Seconds < 10 || totalCusDuration.Seconds == 0)
                            {
                                localSeconds = $"0{totalCusDuration.Seconds}";
                            }
                            else
                            {
                                localSeconds = $"{totalCusDuration.Seconds}";
                            }
                            returnText += $":alarm_clock:**Общая продолжительность песен:** {localHour}:{localMinute}:{localSeconds}\n\n:notes:Сейчас играет `{player.Track.Title}` [{player.Track.Duration}]\n\n{bot_koridlon_musicList}";
                        }
                        else
                        {
                            returnText += $":alarm_clock:**Общая продолжительность песен:** *бесконечность*\n\n:notes:Сейчас играет `{player.Track.Title}` [{player.Track.Duration}]\n\n{bot_koridlon_musicList}";
                        }

                        return returnText;
                    }


                }
                else
                {
                    return "Я ничего не играю";
                }
            }
            catch (Exception ex)
            {
                CommandManager.DebugError(ex);
                return $":no_entry:Ошибка!\t{ex.Message} {ex}";
            }
        }


        public static async Task<string> LeaveAsync(IGuild guild, SocketGuildUser user)
        {
            try
            {
                if (user.VoiceState == null) return ":information_source: Ты должен быть в голосовом канале";
                var player = _lavaNode.GetPlayer(guild);
                if (player.PlayerState is PlayerState.Playing) await player.StopAsync();
                await _lavaNode.LeaveAsync(player.VoiceChannel);

                if (isLooped.ContainsKey(guild.Id))
                isLooped.Remove(guild.Id);
                if (isLoopedQueue.ContainsKey(guild.Id))
                isLoopedQueue.Remove(guild.Id);

                try
                {
                    CommandManager.Debug("requested disconnect");
                    if (_disconnectTokens.TryGetValue(user.VoiceChannel.Id, out var value))
                    {
                        if (!value.IsCancellationRequested)
                        {
                            value.Cancel(true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CommandManager.DebugError(ex.Message);
                }

                Console.WriteLine($"[{DateTime.Now}]\t(AUDIO)\t\tLeaved from vc {guild}");
                return ":information_source:Я покидаю ваш голосовой канал(";
            }
            catch (InvalidOperationException ex)
            {
                return $":no_entry:Ошибка!\t{ex.Message}";
            }
        }
        //public async Task TrackEnded(TrackEndedEventArgs args)
        //{
        //    if (args.Reason != TrackEndReason.Finished || args.Player.PlayerState != PlayerState.Stopped || args.Track == null)
        //    {
        //        return;
        //    }

        //    if (!args.Player.Queue.TryDequeue(out var queueable))
        //    {
        //        //await args.Player.TextChannel.SendMessageAsync("Playback Finished.");
        //        return;
        //    }

        //    if (!(queueable is LavaTrack track))
        //    {
        //        await args.Player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
        //        return;
        //    }

        //    await args.Player.PlayAsync(track);
        //    await args.Player.TextChannel.SendMessageAsync();
        //}
        public async static Task OnTrackEnded(TrackEndedEventArgs args)
        {
            var previousTrack = args.Track;

            if (args.Reason != TrackEndReason.Finished || args.Player.PlayerState != PlayerState.Stopped || args.Track == null)
            {
                return;
            }
            if (isLooped.ContainsKey(args.Player.TextChannel.GuildId))
            {
                if (isLooped[args.Player.TextChannel.GuildId])
                {
                    CommandManager.Debug("Loop is active");
                    await args.Player.PlayAsync(previousTrack);
                    return;
                }
            }
            if(isLoopedQueue.ContainsKey(args.Player.TextChannel.GuildId))
            {
                if (isLoopedQueue[args.Player.TextChannel.GuildId])
                {
                    CommandManager.Debug("Looping queue is active");
                    args.Player.Queue.Enqueue(previousTrack);
                }
            }
            if (!args.Player.Queue.TryDequeue(out var queueable))
            {
                //await args.Player.TextChannel.SendMessageAsync("Queue completed! Please add more tracks to rock n' roll!");
                await (_ = InitiateDisconnectAsync(args.Player, TimeSpan.FromMinutes(5)));
                return;

            }

            if (!(queueable is LavaTrack track))
            {
                await args.Player.TextChannel.SendMessageAsync(":no_entry:Это не трек, я не могу это играть!");
                return;
            }

            await args.Player.PlayAsync(track);
            var cachedMsg = await args.Player.TextChannel.GetMessagesAsync(1).FlattenAsync();
            var listmsg = cachedMsg.ToList();
            var msg = listmsg[0] as SocketUserMessage;
            //CommandManager.Debug(msg.Content);
            try
            {
                if(isLoopedQueue.ContainsKey(args.Player.TextChannel.GuildId) == true)
                {
                    if (args.Player.Queue.Count > 5 && isLoopedQueue[args.Player.TextChannel.GuildId] == true)
                        await args.Player.TextChannel.SendMessageAsync($":notes:Сейчас играет: `{track.Title} - {track.Author}` [{track.Duration}]");
                }
                else
                {
                    await args.Player.TextChannel.SendMessageAsync($":notes:Сейчас играет: `{track.Title} - {track.Author}` [{track.Duration}]");
                }
            }
            catch (Exception ex)
            {
                CommandManager.DebugError(ex.Message);
            }            
        }
        public static async Task OnPlayerDestroyed(WebSocketClosedEventArgs arg)
        {
            CommandManager.Debug("Player must be destroyed");
        }

        //public static async Task OnVoiceUpdate(SocketVoiceServer args)
        //{
        //    var cacheGuild = args.Guild;
        //    var guild = cacheGuild.GetOrDownloadAsync().Result;
        //    var player = _lavaNode.GetPlayer(guild);

        //    await player.PauseAsync();
        //    return;
        //}
        public static async Task OnTrackStarted(TrackStartEventArgs arg)
        {
            try
            {
                CommandManager.Debug("track started");
                if (!_disconnectTokens.TryGetValue(arg.Player.VoiceChannel.Id, out var value))
                {
                    return;
                }

                if (value.IsCancellationRequested)
                {
                    return;
                }

                value.Cancel(true);
            }
            catch(Exception ex)
            {
                CommandManager.DebugError(ex.Message);
            }
        }
        public static async Task OnTrackException(TrackExceptionEventArgs arg)
        {
            CommandManager.DebugError(arg.Exception.Message + "|" + arg.Exception.Cause + "|" + arg.Exception.Severity);
        }
        public static async Task OnTrackStuck(TrackStuckEventArgs args)
        {
            CommandManager.DebugError(args.Threshold);
        }
        //public static Task OnPlayerUpdated(PlayerUpdateEventArgs arg)
        //{
            
        //}

        private static async Task InitiateDisconnectAsync(LavaPlayer player, TimeSpan timeSpan)
        {
            if (!_disconnectTokens.TryGetValue(player.VoiceChannel.Id, out var value))
            {
                value = new CancellationTokenSource();
                _disconnectTokens.TryAdd(player.VoiceChannel.Id, value);
            }
            else if (value.IsCancellationRequested)
            {
                _disconnectTokens.TryUpdate(player.VoiceChannel.Id, new CancellationTokenSource(), value);
                value = _disconnectTokens[player.VoiceChannel.Id];
            }

            //CommandManager.Debug($" {timeSpan}...");
            var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
            if (isCancelled)
            {
                return;
            }

            await _lavaNode.LeaveAsync(player.VoiceChannel);
            await player.TextChannel.SendMessageAsync($":information_source: Ухожу из вашего гк потому что 5 минут ничего не играл");
        }
        //public static async Task OnPlayerDestroyed()
        //{
        //    try
        //    {

        //    }
        //    catch (Exception ex)
        //    {
        //        ServiceManager.Debug($"OnPlayerDestroyed Error! {ex.Message}");
        //    }
        //}
        public static async Task<string> LoopTrackAsync(IGuild guild, SocketGuildUser user)
        {
            try
            {
                if (user.VoiceChannel == null) return ":information_source: Ты должен быть в голосовом канале";
                ulong gId = guild.Id;
                if (isLoopedQueue.ContainsKey(gId))
                {
                    if (isLoopedQueue[gId])
                    {
                        isLoopedQueue[gId] = false;
                    }
                }
                if (!isLooped.ContainsKey(gId))
                {
                    isLooped.Add(gId,true);
                    return ":white_check_mark: Повтор трека включён";
                }
                else
                {
                    if (isLooped[gId])
                    {
                        isLooped[gId] = false;
                        return ":white_check_mark: Повтор трека отключён";
                    }
                    else
                    {
                        isLooped[gId] = true;
                        return ":white_check_mark: Повтор трека включён";
                    }
                    return ":no_entry: Случилась непредвиденная ошибка :/";
                }
            }
            catch (Exception ex)
            {
                CommandManager.DebugError(ex.ToString());
                return $":no_entry: Ошибка! \t {ex.Message}";
            }       
        }
        public static async Task<string> LoopQueueAsync(IGuild guild, SocketGuildUser user)
        {
            try
            {
                if (user.VoiceChannel == null) return ":information_source: Ты должен быть в голосовом канале";
                ulong gId = guild.Id;
                var player = _lavaNode.GetPlayer(guild);
                if (isLooped.ContainsKey(gId))
                {
                    if (isLooped[gId])
                    {
                        isLooped[gId] = false;
                    }
                }
                if (!isLoopedQueue.ContainsKey(gId))
                {
                    if (player.Queue.ToList().Count == 0)
                        return ":information_source: Для того чтобы зацыклить очередь тебе нужно создать её. Добавь ещё треков";
                    isLoopedQueue.Add(gId, true);
                    return ":white_check_mark: Повтор очереди включён";
                }
                else
                {
                    if (isLoopedQueue[gId])
                    {
                        isLoopedQueue[gId] = false;
                        return ":white_check_mark: Повтор очереди отключён";
                    }
                    else
                    {
                        if (player.Queue == null)
                            return ":information_source: Для того чтобы зацыклить очередь тебе нужно создать её. Добавь ещё треков";
                        isLoopedQueue[gId] = true;
                        return ":white_check_mark: Повтор очереди включён";
                    }
                    return ":no_entry: Случилась непредвиденная ошибка :/";
                }
            }
            catch (Exception ex)
            {
                CommandManager.DebugError(ex.ToString());
                return $":no_entry: Ошибка! \t {ex.Message}";
            }
        }
        public static async Task<string> ShuffleQueueAsync(IGuild guild, SocketGuildUser user)
        {
            if (user.VoiceChannel == null) return ":information_source: Ты должен быть в голосовом канале";
            var player = _lavaNode.GetPlayer(guild);
            if (player == null)
                return ":information_source: Похоже, что я ничего не играю";
            if (player.Queue.Count <= 1)
                return $":no_entry: В очереди нет достаточно треков для перемешивания";
            else
            {
                player.Queue.Shuffle();
                return ":white_check_mark: Треки перемешаны";
            }
        }
        public static async Task<string> RemoveTrackQueueAsync(IGuild guild, int count, SocketGuildUser user)
        {
            if (user.VoiceChannel == null) return ":information_source: Ты должен быть в голосовом канале";
            var player = _lavaNode.GetPlayer(guild);
            var queue = player.Queue;
            if (player == null)
                return ":information_source: Похоже, что я ничего не играю";
            if (player.Queue.Count < count)
                return ":no_entry: Номер трека больше чем треков в очереди";
                queue.RemoveAt(count);
            return ":white_check_mark: Готово!";

        }
        public static async Task<string> RemoveTracksAsync(IGuild guild, int startIndex, SocketGuildUser user, int amount = 0)
        {
            if (user.VoiceChannel == null) return ":information_source: Ты должен быть в голосовом канале";
            var player = _lavaNode.GetPlayer(guild);
            var queue = player.Queue;
            if (player == null)
                return ":information_source: Похоже, что я ничего не играю";
            if (player.Queue.Count < startIndex)
                return ":no_entry: Число начала отсчёта больше чем треков в очереди";
            if (player.Queue.Count < amount + startIndex)
                return ":no_entry: Количество треков на удаление превышает количество треков в очереди";
            if (amount == 0)
                amount = queue.Count-startIndex;

            queue.RemoveRange(startIndex, amount);
            return ":white_check_mark: Готово!";
        }
        public static async Task<string> ReconectPlayerAsync(IGuild guild, SocketGuildUser user)
        {
            if (user.VoiceChannel == null) return ":information_source: Ты должен быть в голосовом канале";
            var player = _lavaNode.GetPlayer(guild);
            if (player == null)
                return ":information_source: Похоже я просто не был подключён";
            var queue = player.Queue.ToList();
            var track = player.Track;
            await _lavaNode.LeaveAsync(user.VoiceChannel);
            await _lavaNode.JoinAsync(user.VoiceChannel);
            if (queue == null)
            {
                await player.PlayAsync(track);
                return ":white_check_mark: Готово!";
            }
            else
            {
                await player.PlayAsync(track);
                try
                {
                    foreach (var qTrack in queue)
                    {
                        player.Queue.Enqueue(qTrack);
                    }
                    return ":information_source: Очередь восстановлена";
                }
                catch (Exception ex)
                {
                    return $":no_entry: хм... {ex.Message}";
                }
            }
        }
        public static async Task<string> SkipTrackAsync(IGuild guild, int count, SocketGuildUser user)
        {
            try
            {
                if (user.VoiceChannel == null) return ":information_source: Ты должен быть в голосовом канале";
                var player = _lavaNode.GetPlayer(guild);
                if (player is null)
                    return ":no_entry: Сейчас я ничем не занят";
                if (player.Queue.Count < 1)
                {
                    return $":no_entry: Оу, я не могу пропустить этот трек так как это либо последний трек в очереди либо сейчас вообще ничего не играет.";
                }
                else
                {
                    try
                    {
                        if(count <= 1)
                        {
                            var currentTrack = player.Track;

                            await player.SkipAsync();

                            Console.WriteLine($"{DateTime.Now}\t(Audio)\tSkipped {currentTrack.Title} in {guild.Name}");
                            return $":white_check_mark:Я успешно пропустил `{currentTrack.Title}`!\n:notes:Сейчас играет: `{player.Track.Author} - {player.Track.Title}`";

                        }
                        else
                        {
                            var queue = player.Queue;
                            if (queue.Count < count) return ":information_source: Я не смогу пропустить столько треков, в очереди их меньше";
                            queue.RemoveRange(0, count-1);
                            queue.TryDequeue(out var track);
                            await player.PlayAsync(track);
                            return $":white_check_mark: Я успешно пропустил {count} треков!\n:notes:Сейчас играет: `{player.Track.Author} - {player.Track.Title}`";
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        return $":no_entry:Ошибка!\t{ex.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                return $":no_entry:Ошибка!\t{ex.Message}";
            }
        }

        //public static async Task StartLava()
        //{
        //    try
        //    {
        //        Console.ForegroundColor = ConsoleColor.Green;
        //        Console.WriteLine($"[{DateTime.Now}]\t(Audio)\t\tStarting Lavalink");
        //        Console.ForegroundColor = ConsoleColor.White;
        //        string strCmdText;
        //        strCmdText = @"cd C:\Users\nonam\source\repos\Bot slit noob experimental\Bot slit noob experimental" +"\n" + "java -Djdk.tls.client.protocols=TLSv1.2 -jar lavalink.jar";
        //        Process p = new Process();
        //        ProcessStartInfo info = new ProcessStartInfo();
        //        info.FileName = "cmd.exe";
        //        info.RedirectStandardInput = true;
        //        info.UseShellExecute = false;

        //        p.StartInfo = info;
        //        p.Start();

        //        using (StreamWriter sw = p.StandardInput)
        //        {
        //            if (sw.BaseStream.CanWrite)
        //            {
        //                sw.WriteLine(@"cd C: \Users\nonam\source\repos\Bot slit noob experimental\Bot slit noob experimental");
        //                sw.WriteLine("java -Djdk.tls.client.protocols=TLSv1.2 -jar lavalink.jar");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Oops, error\t\t{ex.Message}");
        //    }
        //}
    }
}
