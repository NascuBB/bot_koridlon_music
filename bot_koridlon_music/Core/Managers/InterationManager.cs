using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace bot_koridlon_music.Core.Managers
{
    public class InterationManager
    {
        public static async Task MyButtonHandler(SocketMessageComponent component)
        {
            try
            {
                DateTime yesterday = DateTime.UtcNow.AddDays(-1);
                CommandManager.Debug(yesterday);
                if(component.Message.CreatedAt <= yesterday)
                {
                    component.RespondAsync(":information_source: Это сообщение слишком старое для нормального ответа. Лучше запроси новую помощь",ephemeral:true);
                    return;
                }
                // We can now check for our custom id
                switch (component.Data.CustomId)
                {
                    // Since we set our buttons custom id as 'custom-id', we can check for it like this:
                    case "nextHelp":
                        // Lets respond by sending a message saying they clicked the button
                        await component.DeferAsync();
                        await component.Message.ModifyAsync(async x => 
                        { 
                            x.Embed = await HelpVariations.helpsAsync(2, (component.Message.Channel as SocketGuildChannel).Guild);
                            x.Components = new ComponentBuilder().WithButton("←", "previousHelpPage2").WithButton("→", "nextHelpPage2").WithButton("🗑", "delete", ButtonStyle.Danger).Build(); 
                        });
                        break;
                    case "nextHelpPage2":
                        await component.DeferAsync();
                        await component.Message.ModifyAsync(async x => 
                        { 
                            x.Embed = await HelpVariations.helpsAsync(3, (component.Message.Channel as SocketGuildChannel).Guild);
                            x.Components = new ComponentBuilder().WithButton("←", "previousHelpPage3").WithButton("→", "nextHelpPage3").WithButton("🗑", "delete", ButtonStyle.Danger).Build(); 
                        });
                        break;
                    case "nextHelpPage3":
                        await component.DeferAsync();
                        await component.Message.ModifyAsync(async x =>
                        {
                            x.Embed = await HelpVariations.helpsAsync(4, (component.Message.Channel as SocketGuildChannel).Guild);
                            x.Components = new ComponentBuilder().WithButton("←", "previousHelpPage4").WithButton("→", "disabled", disabled: true).WithButton("🗑", "delete", ButtonStyle.Danger).Build();
                        });
                        break;
                    case "previousHelpPage4":
                        await component.DeferAsync();
                        await component.Message.ModifyAsync(async x =>
                        {
                            x.Embed = await HelpVariations.helpsAsync(3, (component.Message.Channel as SocketGuildChannel).Guild);
                            x.Components = new ComponentBuilder().WithButton("←", "previousHelpPage3").WithButton("→", "nextHelpPage3").WithButton("🗑", "delete", ButtonStyle.Danger).Build();
                        });
                        break;

                    case "previousHelpPage3":
                        await component.DeferAsync();
                        await component.Message.ModifyAsync(async x => 
                        { 
                            x.Embed = await HelpVariations.helpsAsync(2, (component.Message.Channel as SocketGuildChannel).Guild);
                            x.Components = new ComponentBuilder().WithButton("←", "previousHelpPage2").WithButton("→", "nextHelpPage2").WithButton("🗑", "delete", ButtonStyle.Danger).Build();
                        });
                        break;
                    case "previousHelpPage2":
                        await component.DeferAsync();
                        await component.Message.ModifyAsync(async x =>
                        {
                            x.Embed = await HelpVariations.helpsAsync(1, (component.Message.Channel as SocketGuildChannel).Guild);
                            x.Components = new ComponentBuilder().WithButton("←", "disabled", disabled: true).WithButton("→", "nextHelp").WithButton("🗑", "delete", ButtonStyle.Danger).Build();
                        });
                        break;
                    case "delete":
                        await component.DeferAsync();
                        await component.Message.DeleteAsync();
                        break;
                }
            }
            catch(Exception ex)
            {
                CommandManager.DebugError(ex.ToString());
            }
        }
    }
}
