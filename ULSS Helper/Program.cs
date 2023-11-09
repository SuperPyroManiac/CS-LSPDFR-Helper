using System.Reflection;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Modules;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using ULSS_Helper.Events;
using ULSS_Helper.Objects;
using Timer = ULSS_Helper.Timer;

namespace ULSS_Helper;

internal class Program
{
    public static string? PlugName;
    public static string? ErrId;
    public static State PlugState;
    public static Level ErrLevel;
    internal static DiscordClient Client {get; set;}
    internal static CommandsNextExtension Commands {get; set;}
    internal static Cache Cache;
    
    static async Task Main(string[] args)
    {
        Timer.StartTimer();
        
        var discordConfig = new DiscordConfiguration()
        {
            Intents = DiscordIntents.All,
            Token = Settings.Token,
            TokenType = TokenType.Bot,
            AutoReconnect = true
        };
        Client = new DiscordClient(discordConfig);

        Cache = new Cache();

        var sCommands = Client.UseSlashCommands();

        sCommands.RegisterCommands(Assembly.GetExecutingAssembly(), Settings.GetServerID());
        sCommands.RegisterCommands<ContextMenu>(Settings.GetServerID());

        Client.ModalSubmitted += ModalSubmit.HandleModalSubmit;
        Client.ComponentInteractionCreated += ButtonPress.OnButtonPress;
        Client.MessageCreated += MessageSent;
        //TODO: Client.VoiceStateUpdated += VoiceChatManager.OnMemberJoinLeaveVC;

        Client.UseInteractivity(new InteractivityConfiguration());

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
