using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;
using DiscordUser = DSharpPlus.Entities.DiscordUser;

namespace ULSS_Helper.Commands.User;

public class EditUser : ApplicationCommandModule
{
    [SlashCommand("EditUser", "Edits a user!")]
    [RequireBotAdmin]
    public async Task EditUserCmd(InteractionContext ctx, 
        [Option("User", "User to edit!")] DiscordUser userId)
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        if (Database.LoadUsers().All(x => x.UID.ToString() != userId.Id.ToString()))
        {
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error("User is not in the DB!")));
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