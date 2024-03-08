using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class EditUser : ApplicationCommandModule
{
    [SlashCommand("ChangeErrorView", "Edits what you see in more details!")]
    [RequireTsRoleSlash]
    public async Task EditViewCmd(
        InteractionContext ctx, 
        [Option("View", "True shows XTRA errors, False does not.")] bool view)
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        if (Database.LoadUsers().All(ts => ts.UID.ToString() != ctx.Member.Id.ToString()))
        {
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error("You're not in the DB, this shouldnt be possible!")));
            return;
        }
        
        var ts = Database.LoadUsers().FirstOrDefault(x => x.UID.ToString() == ctx.Member.Id.ToString());
        ts!.Username = ctx.Guild.GetMemberAsync(ulong.Parse(ts.UID)).Result.Username;
        Database.EditUser(ts);
        
        await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Success($"<@{ctx.Member.Id.ToString()}>: You have changed your view type to: {(view ? "Show XTRA Errors" : "Hide XTRA Errors")}")));
        Logging.SendLog(
            ctx.Interaction.Channel.Id, 
            ctx.Interaction.User.Id,
            BasicEmbeds.Info($"**<@{ctx.Member.Id.ToString()}> has changed their view type to: {(view ? "Show XTRA errors" : "Hide XTRA Errors")}**")
        );
    }
    
    [SlashCommand("EditUser", "Edits a user!")]
    [RequireBotAdmin]
    public async Task EditUserCmd(
        InteractionContext ctx, 
        [Option("ID", "User ID to edit!")] string userId)
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        if (Database.LoadUsers().All(x => x.UID.ToString() != userId))
        {
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error("User is not in the DB!")));
            return;
        }

        var dUser = Database.LoadUsers().FirstOrDefault(x => x.UID.ToString() == userId);
        dUser!.Username = ctx.Guild.GetMemberAsync(ulong.Parse(dUser.UID)).Result.Username;
        
        DiscordInteractionResponseBuilder modal = new();
        modal.WithCustomId(ModalSubmit.EditUser);
        modal.WithTitle($"Editing {dUser.Username}!");
        modal.AddComponents(new TextInputComponent(
            label: "Editor:", 
            customId: "userEditor", 
            required: false,
            style: TextInputStyle.Short, 
            value: dUser.BotEditor.ToString()
        ));
        modal.AddComponents(new TextInputComponent(
            label: "BotAdmin:", 
            customId: "userBotAdmin", 
            required: false,
            style: TextInputStyle.Short, 
            value: dUser.BotAdmin.ToString()
        ));
        modal.AddComponents(new TextInputComponent(
            label: "Blacklisted:", 
            customId: "userBlacklist", 
            required: false, 
            style: TextInputStyle.Short, 
            value: dUser.Blocked.ToString()
        ));
        modal.AddComponents(new TextInputComponent(
            label: "Bully:", 
            customId: "userBully", 
            required: false, 
            style: TextInputStyle.Short, 
            value: dUser.Bully.ToString()
        ));
        
        Program.Cache.SaveUserAction(ctx.Interaction.User.Id, modal.CustomId, new UserActionCache(ctx.Interaction, dUser));
        await ctx.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    }
}