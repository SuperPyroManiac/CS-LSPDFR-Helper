using System.Reflection;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Modules;

namespace ULSS_Helper;

internal class Program
{
    public static string? PlugName;
    public static string? ErrId;
    public static State PlugState;
    public static Level ErrLevel;
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

        sCommands.RegisterCommands(Assembly.GetExecutingAssembly(), Settings.GetServerID());
        sCommands.RegisterCommands<ContextManager>(Settings.GetServerID());

        Client.ModalSubmitted += ModalManager.PluginModal;
        Client.ComponentInteractionCreated += ContextManager.OnButtonPress;
        Client.MessageCreated += MessageSent;
        //TODO: Client.VoiceStateUpdated += VoiceChatManager.OnMemberJoinLeaveVC;

        await Client.ConnectAsync();
        await Task.Delay(-1);
    }
    private static async Task MessageSent(DiscordClient s, MessageCreateEventArgs ctx)
    {
        if (ctx.Author.Id == 478591527321337857 || ctx.Author.Id == 614191277528973428)
        {
            var rNd = new Random().Next(4);
            if (rNd == 1) await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":tarabruh:"));
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
    public string? ID { get; set; }
    public string Regex { get; set; }
    public string Solution { get; set; }
    public string Level { get; set; }
}

public class AnalyzedLog
{
    public List<Plugin?> Current { get; set; }
    public List<Plugin?> Outdated { get; set; }
    public List<Plugin?> Broken { get; set; }
    public List<Plugin?> Library { get; set; }
    public List<Plugin?> Missing { get; set; }
    
    public List<Error?> Errors { get; set; }

    public string GTAVersion { get; set; }
    public string RPHVersion { get; set; }
    public string LSPDFRVersion { get; set; }
}

public enum State
{
    [ChoiceName("LSPDFR")]
    LSPDFR,
    [ChoiceName("EXTERNAL")]
    EXTERNAL,
    [ChoiceName("BROKEN")]
    BROKEN,
    [ChoiceName("LIB")]
    LIB
}
public enum Level
{
    [ChoiceName("WARN")]
    WARN,
    [ChoiceName("SEVERE")]
    SEVERE
}