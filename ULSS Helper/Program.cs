using System.Reflection;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Modules;

namespace ULSS_Helper;

internal class Program
{
    internal static DiscordClient Client {get; set;}
    internal static CommandsNextExtension Commands {get; set;}
    
    static async Task Main(string[] args)
    {
        var discordConfig = new DiscordConfiguration()
        {
            Intents = DiscordIntents.All,
            Token = Settings.Token,
            TokenType = TokenType.Bot,
            AutoReconnect = true
        };
        Client = new DiscordClient(discordConfig);

        var sCommands = Client.UseSlashCommands();
        
        sCommands.RegisterCommands<ContextManager>(449706194140135444);
        sCommands.RegisterCommands<CommandManager>(449706194140135444);

        Client.MessageCreated += JarJarBinks;
        Client.ModalSubmitted += CommandManager.PluginModal;
        Client.ComponentInteractionCreated += ContextManager.OnButtonPress;

        await Client.ConnectAsync();
        await Task.Delay(-1);
    }

    private static async Task JarJarBinks(DiscordClient s, MessageCreateEventArgs ctx)
    {
        if (ctx.Author.Id == 478591527321337857 || ctx.Author.Id == 614191277528973428)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":tarabruh:"));
        }
    }
}

public class Plugin
{
    public string? Name { get; set; }
    public string? DName { get; set; }
    public string? Version { get; set; }
    public string? EAVersion { get; set; }
    public string? ID { get; set; }
    public string? State { get; set; }
    public string? Link { get; set; }
}

public class Error
{
    public string ID { get; set; }
    public string Regex { get; set; }
    public string Solution { get; set; }
    public string Level { get; set; }
}