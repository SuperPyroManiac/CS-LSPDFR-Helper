using System.Reflection;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using ULSS_Helper.Events;
using ULSS_Helper.Objects;
using ULSS_Helper.Messages;

namespace ULSS_Helper;

internal class Program
{
    internal static DiscordClient Client {get; set;}
    internal static Settings Settings;
    internal static Cache Cache = new();
    public static string PlugName;
    public static string ErrId;
    public static State PlugState;
    public static Level ErrLevel;
    
    static async Task Main()
    {
        Timer.StartTimer();

        Settings = new Settings();
        
        var discordConfig = new DiscordConfiguration
        {
            Intents = DiscordIntents.All,
            Token = Settings.Env.BotToken,
            TokenType = TokenType.Bot,
            AutoReconnect = true
        };
        Client = new DiscordClient(discordConfig);
        
        var sCommands = Client.UseSlashCommands();

        sCommands.RegisterCommands(Assembly.GetExecutingAssembly(), Settings.Env.ServerId);
        sCommands.RegisterCommands<ContextMenu>(Settings.Env.ServerId);

        Client.ModalSubmitted += ModalSubmit.HandleModalSubmit;
        Client.ComponentInteractionCreated += ComponentInteraction.HandleInteraction;
        //Client.MessageCreated += MessageSent;
        //Client.VoiceStateUpdated += VoiceChatManager.OnMemberJoinLeaveVC;

        Client.UseInteractivity(new InteractivityConfiguration());

        await Client.ConnectAsync();
        StatusMessages.SendStartupMessage();
        await Task.Delay(-1);
    }
//    private static async Task MessageSent(DiscordClient s, MessageCreateEventArgs ctx)
//    {
//        if (Settings.Env.BullyingVictims.Any(victimId => victimId == ctx.Author.Id))
//        {
//            var rNd = new Random().Next(4);
//            if (rNd == 1) await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":tarabruh:"));
//            if (rNd == 2) await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":middle_finger:"));
//            if (rNd == 0)
//            {
//                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":tarabruh:"));
//                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":middle_finger:"));
//            }
//        }
//    }
}
