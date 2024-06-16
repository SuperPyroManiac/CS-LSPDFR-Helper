using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using LSPDFR_Helper.CustomTypes.CacheTypes;
using LSPDFR_Helper.EventManagers;
using LSPDFR_Helper.Functions;
using LSPDFR_Helper.Functions.Messages;

namespace LSPDFR_Helper.Commands.User;

public class EditUser
{
    [Command("edituser")]
    [Description("Edits a user!")]
    public async Task EditUserCmd(SlashCommandContext ctx, [Description("User to edit!")] DiscordUser user)
    {
        if (!await PermissionManager.RequireBotAdmin(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        if (Program.Cache.GetUsers().All(x => x.UID.ToString() != user.Id.ToString()))
        {
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                bd.AddEmbed(BasicEmbeds.Error("__User is not in the DB!__", true)));
            return;
        }
        
        var dUser = Program.Cache.GetUser(user.Id);
        var tempus = await ctx.Guild!.GetMemberAsync(dUser.UID);
        dUser!.Username = tempus.Username;
        
        DiscordInteractionResponseBuilder modal = new();
        modal.WithCustomId(CustomIds.SelectUserValueToEdit);
        modal.WithTitle($"Editing {dUser.Username}!");
        modal.AddComponents(new DiscordTextInputComponent(
            label: "Editor:", 
            customId: "userEditor", 
            required: false,
            style: DiscordTextInputStyle.Short, 
            value: dUser.BotEditor.ToString()
        ));
        modal.AddComponents(new DiscordTextInputComponent(
            label: "BotAdmin:", 
            customId: "userBotAdmin", 
            required: false,
            style: DiscordTextInputStyle.Short, 
            value: dUser.BotAdmin.ToString()
        ));
        modal.AddComponents(new DiscordTextInputComponent(
            label: "Blacklisted:", 
            customId: "userBlacklist", 
            required: false, 
            style: DiscordTextInputStyle.Short, 
            value: dUser.Blocked.ToString()
        ));
        
        Program.Cache.SaveUserAction(ctx.Interaction.User.Id, modal.CustomId, new InteractionCache(ctx.Interaction, dUser));
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
    }
}