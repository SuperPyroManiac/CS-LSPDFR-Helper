using System.Reflection;
using DSharpPlus;
using DSharpPlus.CommandsNext;
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
        
        sCommands.RegisterCommands(Assembly.GetExecutingAssembly(), 449706194140135444);
        sCommands.RegisterCommands<ContextManager>(449706194140135444);

        Client.ModalSubmitted += ModalManager.PluginModal;
        Client.ComponentInteractionCreated += ContextManager.OnButtonPress;

        await Client.ConnectAsync();
        await Task.Delay(-1);
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