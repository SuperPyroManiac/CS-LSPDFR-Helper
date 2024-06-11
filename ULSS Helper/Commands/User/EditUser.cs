using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.Functions;
using ULSS_Helper.Objects;
using DiscordUser = DSharpPlus.Entities.DiscordUser;

namespace ULSS_Helper.Commands.User;

public class EditUser
{
    [Command("EditUser")]
    [Description("Edits a user!")]
    public async Task EditUserCmd(SlashCommandContext ctx, [Description("User to edit!")] DiscordUser userId)
    {
        if (!await PermissionManager.RequireBotAdmin(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        if (Database.LoadUsers().All(x => x.UID.ToString() != userId.Id.ToString()))
        {
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                bd.AddEmbed(BasicEmbeds.Error("__User is not in the DB!__", true)));
            return;
        }

        var dUser = Database.LoadUsers().FirstOrDefault(x => x.UID.ToString() == userId.Id.ToString());
        var tmpusr = await ctx.Guild.GetMemberAsync(ulong.Parse(dUser.UID));
        dUser!.Username = tmpusr.Username;
        
        DiscordInteractionResponseBuilder modal = new();
        modal.WithCustomId(ModalSubmit.EditUser);
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
        
        Program.Cache.SaveUserAction(ctx.Interaction.User.Id, modal.CustomId, new UserActionCache(ctx.Interaction, dUser));
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
    }
}