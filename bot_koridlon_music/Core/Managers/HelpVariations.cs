using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using ShablePrefics;

namespace bot_koridlon_music.Core.Managers
{
    public class HelpVariations
    {
        
        private static string _prefix = ConfigManager.Config.Prefix; 
        public static async Task<Embed> helpsAsync(int page, IGuild guild)
        {
            string prefix = await Prefix.GetPrefix(guild, _prefix);
            switch(page)
            {
                case 1:
                    Embed helpPageOne = new EmbedBuilder().WithAuthor("Справка по командам").WithColor(Color.LightOrange).WithDescription($"`{prefix}join` **- команда для подключения к каналу, в котором вы находитесь**\n`{prefix}play` **- команда для воспроизведения музыки(введите rm для воспроизведения случайного трека из базы данных бота)**\n`{prefix}leave` **- команда для отключения бота от голосового канала**\n`{prefix}stop` **- команда для остановки воспроизведения**\n`{prefix}pause` **- команда для приостановки воспроизведения**").WithTitle(":book:Все команды, которые знает бот").WithFooter("страница 1").Build();
                    return helpPageOne;
                case 2:
                    Embed helpPageTwo = new EmbedBuilder().WithAuthor("Справка по командам").WithColor(Color.LightOrange).WithDescription($"`{prefix}list` **- команда для отображения очереди треков**\n`{prefix}skip` **- команда чтобы пропустить трек**\n`{prefix}skipto` **- команда для пропуска нескольких треков в очереди**\n`{prefix}shuffle` **- команда для перемешивания очереди**\n`{prefix}volume` **- команда для установки громкости трека**").WithTitle(":book:Все команды, которые знает бот").WithFooter("страница 2").Build();
                    return helpPageTwo;
                case 3:
                    Embed helpPageThree = new EmbedBuilder().WithAuthor("Справка по командам").WithColor(Color.LightOrange).WithDescription($"`{prefix}seek` **- команда для поиска времени на треке (Пример: {prefix}seek 1 23)**\n`{prefix}lyrics` **- команда для поиска текста к воспроизводимой в данный момент дорожке**\n`{prefix}loop` **- команда для включения/отключения повтора трека**\n`{prefix}qloop` **- комманда для включения/отключения повтора очереди**").WithTitle(":book:Все команды, которые знает бот").WithFooter("страница 3").Build();
                    return helpPageThree;
                case 4:
                    Embed helpPageFour = new EmbedBuilder().WithAuthor("Справка по командам").WithColor(Color.LightOrange).WithDescription($"`{prefix}removetrack` **- команда для удаления определённого трека из очереди**\n`{prefix}removefew` **- команда для удаления нескольких треков из очереди (если количество треков для удаления 0, то очередь очистится от заданого трека до конца)**\n`{prefix}ping` **- команда для показания задержки бота**\n`{prefix}setprefix` **- команда для задания префикса для данного сервера**\n`{prefix}defaultprefix` **- команда для сброса префикса этого сервера (по умолчанию `>`)**").WithTitle(":book:Все команды, которые знает бот").WithFooter("страница 4").Build();
                    return helpPageFour;
            }
            return null;
        }
        
    }
}
