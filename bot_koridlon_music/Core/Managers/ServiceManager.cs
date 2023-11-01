using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace bot_koridlon_music.Core.Managers
{
    public static class ServiceManager
    {
        public static IServiceProvider Provider { get; private set; }

        public static void SetProvider(ServiceCollection collection)
            => Provider = collection.BuildServiceProvider();
        public static T GetService<T>() where T : new() => Provider.GetRequiredService<T>();
    }
}
